/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface IIMATestResult
    {
        int IterationNo { get; set; }
        bool? LWRdB { get; set; }
        sbyte? LWRRSSI { get; set; }
        byte? NB { get; set; }
        byte NHV { get; set; }
        int PER { get; set; }
        int? RC { get; set; }
    }
}