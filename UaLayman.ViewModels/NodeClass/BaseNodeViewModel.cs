using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class BaseNodeViewModel : ReactiveObject
    {
        public static uint[] BaseAttributes { get; } = new[]
        {
            AttributeIds.Description,
            AttributeIds.WriteMask,
            AttributeIds.UserWriteMask
        };

        // Attributes
        public NodeId NodeId { get; }
        public NodeClass NodeClass { get; }
        public QualifiedName BrowseName { get; }
        public LocalizedText DisplayName { get; }

        private LocalizedText _description;
        public LocalizedText Description
        {
            get => _description;
            private set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private UInt32? _writeMask;
        public UInt32? WriteMask
        {
            get => _writeMask;
            private set => this.RaiseAndSetIfChanged(ref _writeMask, value);
        }

        private UInt32? _userWriteMask;
        public UInt32? UserWriteMask
        {
            get => _userWriteMask;
            private set => this.RaiseAndSetIfChanged(ref _userWriteMask, value);
        }

        protected ReactiveCommand<IEnumerable<uint>,IEnumerable<(uint, DataValue)>> Update { get; }

        protected BaseNodeViewModel(NodeId id, ReferenceDescription rd, IChannelService channel)
        {
            NodeId = id;
            NodeClass = rd.NodeClass;
            BrowseName = rd.BrowseName;
            DisplayName = rd.DisplayName;

            Update = ReactiveCommand.CreateFromObservable((IEnumerable<uint> ids) =>
            {
                return channel.ReadAttributes(NodeId, ids).Select(dvs => ids.Zip(dvs, (i, d) => (i, d)));
            });

            Update.Subscribe(result =>
            {
                foreach (var r in result)
                {
                    var att = r.Item1;
                    var val = r.Item2.Value;

                    switch (att)
                    {
                        case AttributeIds.Description:
                            var s = val as LocalizedText;
                            Description = string.IsNullOrWhiteSpace(s?.Text) ? null : s;
                            break;
                        case AttributeIds.WriteMask:
                            WriteMask = val as uint?;
                            break;
                        case AttributeIds.UserWriteMask:
                            WriteMask = val as uint?;
                            break;
                    }
                }
            });
        }
    }
}
