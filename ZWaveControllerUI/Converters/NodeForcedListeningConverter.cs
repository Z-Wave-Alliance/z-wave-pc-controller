/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class NodeForcedListeningConverter : ZWaveDefinitionReference, IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;
            if (value is NodeTag)
            {
                var device = (NodeTag)value;
                if (device != null && Network != null)
                {
                    ret = Network.IsForcedListening(device);
                }
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemreference = parameter as ObjectReference;
            
            if (value is bool && itemreference != null && itemreference.Value is NodeTag && Network != null)
            {
                NodeTag device = (NodeTag)itemreference.Value;
                Network.SetForcedListening(device, (bool)value);
            }
            return Binding.DoNothing;
        }

        #endregion
    }
}
