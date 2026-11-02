// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ZWaveControllerUI.Converters
{
    public class AreEqualConvertor : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(System.Collections.Generic.IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Count == 2)
            {
                return values[0].Equals(values[1]);
            }
            else if (values != null && values.Count == 3)
            {
                byte value = (byte)values[2];
                return values[0].Equals(values[1]) && value == 0;
            }
            
            else
            {
                return BindingOperations.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
