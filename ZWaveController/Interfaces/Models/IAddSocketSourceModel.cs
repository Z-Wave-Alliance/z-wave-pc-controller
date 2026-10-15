// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
﻿using ZWave.Layers;

namespace ZWaveController.Interfaces
{
    public interface IAddSocketSourceModel
    {
        string Args { get; set; }
        string IPAddress { get; set; }
        int PortNo { get; set; }
        SoketSourceTypes SelectedSourceType { get; set; }
    }
}