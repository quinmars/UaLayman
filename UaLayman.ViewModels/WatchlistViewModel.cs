using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels
{
    public class WatchlistViewModel : RoutableViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Watch { get; }
        public WatchlistViewModel(IScreen screen) : base(screen) { }
    }
}
