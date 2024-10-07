/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Utils;
using System.Reflection;

namespace ZWaveControllerUI.Converters
{
    public class UInt32ToColorPropertyInfoConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is byte[]))
                return Colors.White;
            else
            {
                
                byte[] argbArray = (byte[])value;
                PropertyInfo[] colors = typeof(Colors).GetProperties();
                PropertyInfo result = typeof(Colors).GetProperty("White"); //Default
                foreach (PropertyInfo pInfo in colors)
                {
                    Color c = (Color)pInfo.GetValue(null, null);
                    if (c.A == argbArray[0] && 
                        c.R == argbArray[1] && 
                        c.G == argbArray[2] && 
                        c.B == argbArray[3])
                    {
                        result = pInfo;
                        break;
                    }
                }
                return result;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is PropertyInfo))
                return 0;
            else
            {
                Color _value = (Color)((value as PropertyInfo).GetValue(null, null));
                byte[] argbArray = new byte[] { _value.A, _value.R, _value.G, _value.B };
                return argbArray;
            }

        }

        #endregion
    }
}
