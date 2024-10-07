/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Wrappers;
using ZWave.Devices;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface IIMAEntity: ISelectable
    {
        NodeTag Id { get; set; }
        bool IsItemSelected { get; set; }
        NetworkCanvasLayout Layout { get; set; }
        IMAEntityTypes Type { get; set; }
    }
}