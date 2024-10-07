/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;

namespace ZWaveController.Interfaces
{
    public interface IBitFieldConfigurationParam : IDefaultConfigurationParamModel
    {
        List<ISelectableItem<int>> CheckBoxes { get; set; }
    }
}
