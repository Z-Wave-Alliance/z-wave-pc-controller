/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;

namespace ZWaveController.Interfaces
{
    public interface IScanDSKModel : IUserInput
    {
        bool IsHexInputChecked { get; set; }
    }

    public interface IScanDSKDialog : IDialog
    {
        IScanDSKModel State { get; set; }
        byte[] GetBytesFromInput(int bytesCount = 2);
    }
}