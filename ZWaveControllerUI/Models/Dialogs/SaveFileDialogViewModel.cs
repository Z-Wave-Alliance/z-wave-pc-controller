/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SaveFileDialogViewModel : DialogVMBase, ISaveFileDialogModel
    {
        public string FileName { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }

        public SaveFileDialogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }
    }
}
