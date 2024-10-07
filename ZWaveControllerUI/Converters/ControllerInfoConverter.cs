/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using ZWaveController.Converters;

namespace ZWaveControllerUI.Converters
{
    public class ControllerInfoConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IViewModelConverter controllerInfoVMConverter = new ControllerInfoVMConverter();
            return controllerInfoVMConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IViewModelConverter controllerInfoVMConverter = new ControllerInfoVMConverter();
            return controllerInfoVMConverter.ConvertBack(value, targetType, parameter, culture);
        }

        #endregion
    }
}
