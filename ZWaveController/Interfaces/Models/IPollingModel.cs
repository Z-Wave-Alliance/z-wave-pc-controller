/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI.Bind;

namespace ZWaveController.Interfaces
{
    public interface IPollingModel
    {
        int Counter { get; set; }
        TimeSpan Duration { get; set; }
        bool IsTestReady { get; set; }
        ISubscribeCollection<IPollDeviceInfo> Nodes { get; set; }
        int ResetSpanMode { get; set; }
        bool UseBasicCC { get; set; }
        bool UseResetSpan { get; set; }
        object NodesOperationsLockObject { get; }
    }
}