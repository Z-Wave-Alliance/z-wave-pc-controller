/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows.Data;
using ZWave.Layers;
using Utils;

namespace ZWaveControllerUI.Converters
{
    public class DataSourceToSourceTypeConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is SocketDataSource)
            {
                var socketDataSource = (SocketDataSource)value;
                return EnumDescriptionTypeConverter.GetEnumDescription(socketDataSource.Type);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
