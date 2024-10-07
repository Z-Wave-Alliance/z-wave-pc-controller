/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;

namespace ZWaveController.Interfaces
{
    public interface ITransmitSettingsModel
    {
        short Measured0dBmPower { get; set; }
        short NormalTxPower { get; set; }
        RfRegions RfRegion { get; set; }
        LongRangeChannels LRChannel { get; set; }
        DcdcModes DcdcMode { get; set; }
        MaxLrTxPowerModes MaxLrTxPowerMode { get; set; }
        bool IsRfRegionLR { get; set; }
        bool IsRadioPTIEnabled { get; set; }
    }
}