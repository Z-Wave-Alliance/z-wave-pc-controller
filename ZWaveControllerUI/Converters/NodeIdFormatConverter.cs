/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class NodeIdFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is byte) && !(value is ushort))
                return (value != null ? value.ToString() : string.Empty);

            if (value is ushort)
            {
                var val = (ushort)(value);
                byte nodeId = (byte)val;
                byte endPointId = (byte)(val >> 8);
                string ret = nodeId.ToString("");
                if (endPointId != 0)
                {
                    ret = ret + "." + endPointId.ToString();
                }
                return ret;
            }
                
            byte id = (byte) value;
            return id.ToString("");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string)
            {
                byte res = 0;
                byte.TryParse(value as string, out res);
                return res;
            }
            else
                return Binding.DoNothing;
        }
    }
}