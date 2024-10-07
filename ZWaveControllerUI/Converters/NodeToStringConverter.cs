/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
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
                ret = node.Id.ToString();
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ushort tmp = 0;
            if (value is int)
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
