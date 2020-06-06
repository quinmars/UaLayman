using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class BrowseViewModel : RoutableViewModelBase
    {
        private readonly IChannelService _channelService;
        private readonly ReactiveCommand<Unit, IChangeSet<BrowseTreeNode, NodeId>> _browse;

        private readonly ReadOnlyObservableCollection<BrowseItemViewModel> _nodes;
        public ReadOnlyObservableCollection<BrowseItemViewModel> Nodes => _nodes;

        private ObservableAsPropertyHelper<bool> _isBrowsing;
        public bool IsBrowsing => _isBrowsing.Value;
        
        private ObservableAsPropertyHelper<string> _executionError;
        public string ExecutionError => _executionError.Value;

        private ObservableAsPropertyHelper<bool> _hasExecutionError;
        public bool HasExecutionError => _hasExecutionError.Value;

        private BrowseItemViewModel _selectedItem;
        public BrowseItemViewModel SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        private ObservableAsPropertyHelper<BaseNodeViewModel> _detailViewModel;
        public BaseNodeViewModel DetailViewModel => _detailViewModel.Value;

        public BrowseViewModel(IScreen screen, IChannelService channelService) : base(screen)
        {
            _channelService = channelService;

            _browse = ReactiveCommand.CreateFromObservable(
                () => _channelService
                    .Browse()
                    .Do(n => Debug.WriteLine($"count: {n.Length}"))
                    .ToObservableChangeSet(r => r.NodeId));

            _browse
                .TransformToTree(r => r.ParentId)
                .Transform(node => new BrowseItemViewModel(node, null, this))
                .Bind(out _nodes)
                .DisposeMany()
                .Subscribe();

            _browse.IsExecuting
                .ToProperty(this, x => x.IsBrowsing, out _isBrowsing);

            _channelService
                .State
                .Where(s => s == CommunicationState.Opened)
                .Select(_ => _browse
                            .Execute()
                            .Select(__ => Unit.Default)
                            .Catch(Observable.Return(Unit.Default)))
                .Switch()
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedItem)
                .Select(item => NodeViewModel.Create(item, _channelService))
                .DisposeLast()
                .ToProperty(this, x => x.DetailViewModel, out _detailViewModel);

            /*
             * The error message
             */
            var noerr = _browse.IsExecuting
                .Where(x => x)
                .Select(_ => "");

            var err = _browse.ThrownExceptions
                .Select(ex =>
                {
                    switch (ex)
                    {
                        case ServiceResultException e:
                            var sc = e.StatusCode;
                            return $"{StatusCodes.GetDefaultMessage(sc)} ({sc})";
                        default:
                            return ex.Message;
                    };
                });

            Observable.Merge(err, noerr)
                .ToProperty(this, x => x.ExecutionError, out _executionError, null);

            this.WhenAnyValue(x => x.ExecutionError)
                .Select(x => string.IsNullOrEmpty(x))
                .ToProperty(this, x => x.HasExecutionError, out _hasExecutionError, false);

        }
    }
}
