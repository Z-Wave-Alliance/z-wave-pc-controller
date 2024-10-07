/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace ZWaveController.Converters
{
    public class HasBitVMConverter : IViewModelConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            if (value is byte && param is int)
            {
                var bit = (byte)param;
                return (Tools.GetMaskFromBits(1, bit) & (byte)value) > 0;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
