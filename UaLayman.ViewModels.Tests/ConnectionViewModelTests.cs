using FluentAssertions;
using NSubstitute;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using UaLayman.ViewModels.Tests.TestServices;
using Workstation.ServiceModel.Ua;
using Xunit;

namespace UaLayman.ViewModels.Tests
{
    public class ConnectionViewModelTests : TestBase
    {
        private readonly EndpointDescription[] _testEndpoints1 = new []
        {
            new EndpointDescription
            {
                SecurityPolicyUri = "None",
                SecurityMode = MessageSecurityMode.None
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "RSA",
                SecurityMode = MessageSecurityMode.Sign
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "RSA",
                SecurityMode = MessageSecurityMode.SignAndEncrypt
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "SHA",
                SecurityMode = MessageSecurityMode.Sign
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "SHA",
                SecurityMode = MessageSecurityMode.SignAndEncrypt
            },
        };
        
        private readonly EndpointDescription[] _testEndpoints2 = new []
        {
            new EndpointDescription
            {
                SecurityPolicyUri = "None2",
                SecurityMode = MessageSecurityMode.None
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "RSA2",
                SecurityMode = MessageSecurityMode.Sign
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "RSA2",
                SecurityMode = MessageSecurityMode.SignAndEncrypt
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "SHA2",
                SecurityMode = MessageSecurityMode.Sign
            },
            new EndpointDescription
            {
                SecurityPolicyUri = "SHA2",
                SecurityMode = MessageSecurityMode.SignAndEncrypt
            },
        };

        [Fact]
        public void Create()
        {
            var vm = new ConnectionViewModel(null, TestChannelService.Silent, TestDiscoveryService.Silent);

            vm.AvailableEndpoints
                .Should().BeNullOrEmpty();

            vm.ConnectionString
                .Should().BeNull();

            vm.SelectedEndpoint
                .Should().BeNull();

            vm.IsConnectingOrDisconnecting
                .Should().BeFalse();

            vm.IsSearchingForSecurityPolicies
                .Should().BeFalse();

            vm.ExecutionError
                .Should().BeNull();
        }
        
        [Fact]
        public void SetConnectionString_Fast()
        {
            var discovery = Substitute.For<IDiscoveryService>();
            discovery.GetEndpoints("url").Returns(Observable.Return(_testEndpoints1));

            var vm = new ConnectionViewModel(null, TestChannelService.Silent, discovery);

            vm.ConnectionString = "url";
            vm.StartDiscover();

            vm.AvailableEndpoints
                .Should().BeEquivalentTo(_testEndpoints1);
        }

        [Fact]
        public void SetConnectionString_IsSearching()
        {
            var policies = new Subject<EndpointDescription[]>();

            var discovery = Substitute.For<IDiscoveryService>();
            discovery.GetEndpoints("url").Returns(policies);

            var vm = new ConnectionViewModel(null, TestChannelService.Silent, discovery);

            vm.ConnectionString = "url";
            vm.StartDiscover();

            vm.IsSearchingForSecurityPolicies
                .Should().BeTrue();

            policies.OnNext(_testEndpoints1);
            policies.OnCompleted();
            
            vm.IsSearchingForSecurityPolicies
                .Should().BeFalse();
            vm.AvailableEndpoints
                .Should().BeEquivalentTo(_testEndpoints1);
        }
        
        [Fact]
        public void SetConnectionString_OneFalling()
        {
            var discovery = Substitute.For<IDiscoveryService>();
            discovery.GetEndpoints("url1").Returns(Observable.Return(_testEndpoints1));
            discovery.GetEndpoints("badurl").Returns(Observable.Throw<EndpointDescription[]>(new ServiceResultException(StatusCodes.BadArgumentsMissing)));
            discovery.GetEndpoints("url2").Returns(Observable.Return(_testEndpoints2));

            var vm = new ConnectionViewModel(null, TestChannelService.Silent, discovery);

            vm.ConnectionString = "url1";
            vm.StartDiscover();

            vm.AvailableEndpoints
                .Should().BeEquivalentTo(_testEndpoints1);

            vm.ConnectionString = "badurl";
            vm.StartDiscover();

            vm.AvailableEndpoints
                .Should().BeNullOrEmpty();
            vm.ExecutionError
                .Should().NotBeNullOrEmpty();

            vm.ConnectionString = "url2";
            vm.StartDiscover();

            vm.AvailableEndpoints
                .Should().BeEquivalentTo(_testEndpoints2);
            vm.ExecutionError
                .Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void SetEndpoint()
        {
            var discovery = Substitute.For<IDiscoveryService>();
            discovery.GetEndpoints("url").Returns(Observable.Return(_testEndpoints1));

            var vm = new ConnectionViewModel(null, TestChannelService.Silent, discovery);

            vm.ConnectionString = "url";
            vm.StartDiscover();

            vm.SelectedSecurityPolicy = _testEndpoints1[3].SecurityPolicyUri;
            vm.SelectedSecurityMode = _testEndpoints1[3].SecurityMode.ToString();

            vm.SelectedEndpoint
                .Should().Be(_testEndpoints1[3]);
        }
    }
}
