using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class VariantViewModel : ReactiveObject
    {
        private Variant _variant;
        public Variant Variant
        {
            get => _variant;
            set => this.RaiseAndSetIfChanged(ref _variant, value);
        }

        public VariantType VariantType => _variant.Type;
        public int Rank
        {
            get
            {
                if (Variant.ArrayDimensions is null)
                {
                    return 0;
                }
                return Variant.ArrayDimensions.Length;
            }
        }

        public static VariantViewModel Create(Variant variant)
        {
            if (variant.ArrayDimensions != null && variant.ArrayDimensions.Length != 0)
                return new StringVariantViewModel<object>(variant, _ => "Array");

            switch (variant.Type)
            {
                case VariantType.Null:
                    return new StringVariantViewModel<object>(variant, _ => "Null");
                case VariantType.Boolean:
                    return new StringVariantViewModel<bool>(variant, b => b ? "True" : "False");
                case VariantType.SByte:
                case VariantType.Byte:
                case VariantType.Int16:
                case VariantType.UInt16:
                case VariantType.Int32:
                case VariantType.UInt32:
                case VariantType.Int64:
                case VariantType.UInt64:
                    return new StringVariantViewModel<object>(variant, v => v.ToString());
                case VariantType.Float:
                    return new StringVariantViewModel<float>(variant, v => v.ToString("G5"));
                case VariantType.Double:
                    return new StringVariantViewModel<double>(variant, v => v.ToString("G5"));
                case VariantType.String:
                    return new StringVariantViewModel<string>(variant, v => $"'{v}'", isDisplayString: false);
                case VariantType.DateTime:
                    return new StringVariantViewModel<DateTime>(variant, v => $"{v:yyyy-dd-MM}\n{v:HH:mm:ss}");
                case VariantType.Guid:
                    return new StringVariantViewModel<Guid>(variant, v => v.ToString(), isDisplayString: false, isMonospaced: true);
                case VariantType.ByteString:
                    return new StringVariantViewModel<byte[]>(variant, ByteStringToString, isDisplayString: false, isMonospaced: true);
                case VariantType.XmlElement:
                case VariantType.NodeId:
                case VariantType.ExpandedNodeId:
                case VariantType.StatusCode:
                    return new StringVariantViewModel<StatusCode>(variant, StatusCodeToString, isDisplayString: false);
                case VariantType.QualifiedName:
                case VariantType.LocalizedText:
                case VariantType.ExtensionObject:
                case VariantType.DataValue:
                case VariantType.Variant:
                case VariantType.DiagnosticInfo:
                default:
                    return new StringVariantViewModel<object>(variant, _ => "Unkwon");
            }
        }

        private static string StatusCodeToString(StatusCode code)
        {
            return $"{StatusCodes.GetDefaultMessage(code)} ({code})";
        }

        private static string ByteStringToString(byte[] v)
        {
            return $"[{v.Length:000}] {string.Join(" ", v.Select(b => b.ToString("X2")))}";
        }
    }

    public class VariantViewModel<T> : VariantViewModel
    {
        private T _value;
        public T Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
    }
}
