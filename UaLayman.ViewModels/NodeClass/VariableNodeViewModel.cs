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
                            Value = ConvertToString(r.Item2.Variant);
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
        }

        private static string ConvertToString(Variant variant)
        {
            if (variant.ArrayDimensions != null && variant.ArrayDimensions.Length != 0)
                return "Array";

            switch (variant.Type)
            {
                case VariantType.Null:
                    return "Null";
                case VariantType.Boolean:
                    {
                        var v = (bool)variant;
                        return v ? "True" : "False";
                    }
                case VariantType.SByte:
                    {
                        var v = (sbyte)variant;
                        return v.ToString();
                    }
                case VariantType.Byte:
                    {
                        var v = (byte)variant;
                        return v.ToString();
                    }
                case VariantType.Int16:
                    {
                        var v = (short)variant;
                        return v.ToString();
                    }
                case VariantType.UInt16:
                    {
                        var v = (ushort)variant;
                        return v.ToString();
                    }
                case VariantType.Int32:
                    {
                        var v = (int)variant;
                        return v.ToString();
                    }
                case VariantType.UInt32:
                    {
                        var v = (uint)variant;
                        return v.ToString();
                    }
                case VariantType.Int64:
                    {
                        var v = (long)variant;
                        return v.ToString();
                    }
                case VariantType.UInt64:
                    {
                        var v = (ulong)variant;
                        return v.ToString();
                    }
                case VariantType.Float:
                    {
                        var v = (float)variant;
                        return v.ToString("G5");
                    }
                case VariantType.Double:
                    {
                        var v = (double)variant;
                        return v.ToString("G5");
                    }
                case VariantType.String:
                    return (string)variant.Value;
                case VariantType.DateTime:
                case VariantType.Guid:
                case VariantType.ByteString:
                case VariantType.XmlElement:
                case VariantType.NodeId:
                case VariantType.ExpandedNodeId:
                case VariantType.StatusCode:
                case VariantType.QualifiedName:
                case VariantType.LocalizedText:
                case VariantType.ExtensionObject:
                case VariantType.DataValue:
                case VariantType.Variant:
                case VariantType.DiagnosticInfo:
                default:
                    return "Unkown";
            }
        }
    }
}
