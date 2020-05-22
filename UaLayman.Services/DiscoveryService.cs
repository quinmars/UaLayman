using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;

namespace UaLayman.Services
{
    public class DiscoveryService : IDiscoveryService
    {
        public IObservable<EndpointDescription[]> GetEndpoints(string url)
        {
            var request = new GetEndpointsRequest
            {
                EndpointUrl = url,
            };

            return Observable.FromAsync(async () =>
            {
                var respones = await UaTcpDiscoveryService.GetEndpointsAsync(request);
                return respones
                    .Endpoints;
            });
        }
    }
}
