/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;

namespace ZWaveController.Extentions
{
    public static class GetNodeTypeExtention
    {
        public static string GetControllerType(this NodeTag nodeTag, NetworkViewPoint network, ZWaveDefinition zWaveDefinition)
        {
            var ret = string.Empty;
            var deviceInfo = network.GetNodeInfo(nodeTag);
            var schemes = network.GetSecuritySchemes(nodeTag);
            var genericDevice = zWaveDefinition?.GenericDevices?.FirstOrDefault(p => p.KeyId == deviceInfo.Generic);
            if (genericDevice != null)
            {
                if (genericDevice.SpecificDevice != null && genericDevice.SpecificDevice.Count > 0 && deviceInfo.Specific > 0)
                {
                    var specificDevice = genericDevice.SpecificDevice.FirstOrDefault(p => p.KeyId == deviceInfo.Specific);
                    if (specificDevice != null)
                    {
                        ret = specificDevice.Text;
                    }
                }
                if (string.IsNullOrEmpty(ret))
                {
                    ret = genericDevice.Text;
                }
            }
            else
            {
                BasicDevice basicDevice = zWaveDefinition?.BasicDevices?.FirstOrDefault(p => p.KeyId == deviceInfo.Basic);
                if (basicDevice != null)
                {
                    ret = basicDevice.Text;
                }
            }
            if (schemes != null && schemes.Intersect(SecuritySchemeSet.ALLS2).Any())
            {
                ret = "[S2] " + ret;
            }
            else if (schemes != null && schemes.Contains(SecuritySchemes.S0))
            {
                ret = "[S0] " + ret;
            }
            return ret;
        }
    }
}
