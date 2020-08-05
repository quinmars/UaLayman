using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UaLayman.Common;
using UaLayman.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace UaLayman.Views
{
    public sealed partial class MainView : Page, IViewFor<MainViewModel>, IThemeService
    {
        public ConnectionStateViewModel StateViewModel { get; }

        public MainView()
        {
            this.InitializeComponent();
            
            var channelService = Locator.Current.GetService<IChannelService>();
            var discoveryService = Locator.Current.GetService<IDiscoveryService>();

            ViewModel = new MainViewModel(discoveryService, channelService);
            StateViewModel = new ConnectionStateViewModel(channelService);

            // The settings
            var settingsViewModel = new SettingsViewModel(ViewModel, this);
            Locator.CurrentMutable.RegisterLazySingleton(() => settingsViewModel, typeof(IRoutableViewModel), "Settings");

            Window.Current.SetTitleBar(AppTitleBar);
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle(s);

            // remove the solid-colored backgrounds behind the caption controls and system back button
            // This is done when the app is loaded since before that the actual theme that is used is not "determined" yet
            Loaded += (sender, e) => UpdateTitleBar(true);

            NavigationViewControl.RegisterPropertyChangedCallback(Muxc.NavigationView.PaneDisplayModeProperty, new DependencyPropertyChangedCallback(OnPaneDisplayModeChanged));
            
            ViewModel.NavigateTo.Execute("Connection").Subscribe();
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainView), null);

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public void RequestTheme(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Light:
                    RequestedTheme = ElementTheme.Light;
                    break;
                case ThemeMode.Dark:
                    RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    RequestedTheme = ElementTheme.Default;
                    break;
            }
        }

        private void OnPaneDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var navigationView = sender as Muxc.NavigationView;
            //AppTitleBar.Visibility = navigationView.PaneDisplayMode == Muxc.NavigationViewPaneDisplayMode.Top ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Navigation_ItemInvoked(Muxc.NavigationView sender, Muxc.NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem is string tag)
            {
                ViewModel.NavigateTo.Execute(tag).Subscribe();
            }
            else if (args.IsSettingsInvoked)
            {
                ViewModel.NavigateTo.Execute("Settings").Subscribe();
            }
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            //ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void UpdateTitleBar(bool isLeftMode)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = isLeftMode;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (isLeftMode)
            {
                NavigationViewControl.PaneDisplayMode = Muxc.NavigationViewPaneDisplayMode.Auto;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                NavigationViewControl.PaneDisplayMode = Muxc.NavigationViewPaneDisplayMode.Top;
                var userSettings = new UISettings();
                titleBar.ButtonBackgroundColor = userSettings.GetColorValue(UIColorType.Accent);
                titleBar.ButtonInactiveBackgroundColor = userSettings.GetColorValue(UIColorType.Accent);
            }
        }

        private void Navigation_PaneClosing(Muxc.NavigationView sender, Muxc.NavigationViewPaneClosingEventArgs args)
            => UpdateAppTitleMargin(sender);

        private void Navigation_PaneOpened(Muxc.NavigationView sender, object args)
            => UpdateAppTitleMargin(sender);

        private void Navigation_DisplayModeChanged(Muxc.NavigationView sender, Muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }

            UpdateAppTitleMargin(sender);
        }

        private void UpdateAppTitleMargin(Muxc.NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                AppTitle.TranslationTransition = new Vector3Transition();

                if ((sender.DisplayMode == Muxc.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent, 0, 0);
                }
                else
                {
                    AppTitle.Translation = new System.Numerics.Vector3(largeLeftIndent, 0, 0);
                }
            }
            else
            {
                Thickness currMargin = AppTitle.Margin;

                if ((sender.DisplayMode == Muxc.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == Muxc.NavigationViewDisplayMode.Minimal)
                {
                    AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
                else
                {
                    AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                }
            }
        }

        public string GetAppTitleFromSystem()
        {
            return Windows.ApplicationModel.Package.Current.DisplayName;
        }

        private void Navigation_BackRequested(Muxc.NavigationView sender, Muxc.NavigationViewBackRequestedEventArgs args)
        {
            ViewModel.GoBack.Execute().Subscribe();
        }
    }
}
