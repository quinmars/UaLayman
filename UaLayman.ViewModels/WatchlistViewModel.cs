using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels
{
    public class WatchlistViewModel : RoutableViewModelBase
    {
        public ObservableCollection<WatchlistItemViewModel> Items { get; } = new ObservableCollection<WatchlistItemViewModel>();

        public WatchlistViewModel(IScreen screen) : base(screen)
        {
        }
    }
}
