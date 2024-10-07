/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class OpenFileDialogViewModel : DialogVMBase, IOpenFileDialogModel
    {
        public string FileName { get; set; }
        public string Filter { get; set; }

        public OpenFileDialogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }
    }
}
