/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;
using ZWave.Security;

namespace ZWaveController.Interfaces
{
    public interface IMpanTableConfigurationModel
    {
        IList<IMpanItem> FullMpanTableBind { get; set; }
        IMpanItem SelectedMpanItem { get; set; }
        byte TestMpanGroupId { get; set; }
        NodeTag TestMpanOwner { get; set; }
        bool TestMpanIsMos { get; set; }
        byte[] TestMpanState { get; set; }
        byte[] TestMpanValue { get; set; }
        NodeTag[] TestMpanNodes { get; set; }
        byte TestMpanSequenceNumber { get; set; }
        NodeGroupId TestMpanNodeGroupId { get; }
        void AddMpanItem(MpanContainer mpanContainer);
        void ClearMpanTable();
    }
}