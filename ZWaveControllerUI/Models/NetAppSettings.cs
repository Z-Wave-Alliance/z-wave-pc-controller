/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using ZWaveController.Interfaces;
using ZWaveControllerUI.Properties;

namespace ZWaveControllerUI.Models
{
    public class NetAppSettings : IAppSettings
    {
        public bool SaveLastUsedDeviceSecondary { get; set; }
        public string LastUsedDevice { get => Settings.Default.LastUsedDevice; set => Settings.Default.LastUsedDevice = value; }
        public string LastUsedDeviceAlt { get => Settings.Default.LastUsedDeviceAlt; set => Settings.Default.LastUsedDeviceAlt = value; }
        public IDataSource SourceOnStartup { get; set; }

        public void SaveSettings()
        {
            Settings.Default.Save();
        }
    }
}
