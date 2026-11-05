// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ZWaveControllerUI.Converters
{
    public class AndConverter : IMultiValueConverter
    {
        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0)
                return false;

            foreach (var value in values)
            {
                if (!(value is bool))
                    return false;
                
                if (!(bool) value)
                    return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
