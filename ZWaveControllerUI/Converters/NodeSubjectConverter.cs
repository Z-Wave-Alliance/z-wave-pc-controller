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
    public class NodeSubjectConverter : ZWaveDefinitionReference, IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = false;
            if (value is NodeTag && Network != null)
            {
                var device = (NodeTag)value;
                ret = device == Network.NodeTag;
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
