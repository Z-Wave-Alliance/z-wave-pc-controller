/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;

namespace ZWaveController.Interfaces
{
    public interface IAddNodeWithCustomSettingsModel
    {
        bool IsAssignReturnRoute { get; set; }
        bool IsAssociationCreate { get; set; }
        bool IsDeleteReturnRoute { get; set; }
        bool IsMultichannelAssociationCreate { get; set; }
        bool IsWakeUpCapabilities { get; set; }
        bool IsWakeUpInterval { get; set; }
        bool IsSetAsSisAutomatically { get; set; }
        bool IsBasedOnZwpRoleType { get; set; }

        SetupNodeLifelineSettings GetSetupNodeLifelineSettings();
    }
}