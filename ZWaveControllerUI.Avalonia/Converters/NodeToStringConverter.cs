// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ZWave.Devices;

namespace ZWaveControllerUI.Converters
{
    public class NodeToStringConverter : IValueConverter
    {
        //NodeTag
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = string.Empty;
            if (value != null)
            {
                NodeTag node = (NodeTag)value;
                // Spin boxes bind an int (IntValue); WPF converted the text implicitly.
                var numericType = targetType == null ? null : Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (numericType == typeof(int))
                    return (int)node.Id;
                ret = node.Id.ToString();
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort tmp = 0;
            if (value is int || value is decimal)
            {
                tmp = System.Convert.ToUInt16(value);
            }
            else if (value is string)
            {
                ushort.TryParse((string)value, out tmp);
            }
            return new NodeTag(tmp);
        }
    }
}
