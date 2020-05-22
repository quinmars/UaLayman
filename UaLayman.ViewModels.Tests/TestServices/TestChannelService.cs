using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels.Tests.TestServices
{
    internal static class TestChannelService
    {
        public static IChannelService Silent
        {
            get
            {
                var service = Substitute.For<IChannelService>();

                service.Browse().Returns(Observable.Never<BrowseTreeNode[]>());
                service.Connect(Arg.Any<EndpointDescription>()).Returns(Observable.Never<Unit>());
                service.Disconnect().Returns(Observable.Never<Unit>());
                service.State.Returns(Observable.Never<CommunicationState>());

                return service;
            }
        }
    }
}
