using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Workstation.ServiceModel.Ua;

namespace UaLayman.Converters
{
    public class StateToBrushConverter : IValueConverter
    {
        public Brush DefaultBrush { get; set; }
        public Brush ConnectionBrush { get; set; }
        public Brush FaultBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CommunicationState s)
            {
                switch (s)
                {
                    case CommunicationState.Opened:
                        return ConnectionBrush;
                    case CommunicationState.Faulted:
                        return FaultBrush;
                }
            }

            return DefaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
