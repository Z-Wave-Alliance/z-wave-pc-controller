/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class NodeEndDeviceApiConverter : ZWaveDefinitionReference, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;
            if (value is NodeTag)
            {
                var device = (NodeTag)value;
                NetworkViewPoint network = Network;
                if (device != null && network != null)
                {
                    ret = network.IsEndDeviceApi(device);
                }
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
