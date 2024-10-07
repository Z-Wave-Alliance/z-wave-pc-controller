/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface INetworkManagementModel
    {
        string BasicTestCaption { get; set; }
        bool IsBasicTestStarted { get; set; }
        ActionToken BasicTestToken { get; set; }
        byte RemoveNodeId { get; set; }
        byte SendNopNodeId { get; set; }
        int WakeupIntervalValue { get; set; }
        NodeTag[] SelectedNodeItems { get; set; }
    }
}