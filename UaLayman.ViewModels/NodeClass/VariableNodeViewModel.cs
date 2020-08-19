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
            AttributeIds.DataType,
            AttributeIds.Value,
            AttributeIds.AccessLevel,
            AttributeIds.UserAccessLevel,
            AttributeIds.MinimumSamplingInterval,
            AttributeIds.Historizing
        }).ToArray();

        private VariantViewModel _variantViewModel;
        public VariantViewModel VariantViewModel
        {
            get => _variantViewModel;
            private set => this.RaiseAndSetIfChanged(ref _variantViewModel, value);
        }
        
        private Variant _variant;
        public Variant Variant
        {
            get => _variant;
            private set => this.RaiseAndSetIfChanged(ref _variant, value);
        }

        private NodeId _dataType;
        public NodeId DataType
        {
            get => _dataType;
            private set => this.RaiseAndSetIfChanged(ref _dataType, value);
        }

        private string _dataTypeName;
        public string DataTypeName
        {
            get => _dataTypeName;
            private set => this.RaiseAndSetIfChanged(ref _dataTypeName, value);
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
                            Variant = r.Item2.Variant;
                            break;
                        case AttributeIds.DataType:
                            DataType = val as NodeId;
                            DataTypeName = NodeIds.GetName(DataType);
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
                .Subscribe(val => Variant = val.Variant);

            this.WhenAnyValue(x => x.Variant)
                .Subscribe(x =>
                {
                    if (VariantViewModel == null || x.Type != VariantViewModel.VariantType)
                    {
                        VariantViewModel = VariantViewModel.Create(x);
                    }
                    else
                    {
                        VariantViewModel.Variant = x;
                    }
                });
        }

        public override void Dispose()
        {
            _nodeValueSubscription?.Dispose();
            base.Dispose();
        }
    }
}
