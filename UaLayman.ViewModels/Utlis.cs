using System;
using System.Collections.Generic;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public static class Utlis
    {
        public static string ToDisplayString(this Variant variant)
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
