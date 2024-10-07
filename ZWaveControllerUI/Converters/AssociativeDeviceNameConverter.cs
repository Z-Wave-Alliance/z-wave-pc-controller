/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ZWave.Devices;

namespace ZWaveControllerUI.Converters
{
    class AssociativeDeviceNameConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string ret = "unknown";
            var epid = (NodeTag)value;
            if (epid.EndPointId == 0x00)
            {
                ret = "Root Device (Endpoint 0)";
            }
            else
            {
                ret = "Endpoint " + epid.EndPointId.ToString();
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
