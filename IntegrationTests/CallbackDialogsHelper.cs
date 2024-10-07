/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegrationTests
{
    public static class CallbackDialogsHelper
    {
        public static void ResetValues()
        {
            IsOkDSKNeeded = true;
            IsOkDSKVerification = true;
            IsOkDskPin = true;
            IsOkCsaPin = true;
            IsOkKEXSetConfirm = true;
            IsWrongDsk = false;
            IsWrongCsa = false;

            DelayCsaPin = 0;
            DelayDSKNeeded = 0;
            DelayDskPin = 0;
            DelayDSKVerification = 0;
            DelayKEXSetConfirm = 0;
        }

        public static int DelayDSKNeeded { get; set; }
        public static int DelayDSKVerification { get; set; }
        public static int DelayDskPin { get; set; }
        public static int DelayCsaPin { get; set; }
        public static int DelayKEXSetConfirm { get; set; }

        public static bool WasShownDSKNeeded { get; set; }
        public static bool WasSnownDSKVerification { get; set; }
        public static bool WasSnownDskPin { get; set; }
        public static bool WasSnownCsaPin { get; set; }
        public static bool WasSnownKEXSetConfirm { get; set; }

        public static bool IsOkDSKNeeded { get; set; }
        public static bool IsOkDSKVerification { get; set; }
        public static bool IsOkDskPin { get; set; }
        public static bool IsOkCsaPin { get; set; }
        public static bool IsOkKEXSetConfirm { get; set; }

        public static bool IsWrongDsk { get; set; }
        public static bool IsWrongCsa { get; set; }

        public static string DskPin;
        public static string CsaPin;
    }
}
