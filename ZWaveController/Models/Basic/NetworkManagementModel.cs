/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave;
using ZWave.Devices;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class NetworkManagementModel : INetworkManagementModel
    {
        public string BasicTestCaption { get; set; }
        public bool IsBasicTestStarted { get; set; }
        public ActionToken BasicTestToken { get; set; }
        public byte SendNopNodeId { get; set; }
        public byte RemoveNodeId { get; set; }
        public int WakeupIntervalValue { get; set; } = 5;
        public NodeTag[] SelectedNodeItems { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public ResetSPANCommand ResetSPANCommand { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public NextSPANCommand NextSPANCommand { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int DelayResponseSec { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool IsDelayResponseEnabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}