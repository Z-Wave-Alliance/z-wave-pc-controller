/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class SizeToMaxDecimalConverter : IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int bits = (int)value;
                int maxValue = 0;
                switch (bits / 8)
                {
                    case 0:
                        maxValue = (int)Math.Pow(2, bits) - 1;
                        break;
                    case 1:
                        maxValue = byte.MaxValue;
                        break;
                    case 2:
                        maxValue = ushort.MaxValue;
                        break;
                    case 3:
                        maxValue = ushort.MaxValue * byte.MaxValue;
                        break;
                    default:
                        maxValue = int.MaxValue;
                        break;
                }
                return maxValue;
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
