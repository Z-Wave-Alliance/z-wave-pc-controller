/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;

namespace ZWaveController.Configuration
{
    public interface IConfig
    {
        TransmitOptions TxOptions { get; set; }
        IControllerConfiguration ControllerConfiguration { get; }
        TransmitOptions NonListeningTxOptions { get; set; }
        ZWaveDefinition LoadZWaveDefinition(string xmlFilePath);
        ZWaveDefinition LoadZWaveDefinition();
        void DeleteConfiguration(IDevice controller);
    }
}