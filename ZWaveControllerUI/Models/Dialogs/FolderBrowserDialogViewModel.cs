/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class FolderBrowserDialogViewModel : DialogVMBase
    {
        public string FolderPath { get; set; }

        public FolderBrowserDialogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }
    }
}
