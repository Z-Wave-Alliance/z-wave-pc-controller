/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using Utils;
using System.Collections.Generic;

namespace ZWaveControllerUI.Converters
{
    public class ByteArrayToStringOfDecimalsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];

            if (bytes == null)
                return string.Empty;

            return Tools.GetNodeIds(bytes, " ") + " ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (string.IsNullOrEmpty(strValue))
                return new byte[0];

            return Tools.ToNodeIds(strValue, new[] { ',', ' ', ';' });
        }

        #endregion
    }
}