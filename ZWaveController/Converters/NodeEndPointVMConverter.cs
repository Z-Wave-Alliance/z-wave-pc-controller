/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using ZWave.Devices;

namespace ZWaveController.Converters
{
    public class NodeEndPointVMConverter : IViewModelConverter
    {
        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            bool ret = false;
            if (value is NodeTag)
            {
                var device = (NodeTag)value;
                ret = device.EndPointId > 0;
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
