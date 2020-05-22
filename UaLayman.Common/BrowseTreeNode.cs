using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace UaLayman
{
    public class BrowseTreeNode
    {
        public NodeId NodeId { get; set; }
        public NodeId ParentId { get; set; }
        public ReferenceDescription ReferenceDescription  { get; set; }
    }
}
