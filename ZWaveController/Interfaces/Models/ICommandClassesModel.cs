/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface ICommandClassesModel
    {
        List<NodeTag> SelectedNodeItems { get; set; }
        byte[] Payload { get; set; }
        bool IsSuppressMulticastFollowUp { get; set; }
        bool IsForceMulticastEnabled { get; set; }
        bool IsCrc16Enabled { get; set; }
        bool IsExpectCommand { get; set; }
        KeyValuePair<byte, byte> ExpectedCommand { get; set; }
        bool IsSupervisionGetEnabled { get; set; }
        bool IsSupervisionGetStatusUpdatesEnabled { get; set; }
        byte SupervisionSessionId { get; set; }
        bool IsAutoIncSupervisionSessionId { get; set; }
        bool IsMultiChannelEnabled { get; set; }
        SecureType SecureType { get; set; }
        byte SourceEndPoint { get; set; }
        byte DestinationEndPoint { get; set; }
        bool IsBitAddress { get; set; }
        ICommandClassesModel Clone();
        ICommandClassesModel Clone(PayloadItem payloadItem);
    }
}