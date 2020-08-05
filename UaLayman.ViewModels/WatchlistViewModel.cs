using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UaLayman.ViewModels
{
    public class WatchlistViewModel : RoutableViewModelBase
    {
        public ObservableCollection<WatchlistItemViewModel> Items { get; } = new ObservableCollection<WatchlistItemViewModel>();

        private ObservableAsPropertyHelper<bool> _isEmpty;
        public bool IsEmpty => _isEmpty.Value;

        public WatchlistViewModel(IScreen screen)
            : base(screen, "Watchlist")
        {
            Items
                .ToObservableChangeSet()
                .Count()
                .Select(c => c == 0)
                .ToProperty(this, x => x.IsEmpty, out _isEmpty, true);
        }
    }
}
