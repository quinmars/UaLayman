﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Workstation.ServiceModel.Ua;

namespace UaLayman.Converters
{
    public class BeautifySecurityPolicyUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string v)
            {
                return Regex
                    .Replace(v, ".*\\#(.*$)", "$1")
                    .Replace("_", "-");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
