// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Markup.Xaml;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// TO-DO code review
    /// </summary>
    public class IsValueInRangeConverter : MarkupExtension, IValueConverter
    {
        private static IsValueInRangeConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new IsValueInRangeConverter();
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (targetType == typeof(string))
                {
                    var str = value.ToString();
                    if (str != "0")
                    {
                        return str;
                    }
                    else
                        return null;
                }
                else
                    return BindingOperations.DoNothing;
            }
            else
                return BindingOperations.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
