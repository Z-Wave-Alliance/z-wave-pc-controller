/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegrationTests
{
    public class ReceivedLogItem
    {
        public byte SenderNodeId { get; private set; }
        public byte ReceiverNodeId { get; private set; }
        public byte[] Command { get; private set; }
        public byte SecurityScheme { get; private set; }

        public ReceivedLogItem(byte receiverNodeId, byte senderNodeId, byte[] command, byte securityscheme)
        {
            ReceiverNodeId = receiverNodeId;
            SenderNodeId = senderNodeId;
            Command = command;
            SecurityScheme = securityscheme;
        }
    }

}
