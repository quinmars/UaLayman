using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels
{
    public class RoutableViewModelBase : ReactiveObject, IRoutableViewModel
    {
        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        public ReactiveCommand<string, IRoutableViewModel> NavigateTo => (HostScreen as MainViewModel).NavigateTo;

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public RoutableViewModelBase(IScreen screen) => HostScreen = screen;
    }
}
