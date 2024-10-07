/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.ObjectModel;
using ZWaveController.Configuration;

namespace ZWaveController.Interfaces
{
    public interface ISmartStartModel
    {
        Collection<byte[]> DSK { get; set; }
        bool IsGrantS0Key { get; set; }
        bool IsGrantS2AccessKey { get; set; }
        bool IsGrantS2AuthenticatedKey { get; set; }
        bool IsGrantS2UnauthenticatedKey { get; set; }
        bool IsNodeOptionLongRange { get; set; }
        bool IsNodeOptionNormalPower { get; set; }
        bool IsNodeOptionNetworkWide { get; set; }
        bool IsMetadataEnabled { get; set; }
        bool IsRemoveDSK { get; set; }
        string MetadaLocationValue { get; set; }
        string MetadaNameValue { get; set; }
        object SelectedObject { get; set; }

        byte GetGrantSchemes();
        byte GetNodeOptions();
        void ResetFields();
        Tuple<bool,string> ValidateNewEntry(byte[] newitem, PreKitting configurationItemPreKitting);
        byte[] ParseDskFromLine(string line);

    }
}