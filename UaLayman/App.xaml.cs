using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UaLayman.Services;
using UaLayman.ViewModels;
using UaLayman.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UaLayman
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            Akavache.Registrations.Start("UaLayman");

            var discoveryService = new DiscoveryService();
            var channelService = new ChannelService();

            // The services
            Locator.CurrentMutable.Register(() => discoveryService, typeof(IDiscoveryService));
            Locator.CurrentMutable.Register(() => channelService, typeof(IChannelService));


            // The views
            Locator.CurrentMutable.Register(() => new ConnectionView(), typeof(IViewFor<ConnectionViewModel>));
            Locator.CurrentMutable.Register(() => new BrowseView(), typeof(IViewFor<BrowseViewModel>));
            Locator.CurrentMutable.Register(() => new WatchlistView(), typeof(IViewFor<WatchlistViewModel>));
            Locator.CurrentMutable.Register(() => new SettingsView(), typeof(IViewFor<SettingsViewModel>));

        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO load state
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated != false)
                return;
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainView), e.Arguments);
            Window.Current.Activate();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}
