using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class WatchlistItemViewModel : ReactiveObject, IDisposable
    {
        private IDisposable _nodeValueSubscription;

        public NodeId NodeId { get; }
        public string BrowsePath { get; }

        private string _value;
        public string Value
        {
            get => _value;
            private set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        private string _setValue;
        public string SetValue
        {
            get => _setValue;
            set => this.RaiseAndSetIfChanged(ref _setValue, value);
        }

        public WatchlistItemViewModel(NodeId nodeId, string browsePath, IChannelService channel)
        {
            NodeId = nodeId;
            BrowsePath = browsePath;

            _nodeValueSubscription = channel.NodeValue(NodeId)
                .Subscribe(val => Value = val.Variant.ToDisplayString());
        }

        public void Dispose()
        {
            _nodeValueSubscription?.Dispose();
        }
    }
}
