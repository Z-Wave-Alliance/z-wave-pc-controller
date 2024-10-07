/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using ZWave.Devices;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class NodeLongRangeConverter : ZWaveDefinitionReference, IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? ret = null;
            if (value is NodeTag && Network != null)
            {
                var device = (NodeTag)value;
                if (Network.IsLongRange(device))
                {
                    ret = device == Network.NodeTag || Network.GetNodeInfo(device).LongRange > 0;
                }
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
