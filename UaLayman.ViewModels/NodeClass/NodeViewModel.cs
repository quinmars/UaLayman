using System;
using System.Collections.Generic;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public sealed class NodeViewModel : BaseNodeViewModel
    {
        public static BaseNodeViewModel Create(BrowseItemViewModel item, IChannelService channel)
        {
            if (item is null)
                return null;

            return Create(item.NodeId, item.BrowseTreeNode.ReferenceDescription, channel);
        }

        private static BaseNodeViewModel Create(NodeId id, ReferenceDescription rd, IChannelService channel)
        {
            switch (rd.NodeClass)
            {
                case NodeClass.Variable:
                    return new VariableNodeViewModel(id, rd, channel);
                case NodeClass.Unspecified:
                case NodeClass.Object:
                case NodeClass.Method:
                case NodeClass.ObjectType:
                case NodeClass.VariableType:
                case NodeClass.ReferenceType:
                case NodeClass.DataType:
                case NodeClass.View:
                default:
                    return new NodeViewModel(id, rd, channel);
            }
        }

        public NodeViewModel(NodeId id, ReferenceDescription rd, IChannelService channel) : base(id, rd, channel)
        {
            Update.Execute(BaseNodeViewModel.BaseAttributes).Subscribe();
        }

    }
}
