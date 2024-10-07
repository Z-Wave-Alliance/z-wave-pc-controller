/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class DialogVMBase : VMBase
    {
        public virtual CommandBase CommandOk { get; }
        public virtual CommandBase CommandCancel { get; }
        public DialogSettings DialogSettings { get; set; }

        public DialogVMBase(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings = new DialogSettings { DialogName = GetType().Name };
            CommandOk = CommandsFactory.CommandBaseGet<CommandBase>(x =>
            {
                IsOk = true;
                Close();
            });
            CommandCancel = CommandsFactory.CommandBaseGet<CommandBase>(x =>
            {
                IsOk = false;
                Close();
            });
        }

        public override bool ShowDialog()
        {
            if (ShowDialogAction != null)
                ShowDialogAction(this);
            return base.ShowDialog();
        }
    }
}
