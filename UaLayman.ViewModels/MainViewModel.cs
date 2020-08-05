using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels
{
    public class MainViewModel : ReactiveObject, IScreen
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        // The command that navigates a user to first view model.
        public ReactiveCommand<string, IRoutableViewModel> NavigateTo { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, Unit> GoBack => Router.NavigateBack;

        private readonly ObservableAsPropertyHelper<bool> _CanGoBack;
        public bool CanGoBack => _CanGoBack.Value;

        private readonly ObservableAsPropertyHelper<bool> _isOnConnectionView;
        public bool IsOnConnectionView => _isOnConnectionView.Value;

        private readonly ObservableAsPropertyHelper<bool> _isOnBrowseView;
        public bool IsOnBrowseView => _isOnBrowseView.Value;

        private readonly ObservableAsPropertyHelper<bool> _isOnWatchlistView;
        public bool IsOnWatchlistView => _isOnWatchlistView.Value;

        private readonly ObservableAsPropertyHelper<string> _headerText;
        public string HeaderText => _headerText.Value;

        public MainViewModel(IDiscoveryService discoveryService, IChannelService channelService)
        {
            var connectionViewModel = new ConnectionViewModel(this, channelService, discoveryService);
            var watchListViewModel = new WatchlistViewModel(this);
            var browseViewModel = new BrowseViewModel(this, channelService, watchListViewModel.Items);

            Locator.CurrentMutable.RegisterLazySingleton(() => connectionViewModel, typeof(IRoutableViewModel), "Connection");
            Locator.CurrentMutable.RegisterLazySingleton(() => browseViewModel, typeof(IRoutableViewModel), "Browse");
            Locator.CurrentMutable.RegisterLazySingleton(() => watchListViewModel, typeof(IRoutableViewModel), "Watchlist");

            NavigateTo = ReactiveCommand.CreateFromObservable(
                (string tag) =>
                {
                    var vm = Locator.Current.GetService<IRoutableViewModel>(tag);
                    return Router.Navigate.Execute(vm);
                }
            );

            Router.CurrentViewModel
                .Select(x => x is ConnectionViewModel)
                .ToProperty(this, x => x.IsOnConnectionView, out _isOnConnectionView, false);

            Router.CurrentViewModel
                .Select(x => x is BrowseViewModel)
                .ToProperty(this, x => x.IsOnBrowseView, out _isOnBrowseView, false);

            Router.CurrentViewModel
                .Select(x => x is WatchlistViewModel)
                .ToProperty(this, x => x.IsOnWatchlistView, out _isOnWatchlistView, false);

            Router.CurrentViewModel
                .Select(x =>
                {
                    if (x is RoutableViewModelBase vm)
                    {
                        return vm.HeaderText;
                    }
                    return null;
                })
                .ToProperty(this, x => x.HeaderText, out _headerText);

            GoBack.CanExecute
                .ToProperty(this, x => x.CanGoBack, out _CanGoBack, false);
        }
    }
}
