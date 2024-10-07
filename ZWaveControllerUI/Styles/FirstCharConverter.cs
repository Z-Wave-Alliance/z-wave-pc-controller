/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Data;

namespace ZWaveControllerUI.Styles
{
    public class FirstCharConverter : IValueConverter
    {
        private static bool hasSymbolFont = false;
        private static bool isScanned = false;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!isScanned)
            {
                isScanned = true;
                InstalledFontCollection vv = new InstalledFontCollection();
                foreach (var item in vv.Families)
                {
                    if (item.Name == "Segoe UI Symbol")
                    {
                        hasSymbolFont = true;
                        break;
                    }
                }
            }

            if (hasSymbolFont && value as string != null && ((string)value).Length > 0 && ((string)value)[0] != ' ')
            {
                return ((string)value).Substring(0, 1);
            }
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
