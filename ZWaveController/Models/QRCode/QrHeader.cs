/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Models
{
    public class QrHeader
    {
        public string LeadIn { get; set; }
        public string Version { get; set; }
        public string Checksum { get; set; }
        public string RequestedKeys { get; set; }
        public string DSK { get; set; }
    }
}
