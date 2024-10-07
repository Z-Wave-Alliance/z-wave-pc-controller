/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI;

namespace ZWaveController.Configuration
{
    public class GenericDeviceTypeColorItem : EntityBase
    {
        public GenericDeviceTypeColorItem()
        {
        }
        public GenericDeviceTypeColorItem(byte genericDeviceId, string genericDeviceText, UInt32 colorArgb)
        {
            GenericDeviceId = genericDeviceId;
            GenericDeviceText = genericDeviceText;
            ColorArgb = colorArgb;
        }
        public byte GenericDeviceId { get; set; }
        public string GenericDeviceText { get; set; }
        public UInt32 ColorArgb { get; set; }
    }
}
