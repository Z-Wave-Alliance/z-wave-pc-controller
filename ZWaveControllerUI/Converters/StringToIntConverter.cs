/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ret = null;
            if (value != null)
                ret = value.ToString();
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ret = 0;
            if (value == null)
                ret = 0;

            if (value is string)
            {
                int tmp = 0;
                if (int.TryParse((string)value, out tmp))
                    ret = tmp;
            }

            return ret;
        }
    }
}
