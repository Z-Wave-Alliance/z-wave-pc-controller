// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ZWaveControllerUI.Converters
{
    /// <summary>
    /// True for a non zero number; replaces WPF DataTriggers on Value="0".
    /// </summary>
    public class IsNonZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            try
            {
                return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
