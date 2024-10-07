/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SecuritySchemaDialogViewModel : UserInputDialogViewModel, ISecuritySchemaDialog
    {
        public SecuritySchemaDialogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }
    }
}
