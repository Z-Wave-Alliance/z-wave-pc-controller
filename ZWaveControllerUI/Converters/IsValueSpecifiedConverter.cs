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
    public class IsValueSpecifiedConverter : MarkupExtension, IMultiValueConverter
    {
        private static IsValueSpecifiedConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new IsValueSpecifiedConverter();
            return _converter;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is bool)
            {
                if (targetType == typeof(string))
                {
                    if ((bool)values[0])
                        return values[1].ToString();
                    else
                        return null;
                }
                else if (targetType == typeof(bool?))
                {
                    if ((bool)values[0])
                        return values[1];
                    else
                        return null;
                }
                else
                    return Binding.DoNothing;
            }
            else
                return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
