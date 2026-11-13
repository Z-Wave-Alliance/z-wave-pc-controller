// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using Avalonia.Data;
using Avalonia.Data.Converters;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;
using ZWave.BasicApplication.Security;

namespace ZWaveControllerUI.Converters
{
    public class ParameterS2EnumDescriptionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The combo box selection box shows the already described item.
            if (!(value is ParameterS2Type))
                return value;
            var type = typeof(ParameterS2Type);
            var name = Enum.GetName(type, value);
            FieldInfo fi = type.GetField(name);
            var descriptionAttrib = (DescriptionAttribute)
                Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            return descriptionAttrib.Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
