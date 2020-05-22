using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace UaLayman
{
    public interface IDiscoveryService
    {
        IObservable<EndpointDescription[]> GetEndpoints(string url);
    }
}
