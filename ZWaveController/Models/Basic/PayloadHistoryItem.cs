/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;

namespace ZWaveController.Models
{
    public class PayloadHistoryItem
    {
        public uint Id { get; set; }
        public DateTime Timestamp { get; set; }
        public PayloadItem PayloadItem { get; set; }
        public PayloadHistoryItem() { }
        public PayloadHistoryItem(uint id, DateTime timestamp)
        {
            Id = id; //use ISenderHistoryModel.GetHistoryItemId()
            Timestamp = timestamp;
        }
        public PayloadHistoryItem(uint id, DateTime timestamp, PayloadItem payloadItem)
        {
            Id = id; //use ISenderHistoryModel.GetHistoryItemId()
            Timestamp = timestamp;
            PayloadItem = payloadItem;
        }
    }
}