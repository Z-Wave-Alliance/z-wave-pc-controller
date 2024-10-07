/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;

namespace ZWaveController.Interfaces
{
    public interface ISelectLocalIpAddressModel
    {
        SelectLocalIpViews CurrentViewType { get; set; }
        bool IsListening { get; set; }
        string StatusMessage { get; }
        string ActionButtonName { get; }
        string PrimaryAddress { get; set; }
        ushort PrimaryPort { get; set; }
        bool IsSecondaryOn { get; set; }
        ushort SecondaryPort { get; set; }
        string CustomPrimaryAddress { get; set; }
        ushort CustomPrimaryPort { get; set; }
    }
}