/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class ByteArrayToBoolConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is byte[])
            {
                byte[] tArr = (byte[])value;
                if (tArr != null && tArr.Length > 0)
                    if (tArr[0] == 0x00) return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true) return new byte[] { 0x01 };
            }
            return new byte[] { 0x00 };
        }

        #endregion
    }
}
