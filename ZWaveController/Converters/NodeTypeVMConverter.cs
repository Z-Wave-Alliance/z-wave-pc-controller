/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Globalization;
using System.Linq;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;

namespace ZWaveController.Converters
{
    public class NodeTypeVMConverter : IViewModelConverter
    {
        private ZWaveDefinition _zWaveDefinition;
        private NetworkViewPoint _networkViewPoint;

        public NodeTypeVMConverter(ZWaveDefinition zWaveDefinition, NetworkViewPoint networkViewPoint)
        {
            _zWaveDefinition = zWaveDefinition;
            _networkViewPoint = networkViewPoint;
        }

        public object Convert(object value, Type targetType, object param, CultureInfo culture)
        {
            if (value is NodeTag && _zWaveDefinition != null && _networkViewPoint != null)
            {
                var device = (NodeTag)value;
                string result = string.Empty;
                var nodeInfo = _networkViewPoint.GetNodeInfo(device);
                GenericDevice genericDevice = _zWaveDefinition.GenericDevices.FirstOrDefault(p => p.KeyId == nodeInfo.Generic);
                if (genericDevice != null)
                {
                    if (genericDevice.SpecificDevice != null && genericDevice.SpecificDevice.Count > 0 && nodeInfo.Specific > 0)
                    {
                        SpecificDevice specificDevice = genericDevice.SpecificDevice.FirstOrDefault(p => p.KeyId == nodeInfo.Specific);
                        if (specificDevice != null && specificDevice.Name != "SPECIFIC_TYPE_NOT_USED")
                            result = specificDevice.Text;
                    }

                    if (string.IsNullOrEmpty(result))
                        result = genericDevice.Text;
                }
                else
                {
                    BasicDevice basicDevice = _zWaveDefinition.BasicDevices.FirstOrDefault(p => p.KeyId == nodeInfo.Basic);
                    if (basicDevice != null)
                        result = basicDevice.Text;
                }
                if (device.EndPointId == 0)
                {
                    var securitySchemes = _networkViewPoint.GetSecuritySchemes(device.Id);
                    if (securitySchemes != null)
                    {
                        if (SecuritySchemeSet.ALLS2.Intersect(securitySchemes).Any())
                        {
                            result = "[S2] " + result;
                        }
                        else if (securitySchemes.Contains(SecuritySchemes.S0))
                        {
                            result = "[S0] " + result;
                        }
                    }
                }
                else
                {
                    //result = new string(new[] { '\u2022', '-', ' ' }) + result;
                    //result = ":: " + result;
                }

                return result;
            }
            return "n/a";
        }

        public object ConvertBack(object value, Type targetType, object param, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
