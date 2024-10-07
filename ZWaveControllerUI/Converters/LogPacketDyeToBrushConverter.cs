/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Utils.UI.Enums;

namespace ZWaveControllerUI.Converters
{
    public class LogPacketDyeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Dyes))
                return new SolidColorBrush(Colors.Black);

            var dye = (Dyes) value;
            switch (dye)
            {
                case Dyes.Black:
                    return new SolidColorBrush(Colors.Black);
                case Dyes.Blue:
                    return new SolidColorBrush(Colors.DodgerBlue);
                case Dyes.DarkOrange:
                    return new SolidColorBrush(Colors.DarkGoldenrod);
                case Dyes.Gray:
                    return new SolidColorBrush(Colors.Gray);
                case Dyes.Green:
                    return new SolidColorBrush(Colors.DarkGreen);
                case Dyes.Orange:
                    return new SolidColorBrush(Colors.Orange);
                case Dyes.Red:
                    return new SolidColorBrush(Colors.DarkRed);
                case Dyes.Yellow:
                    return new SolidColorBrush(Colors.Yellow);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
