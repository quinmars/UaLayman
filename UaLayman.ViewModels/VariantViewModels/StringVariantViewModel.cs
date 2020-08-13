using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Workstation.ServiceModel.Ua;

namespace UaLayman.ViewModels
{
    public class StringVariantViewModel : VariantViewModel<string>
    {
        public bool IsDisplayString { get; }

        public StringVariantViewModel(bool isDisplayString)
        {
            IsDisplayString = isDisplayString;
        }
    }

    public class StringVariantViewModel<T> : StringVariantViewModel
    {
        public StringVariantViewModel(Variant variant, Func<T, string> selector, bool isDisplayString = true)
            : base(isDisplayString)
        {
            Variant = variant;

            this.WhenAnyValue(x => (T)x.Variant.Value)
                .Select(selector)
                .Subscribe(x => Value = x);
        }
    }
}
