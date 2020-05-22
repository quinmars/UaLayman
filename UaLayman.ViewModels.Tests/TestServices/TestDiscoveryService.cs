using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels.Tests.TestServices
{
    internal static class TestDiscoveryService
    {
        public static IDiscoveryService Silent
        {
            get
            {
                var service = Substitute.For<IDiscoveryService>();

                service.GetEndpoints(Arg.Any<string>()).Returns(Observable.Never<EndpointDescription[]>());

                return service;
            }
        }
    }
}
