/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI;
using ZWave.Layers;

namespace ZWaveController.Configuration
{
    public interface IControllerConfiguration
    {
        List<string> ZipSourceIps { get; set; } // Legacy collection.
        List<DialogSettings> Dialogs { get; set; }
        string KeysStorageFolder { get; set; }
        int VersionSkipped { get; set; }
        bool IsSaveKeys { get; set; }
        bool UseFirmwareDataTruncated { get; set; }
        List<SocketDataSource> SocketSourcesIPs { get; set; }
        bool? IsUnsolicitedDestinationEnabled { get; set; }
        bool IsSecondaryUnsolicitedEnabled { get; set; }
        ushort UnsolicitedPortNoSecondary { get; set; }
        void Save();
    }
}