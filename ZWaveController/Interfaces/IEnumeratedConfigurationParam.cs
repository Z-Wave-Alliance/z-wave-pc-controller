/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface IEnumeratedConfigurationParam : IDefaultConfigurationParamModel
    {
        List<ISelectableItem<RadioButtonsWrapper>> RadioButtons { get; set; }
    }
}
