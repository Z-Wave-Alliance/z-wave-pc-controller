/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class TreeViewItemIconByDepthConverter : IValueConverter
    {
        public static readonly string[] ImagesPath =
        {
            "Properties.png",
            "Assembly.png",
            "Class_Icon.png",
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                return string.Empty;

            var depth = (int) value;
            if (depth >= ImagesPath.Length)
                depth = ImagesPath.Length - 1;

            if (depth < 0)
                depth = 0;

            return string.Format(@"..\Images\{0}", ImagesPath[depth]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}