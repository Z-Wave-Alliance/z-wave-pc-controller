/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWaveController.Enums;

namespace ZWaveController.Models
{
    public class PayloadItem
    {
        public byte[] Payload { get; set; }
        public string CommandName { get; set; }
        public byte ClassId { get; set; }
        public byte CommandId { get; set; }
        public byte Version { get; set; }
        
        public SecureType SecureType { get; set; }
        //Options:
        public bool IsCrc16Enabled { get; set; }
        public bool IsSuppressMulticastFollowUp { get; set; }
        public bool IsForceMulticastEnabled { get; set; }
        public bool IsSupervisionGetEnabled { get; set; }
        public bool IsSupervisionGetStatusUpdatesEnabled { get; set; }
        public byte SupervisionSessionId { get; set; }
        public bool IsAutoIncSupervisionSessionId { get; set; }
        public bool IsMultiChannelEnabled { get; set; }
        public bool IsBitAddress { get; set; }
        public byte SourceEndPoint { get; set; }
        public byte DestinationEndPoint { get; set; }
    }
}
