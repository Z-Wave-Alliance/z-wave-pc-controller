/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWave.Enums;
using ZWave.Security.S2;

namespace ZWaveController.Interfaces
{
    public interface IKEXSetConfirmDialog : IDialog
    {
        IKEXSetConfirmModel State { get; set; }
        KEXSetConfirmResult GetResult();
        void SetInputParameters(IEnumerable<SecuritySchemes> requestedSchemes,
            bool isClientSideAuthRequested,
            bool isSupportedSecurityS0,
            bool isSupportedSecurityS2_UNAUTHENTICATED,
            bool isSupportedSecurityS2_AUTHENTICATED,
            bool isSupportedSecurityS2_ACCESS);
    }

    public interface IKEXSetConfirmModel
    {
        bool IsCheckedCSA { get; set; }
        bool IsCheckedS0 { get; set; }
        bool IsCheckedS2_ACCESS { get; set; }
        bool IsCheckedS2_AUTHENTICATED { get; set; }
        bool IsCheckedS2_UNAUTHENTICATED { get; set; }
        bool IsEnabledS0 { get; set; }
        bool IsEnabledS2_ACCESS { get; set; }
        bool IsEnabledS2_AUTHENTICATED { get; set; }
        bool IsEnabledS2_UNAUTHENTICATED { get; set; }
        bool IsVisibleCSA { get; set; }
        bool IsVisibleS0 { get; set; }
        bool IsVisibleS2_ACCESS { get; set; }
        bool IsVisibleS2_AUTHENTICATED { get; set; }
        bool IsVisibleS2_UNAUTHENTICATED { get; set; }
    }
}