/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
/// SPDX-FileCopyrightText: Z-Wave Alliance https://z-wavealliance.org
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ZWave.Xml.Application;

namespace ZWaveControllerUI.Converters
{
    public class SupportedCommandClassesConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] as ICollection<CommandClass> != null)
            {
                ICollection<CommandClass> cmdClasses = values[0] as ICollection<CommandClass>;
                List<CommandClass> ret = null;
                if (values[1] as byte[] != null)
                {
                    byte[] filter = values[1] as byte[];
                    ret = new List<CommandClass>(cmdClasses.Where(cc => filter.Contains(cc.KeyId)).ToList());
                }
                else
                {
                    ret = new List<CommandClass>(cmdClasses);
                }
                return ret?.OrderBy(cc => cc.Text).ThenBy(cc => cc.Version);
            }
            else
                return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
