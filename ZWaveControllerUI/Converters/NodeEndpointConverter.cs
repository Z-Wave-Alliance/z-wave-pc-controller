/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using ZWaveController.Converters;

namespace ZWaveControllerUI.Converters
{
    public class NodeEndPointConverter : IValueConverter
    {
        private NodeEndPointVMConverter _nodeEndPointVMConverter = new NodeEndPointVMConverter();

        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _nodeEndPointVMConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
