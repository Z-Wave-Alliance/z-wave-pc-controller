/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;

namespace ZWaveController.Enums
{
    [Flags]
    public enum NotifyProperty
    {
        None = 0,
        ControllerInfo = 1 << 0,
        NodesList = 1 << 1,
        MpanTable = 1 << 2,
        CommandSucceeded = 1 << 3,
        ToggleBasicTest = 1 << 4,
        ToggleSource = 1 << 5,
        ToggleErtt = 1 << 6,
        ErttList = 1 << 7,
        RedirectToHome = 1 << 8,
        RefreshNodeInfoCommandClasses = 1 << 9,
        RefreshProvisioningList = 1 << 10,
        OtaModel = 1 << 11,
        OtaDownloadComplete = 1 << 12,
        OtaUpdateComplete = 1 << 13,
        LastUsedPayload = 1 << 14,
        UnsolicitedDestination = 1 << 15,
        SpanChanged = 1 << 16
    }
}
