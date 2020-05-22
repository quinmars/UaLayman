using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Workstation.ServiceModel.Ua;

namespace UaLayman.Converters
{
    public class SecurityModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is MessageSecurityMode v)
            {
                switch (v)
                {
                    case MessageSecurityMode.Invalid:
                        return "Invalid";
                    case MessageSecurityMode.None:
                        return "None";
                    case MessageSecurityMode.Sign:
                        return "Sign";
                    case MessageSecurityMode.SignAndEncrypt:
                        return "Sign and encrypt";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
