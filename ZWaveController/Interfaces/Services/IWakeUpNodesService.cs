/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Concurrent;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Commands;
using ZWaveController.Services;

namespace ZWaveController.Interfaces
{
    public interface IWakeUpNodesService
    {
        bool IsWakeupDelayed { get; set; }
        ConcurrentDictionary<NodeTag, WakeUpMonitorContainer> WakeUpNodeHealthStatuses { get; set; }

        void Enqueue(NodeTag node, CommandBase command, int delayMs);
        void RxReset(NodeTag node);
        void RxSet(NodeTag node);
        void RxWait(NodeTag node, int timeoutMs);
        void Start();
        void Stop();
        void WakeUp(NodeTag node, SecuritySchemes securityScheme, ReceiveStatuses receiveStatus);
    }
}