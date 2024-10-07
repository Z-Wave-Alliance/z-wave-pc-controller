/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Utils;

namespace ZWaveControllerUI.Converters
{
    public class ByteToHexStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte val = 0x00;
            if (value == null)
                return string.Empty;

            if (value is byte)
                val = (byte)value;

            return Tools.GetHex(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str))
                return 0;

            byte ret = 0;
            byte.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out ret);
            return ret;
        }

        #endregion
    }
}
