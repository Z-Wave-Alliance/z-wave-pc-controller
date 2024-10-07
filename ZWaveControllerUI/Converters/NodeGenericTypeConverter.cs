/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Utils.UI.Wrappers;
using ZWave.BasicApplication.Devices;
using ZWave.Xml.Application;
using ZWaveControllerUI.Bind;
using ZWave.Devices;

namespace ZWaveControllerUI.Converters
{
    public class NodeGenericTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SelectableItem<DeviceInfo>) && !(value is DeviceInfo))
                return "N/A";

            if (!(parameter is ZWaveDefinitionReference))
                return "N/A";

            SelectableItem<DeviceInfo> selectableItem = value as SelectableItem<DeviceInfo>;
            DeviceInfo deviceInfo =  (value as DeviceInfo) ?? selectableItem.Item;
            ZWaveDefinition zWaveDefinition = ((ZWaveDefinitionReference)parameter).ZWaveDefinition;

            if (zWaveDefinition == null)
                return "N/A";

            GenericDevice genericDevice = zWaveDefinition.GenericDevices.FirstOrDefault(p => p.KeyId == deviceInfo.Generic);
            if (genericDevice == null)
                return "N/A";

            return genericDevice.Text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
