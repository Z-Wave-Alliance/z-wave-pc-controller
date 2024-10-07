/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;

namespace ZWaveController.Enums
{
    [Flags]
    public enum DeviceOptionsView
    {
        /// <summary>
        /// Do node contain optional fundtionality.
        /// </summary>
        OptionalFunctionality = 0x02,
        /// <summary>
        /// Is Node a FLiRS 1000ms node.
        /// </summary>
        FreqListeningMode1000ms = 0x10,
        /// <summary>
        /// Is Node a FLiRS 250ms node.
        /// </summary>
        FreqListeningMode250ms = 0x20
    }
}
