/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface ITopologyMapModel
    {
        uint GetGenericDeviceColorArgb(byte genericDeviceId);
        Dictionary<NodeTag, uint> Legend { get; set; }
        BitMatrix Matrix { get; set; }
    }
}
