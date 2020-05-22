using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace UaLayman.Services
{
    public class ChannelService : ReactiveObject, IChannelService
    {
        private UaTcpSessionChannel _channel;
        public UaTcpSessionChannel Channel
        {
            get => _channel;
            set => this.RaiseAndSetIfChanged(ref _channel, value);
        }

        public IObservable<CommunicationState> State { get; }

        public ChannelService()
        {
            State = this
                .WhenAnyValue(x => x.Channel)
                .Select(x =>
                {
                    if (x is null)
                        return Observable.Return(CommunicationState.Faulted);
                    return Observable.Merge(
                            Observable.FromEventPattern(eh => x.Opening += eh, eh => x.Opening -= eh),
                            Observable.FromEventPattern(eh => x.Opened += eh, eh => x.Opened -= eh),
                            Observable.FromEventPattern(eh => x.Closing += eh, eh => x.Closing -= eh),
                            Observable.FromEventPattern(eh => x.Closed += eh, eh => x.Closed -= eh),
                            Observable.FromEventPattern(eh => x.Faulted += eh, eh => x.Faulted -= eh)
                        )
                        .Select(args => ((UaTcpSessionChannel)args.Sender).State);
                })
                .Switch()
                .Prepend(CommunicationState.Faulted)
                .Distinct()
                .Publish()
                .RefCount()
                        .Do(_ =>
                        {

                        });
        }

        public IObservable<Unit> Connect(EndpointDescription endpoint)
        {
            // Step 1 - Describe this app.
            var appDescription = new ApplicationDescription
            {
                ApplicationName = "UaLayman",
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:UaLayman",
                ApplicationType = ApplicationType.Client,
            };

            // Step 2 - Create a certificate store.
            var certificateStore = new DirectoryStore(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UaLayman", "pki"));

            // Step 4 - Create a session with the server.
            var channel = new UaTcpSessionChannel(
                appDescription,
                certificateStore,
                default(IUserIdentity),
                endpoint);

            Channel = channel;

            return Observable.FromAsync(async token =>
            {
                await channel.OpenAsync(token);
            });
        }

        public IObservable<Unit> Disconnect()
        {
            var c = Channel;
            if (c is null)
                return Observable.Return(Unit.Default);

            return Observable.FromAsync(async token =>
            {
                await c.CloseAsync(token);
            });
        }

        public IObservable<BrowseTreeNode[]> Browse()
        {
            if (_channel == null)
                return Observable.Throw<BrowseTreeNode[]>(new Exception("Not connected"));

            var channel = _channel;
            var root = new[] { NodeId.Parse(ObjectIds.RootFolder) };
            return Observable.Create<BrowseTreeNode[]>((obs, token) => 
                    BrowseAsync(channel, root, obs, token));
        }

        static async Task BrowseAsync(UaTcpSessionChannel channel, IEnumerable<NodeId> parentIds, IObserver<BrowseTreeNode[]> obs, CancellationToken token)
        {
            if (channel is null)
                throw new ArgumentNullException(nameof(channel));
            if (parentIds is null)
                throw new ArgumentNullException(nameof(parentIds));
            if (obs is null)
                throw new ArgumentNullException(nameof(obs));

            var nextNodeIds = new List<NodeId>();
            var parentIdOffset = 0;
            var hierachical = NodeId.Parse(ReferenceTypeIds.HierarchicalReferences);

            var request = new BrowseRequest
            {
                NodesToBrowse = parentIds.Select(id => new BrowseDescription
                {
                    NodeId = id,
                    ReferenceTypeId = NodeId.Parse(ReferenceTypeIds.HierarchicalReferences),
                    ResultMask = (uint)BrowseResultMask.TargetInfo,
                    NodeClassMask = (uint)NodeClass.Unspecified,
                    BrowseDirection = BrowseDirection.Forward,
                    IncludeSubtypes = true
                })
                .ToArray()
            };
            
            var response = await channel.BrowseAsync(request);
            Consume(response.Results);
            var continuations = response.Results
                .Select(r => r.ContinuationPoint)
                .Where(cp => cp != null)
                .ToArray();

            while (continuations.Length > 0)
            {
                var next = new BrowseNextRequest
                {
                    ContinuationPoints = continuations,
                    ReleaseContinuationPoints = false
                };

                var response2 = await channel.BrowseNextAsync(next);
                Consume(response2.Results);
                continuations = response2.Results
                    .Select(r => r.ContinuationPoint)
                    .Where(cp => cp != null)
                    .ToArray();
            }

            if (nextNodeIds.Any())
            {
                await BrowseAsync(channel, nextNodeIds, obs, token);
            }

            void Consume(BrowseResult[] results)
            {
                if (results is null)
                    throw new ArgumentNullException(nameof(results));

                var refs = from t in results.Zip(parentIds.Skip(parentIdOffset), (r, p) => (BrowseResult: r, ParentId: p))
                           from reference in t.BrowseResult.References ?? Enumerable.Empty<ReferenceDescription>()
                           select new BrowseTreeNode
                           {
                               NodeId = ExpandedNodeId.ToNodeId(reference.NodeId, channel.NamespaceUris),
                               ParentId = t.ParentId,
                               ReferenceDescription = reference
                           };

                var refsArray = refs.ToArray();

                if (refsArray.Length == 0)
                {
                    return;
                }

                parentIdOffset += response.Results.Length;
                nextNodeIds.AddRange(refsArray.Select(n => n.NodeId));

                obs.OnNext(refsArray);
            }
        }

        public IObservable<DataValue[]> ReadAttributes(NodeId nodeId, IEnumerable<uint> attributeIds)
        {
            if (_channel == null)
                return Observable.Throw<DataValue[]>(new Exception("Not connected"));

            var channel = _channel;
            return Observable.Create<DataValue[]>(async (obs, token) =>
            {
                var request = new ReadRequest
                {
                    NodesToRead = attributeIds
                        .Select(id => new ReadValueId
                        {
                            NodeId = nodeId,
                            AttributeId = id
                        })
                        .ToArray()
                };

                var response = await _channel.ReadAsync(request);

                obs.OnNext(response.Results);
            });
        }
    }
}
