/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Text;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class ProvisioningListItemData
    {
        public PLMetaData Type { get; set; }
        public byte[] Value { get; set; }
        public bool IsCritical { get; set; }

        public static ProvisioningListItemData[] FromSmartStartModel(ISmartStartModel smartStartModel, bool isActiveSessionZip = false)
        {
            var ret = new List<ProvisioningListItemData>();
            if (!string.IsNullOrEmpty(smartStartModel.MetadaNameValue))
            {
                ret.Add(new ProvisioningListItemData
                {
                    IsCritical = false,
                    Type = PLMetaData.Name,
                    Value = System.Text.Encoding.Unicode.GetBytes(smartStartModel.MetadaNameValue)
                });
            }
            if (!string.IsNullOrEmpty(smartStartModel.MetadaLocationValue))
            {
                ret.Add(new ProvisioningListItemData
                {
                    IsCritical = false,
                    Type = PLMetaData.Location,
                    Value = System.Text.Encoding.Unicode.GetBytes(smartStartModel.MetadaLocationValue)
                });
            }
            if (isActiveSessionZip && 
                (smartStartModel.GetNodeOptions() & (byte)Modes.NodeOptionLongRange) == (byte)Modes.NodeOptionLongRange)
            {
                ret.Add(new ProvisioningListItemData
                {
                    IsCritical = true,
                    Type = PLMetaData.BootstrappingMode,
                    Value = new byte[] { 0x02 }
                });
            }
            return ret.ToArray();
        }

        public string ToStringFromValue()
        {
            if (Type == PLMetaData.Name || Type == PLMetaData.Location)
            {
                return Encoding.Unicode.GetString(Value);
            }
            else if (Value.Length == 1)
            {
                return $"0x{Value[0]:X2}";
            }
            return string.Empty;
        }
    }
}
