using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public sealed class VariableNodeViewModel : BaseNodeViewModel
    {
        private IDisposable _nodeValueSubscription;

        public static uint[] VariableAttributes { get; } = BaseAttributes.Concat(new[]
        {
            AttributeIds.Value,
            AttributeIds.DataType,
            AttributeIds.AccessLevel,
            AttributeIds.UserAccessLevel,
            AttributeIds.MinimumSamplingInterval,
            AttributeIds.Historizing
        }).ToArray();

        private string _value;
        public string Value
        {
            get => _value;
            private set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        private NodeId _dataType;
        public NodeId DataType
        {
            get => _dataType;
            private set => this.RaiseAndSetIfChanged(ref _dataType, value);
        }

        private byte? _accessLevel;
        public byte? AccessLevel
        {
            get => _accessLevel;
            private set => this.RaiseAndSetIfChanged(ref _accessLevel, value);
        }

        private byte? _userAccessLevel;
        public byte? UserAccessLevel
        {
            get => _userAccessLevel;
            private set => this.RaiseAndSetIfChanged(ref _userAccessLevel, value);
        }

        private double? _minimumSanmplingInterval;
        public double? MinimumSamplingInterval
        {
            get => _minimumSanmplingInterval;
            private set => this.RaiseAndSetIfChanged(ref _minimumSanmplingInterval, value);
        }

        private bool? _historizing;
        public bool? Historizing
        {
            get => _historizing;
            private set => this.RaiseAndSetIfChanged(ref _historizing, value);
        }

        public VariableNodeViewModel(NodeId nodeId, ReferenceDescription rd, IChannelService channel) : base(nodeId, rd, channel)
        {
            Update.Subscribe(result =>
            {
                foreach (var r in result)
                {
                    var att = r.Item1;
                    var val = r.Item2.Value;

                    switch (att)
                    {
                        case AttributeIds.Value:
                            Value = r.Item2.Variant.ToDisplayString();
                            break;
                        case AttributeIds.DataType:
                            DataType = val as NodeId;
                            break;
                        case AttributeIds.AccessLevel:
                            AccessLevel = val as byte?;
                            break;
                        case AttributeIds.UserAccessLevel:
                            UserAccessLevel = val as byte?;
                            break;
                        case AttributeIds.MinimumSamplingInterval:
                            MinimumSamplingInterval = val as byte?;
                            break;
                        case AttributeIds.Historizing:
                            Historizing = val as bool?;
                            break;
                    }
                }
            });

            Update.Execute(VariableAttributes).Subscribe();

            _nodeValueSubscription = channel.NodeValue(NodeId)
                .Subscribe(val => Value = val.Variant.ToDisplayString());
        }

        public override void Dispose()
        {
            _nodeValueSubscription?.Dispose();
            base.Dispose();
        }
    }
}
