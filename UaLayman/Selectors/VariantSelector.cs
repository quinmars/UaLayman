using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UaLayman.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UaLayman
{
    public class VariantSelector : DataTemplateSelector
    {
        public DataTemplate ShortStringTemplate { get; set; }
        public DataTemplate LongStringTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch (item)
            {
                case StringVariantViewModel svm:
                    if (svm.IsDisplayString)
                        return ShortStringTemplate;
                    else
                        return LongStringTemplate;
            }

            return null;
        }
    }
}
