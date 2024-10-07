/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Devices;
using ZWave.Security;

namespace ZWaveController.Interfaces
{
    public interface IMpanItem
    {
        byte GroupId { get; set; }
        NodeTag Owner { get; set; }
        bool IsMos { get; set; }
        byte[] MpanValue { get; set; }
        byte[] MpanState { get; set; }
        NodeTag[] Nodes { get; set; }
        byte SequenceNumber { get; set; }
        NodeGroupId NodeGroupId { get; }
    }
}