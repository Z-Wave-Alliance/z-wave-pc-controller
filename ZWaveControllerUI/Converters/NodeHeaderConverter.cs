/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System;
using System.Globalization;
using System.Windows.Data;

namespace ZWaveControllerUI.Converters
{
    public class NodeHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string caption = string.Empty;
            string content = string.Empty;

            if (value != null)
                caption = value.ToString();
            if (parameter != null)
                content = parameter.ToString();

            return string.Format("{0}: {1}", caption, content);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}