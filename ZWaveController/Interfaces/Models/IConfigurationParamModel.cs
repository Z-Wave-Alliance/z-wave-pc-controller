/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;

namespace ZWaveController.Interfaces
{
    public interface IConfigurationParamModel
    {
        string Name { get; set; }
        string Info { get; set; }
        int Size { get; set; }
        byte[] ParameterNumber { get; set; }
        int DefaultValue { get; set; }
        ConfigurationFormatTypes Format { get; set; }
        byte[] NextParameterNumber { get; set; }
        IDefaultConfigurationParamModel Item { get; set; }
    }
}
