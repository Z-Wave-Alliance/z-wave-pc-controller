/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.UI.Wrappers;
using ZWave.Devices;

namespace ZWaveControllerUI.Drawing
{
    public interface ISelectableExt : ISelectable
    {
        NodeTag Id { get; set; }
        bool IsSelectedAsSource { get; set; }
        bool IsSelectedAsDestination { get; set; }
    }
}
