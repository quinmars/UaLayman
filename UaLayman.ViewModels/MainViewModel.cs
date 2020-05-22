using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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

        public MainViewModel(IDiscoveryService discoveryService, IChannelService channelService)
        {
            var connectionViewModel = new ConnectionViewModel(this, channelService, discoveryService);
            var browseViewModel = new BrowseViewModel(this, channelService);
            var watchListViewModel = new WatchlistViewModel(this);

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
        }
    }
}
