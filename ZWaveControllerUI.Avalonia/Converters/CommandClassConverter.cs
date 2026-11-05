// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Linq;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class CommandClassConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string ret = string.Empty;
            var definitionReference = parameter as ZWaveDefinitionReference;
            if (definitionReference != null && value != null && value is byte)
            {
                byte val = (byte)value;
                var cc = definitionReference.ZWaveDefinition.CommandClasses.FirstOrDefault(x => x.KeyId == val);
                if (cc != null)
                {
                    ret = cc.Name;
                }
            }
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
