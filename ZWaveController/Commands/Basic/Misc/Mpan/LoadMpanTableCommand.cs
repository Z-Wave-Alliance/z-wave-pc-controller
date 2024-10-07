/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class LoadMpanTableCommand : CommandBasicBase
    {

        public LoadMpanTableCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Load Mpan table";
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is BasicControllerSession;
        }

        protected override void ExecuteInner(object parameter)
        {
            ControllerSession.LoadMpan();            
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData {CommandName = nameof(LoadMpanTableCommand), Message = "Mpan Table Loaded" });
        }
    }
}
