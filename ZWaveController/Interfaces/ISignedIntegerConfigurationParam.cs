/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface ISignedIntegerConfigurationParam : IDefaultConfigurationParamModel
    {
        int Value { get; set; }
        int MinValue { get; set; }
        int MaxValue { get; set; }
    }
}
