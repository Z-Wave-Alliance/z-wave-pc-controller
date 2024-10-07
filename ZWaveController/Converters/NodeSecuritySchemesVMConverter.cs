/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using ZWave.Devices;
using ZWave.Enums;

namespace ZWaveController.Converters
{
    public class NodeSecuritySchemesVMConverter : IViewModelConverter
    {
        private NetworkViewPoint _networkViewPoint;

        public NodeSecuritySchemesVMConverter(NetworkViewPoint networkViewPoint)
        {
            _networkViewPoint = networkViewPoint;
        }

        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            var strParameter = param as string;
            if (value is NodeTag && _networkViewPoint != null && strParameter != null)
            {
                var scheme = (SecuritySchemes)Enum.Parse(typeof(SecuritySchemes), strParameter, true);
                var device = (NodeTag)value;
                bool result = false;
                if (device.EndPointId == 0)
                {
                    result = _networkViewPoint.HasSecurityScheme(device, scheme);
                }
                return result;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
