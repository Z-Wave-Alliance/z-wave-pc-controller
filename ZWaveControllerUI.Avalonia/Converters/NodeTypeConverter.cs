// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ZWaveController.Converters;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class NodeTypeConverter : ZWaveDefinitionReference, IValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nodeTypeVMConverter = new NodeTypeVMConverter(ZWaveDefinition, Network);
            return nodeTypeVMConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}