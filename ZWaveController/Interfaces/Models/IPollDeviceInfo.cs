/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IPollDeviceInfo
    {
        int AvgCommandTime { get; set; }
        NodeTag Device { get; set; }
        int Failures { get; set; }
        bool IsPollEnabled { get; set; }
        DateTime LastPollTime { get; set; }
        int MaxCommandTime { get; set; }
        int MissingReports { get; set; }
        int PollTime { get; set; }
        int ReportTime { get; set; }
        int Requests { get; set; }
        int TotalCommandTime { get; set; }

        void Reset();
    }
}