using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class BrowseItemViewModel : ReactiveObject, IDisposable
    {
        private readonly IDisposable _cleanUp;

        public string DisplayText => BrowseTreeNode.ReferenceDescription.BrowseName.Name;
        public BrowseTreeNode BrowseTreeNode { get; }
        public NodeId NodeId { get; }
        
        private ObservableCollectionExtended<BrowseItemViewModel> _children = new ObservableCollectionExtended<BrowseItemViewModel>();
        public ObservableCollectionExtended<BrowseItemViewModel> Children => _children;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public BrowseItemViewModel(Node<BrowseTreeNode, NodeId> node, BrowseItemViewModel parent, BrowseViewModel root)
        {
            BrowseTreeNode = node.Item;
            NodeId = node.Key;

            //Wrap loader for the nested view model inside a lazy so we can control when it is invoked
            var childrenLoader = new Lazy<IDisposable>(
                () => node.Children.Connect()
                    .Transform(e => new BrowseItemViewModel(e, this, root))
                    .Bind(_children)
                    .DisposeMany()
                    .Subscribe()
            );

            //return true when the children should be loaded 
            //(i.e. if current node is a root, otherwise when the parent expands)
            var shouldExpand = parent == null
                ? Observable.Return(true)
                : parent.WhenAnyValue(x => x.IsExpanded);

            //wire the observable
            var expander = shouldExpand
                    .Where(isExpanded => isExpanded)
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        //force lazy loading
                        var x = childrenLoader.Value;
                    });

            this.WhenAnyValue(x => x.IsSelected)
                .Where(x => x)
                .Subscribe(_ => root.SelectedItem = this);

            _cleanUp = Disposable.Create(() =>
            {
                expander.Dispose();
                if (childrenLoader.IsValueCreated)
                    childrenLoader.Value.Dispose();
            });
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
