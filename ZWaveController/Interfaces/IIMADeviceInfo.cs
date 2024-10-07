/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IIMADeviceInfo
    {
        NodeTag Id { get; set; }
        List<IIMALine> StartLines { get; set; }
        List<IIMALine> EndLines { get; set; }
        bool IsInProgress { get; set; }
        byte? NHV { get; set; }
        int? RC { get; }
        bool? LWRdB { get; }
        int? PER { get; }
        byte? NB { get; }
        sbyte? LWRRSSI { get; }
        NodeTag Device { get; set; }
        void ClearIMATestResult();
        void AddIMATestResult(IIMATestResult imaRes);
        IIMATestResult CreateTestResult(int testRound);
        NodeTag[] Neighbors { get; set; }
        bool IsSelected { get; set; }
    }
}
