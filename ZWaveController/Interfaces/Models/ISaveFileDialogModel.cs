/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿namespace ZWaveController.Interfaces
{
    public interface ISaveFileDialogModel
    {
        string FileName { get; set; }
        string Filter { get; set; }
        int FilterIndex { get; set; }
        bool IsOk { get; set; }
    }
}
