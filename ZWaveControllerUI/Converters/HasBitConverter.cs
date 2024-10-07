/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using ZWaveController.Converters;

namespace ZWaveControllerUI.Converters
{
    public class HasBitConverter : IValueConverter
    {
        HasBitVMConverter _hasBitVMConverter = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_hasBitVMConverter == null)
            {
                lock (this)
                {
                    if (_hasBitVMConverter == null)
                    {
                        _hasBitVMConverter = new HasBitVMConverter();
                    }
                }
            }
            return _hasBitVMConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
