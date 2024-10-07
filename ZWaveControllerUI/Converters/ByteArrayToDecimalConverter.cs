/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Utils;

namespace ZWaveControllerUI.Converters
{
    public class ByteArrayToDecimalConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];

            if (bytes == null)
                return 0;

            return Tools.GetInt32(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var len = parameter is string && new[] { "1", "2", "3", "4" }.Contains((string)parameter) ? int.Parse((string)parameter) : 4;
            if (!(value is int))
            {
                return new byte[len];
            }
            else
            {
                var val = Tools.GetBytes((int)value).Skip(4 - len).Take(len).ToArray();
                return val;
            }
        }

        #endregion
    }
}