/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

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
                    return Binding.DoNothing;
            }
            else
                return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
