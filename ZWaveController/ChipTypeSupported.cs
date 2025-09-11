/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWave.Enums;

namespace ZWaveController
{
    public static class ChipTypeSupported
    {
        public static bool TransmitSettings(ChipTypes chipType)
        {
            return chipType == ChipTypes.ZW070x || chipType == ChipTypes.ZW080x || chipType == ChipTypes.ZW2000;
        }

        public static bool NetworkStatistics(ChipTypes chipType)
        {
            return chipType == ChipTypes.ZW070x || chipType == ChipTypes.ZW080x || chipType == ChipTypes.ZW2000;
        }

        public static bool XModem(ChipTypes chipType)
        {
            return chipType == ChipTypes.ZW070x || chipType == ChipTypes.ZW080x || chipType == ChipTypes.ZW2000;
        }

        internal static bool SoftResetAfterSetNodeInformation(ChipTypes chipType)
        {
            return chipType == ChipTypes.ZW070x || chipType == ChipTypes.ZW080x || chipType == ChipTypes.ZW2000;
        }
    }
}
