using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace UaLayman
{
    public interface IChannelService
    {
        IObservable<CommunicationState> State { get; }

        IObservable<Unit> Connect(EndpointDescription endpoint);
        IObservable<Unit> Disconnect();

        IObservable<BrowseTreeNode[]> Browse();
        IObservable<DataValue[]> ReadAttributes(NodeId nodeId, IEnumerable<uint> attributeIds);
        IObservable<DataValue> NodeValue(NodeId nodeId);
    }
}
