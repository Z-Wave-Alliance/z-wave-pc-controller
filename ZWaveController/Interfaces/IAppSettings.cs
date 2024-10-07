/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;

namespace ZWaveController.Interfaces
{
    public interface IAppSettings
    {
        bool SaveLastUsedDeviceSecondary { get; set;  }
        string LastUsedDevice { get; set; }
        string LastUsedDeviceAlt { get; set; }
        IDataSource SourceOnStartup { get; set; }

        void SaveSettings();
    }
}
