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
    public class NodeViewSelector : DataTemplateSelector
    {
        public DataTemplate BaseTemplate { get; set; }
        public DataTemplate VariableTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is VariableNodeViewModel)
                return VariableTemplate;
            else if (item is BaseNodeViewModel)
                return BaseTemplate;

            return null;
        }
    }
}
