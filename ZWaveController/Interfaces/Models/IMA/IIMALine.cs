/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Drawing;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IIMALine : IIMAEntity
    {
        NodeTag FromId { get; set; }
        bool IsValid { get; set; }
        Point Point { get; }
        IIMALine PreviousLine { get; set; }
        NodeTag RouteId { get; set; }
        Size Size { get; }
        Point StartPoint { get; }
        NodeTag ToId { get; set; }
        int X1 { get; }
        int X2 { get; }
        int Y1 { get; }
        int Y2 { get; }

        void UpdateFrom();
        void UpdateTo();
    }
}