/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class UnsolicitedDestSecondaryStartStopCommand : ControllerSessionCommandBase
    {
        public UnsolicitedDestSecondaryStartStopCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return base.CanExecuteModelDependent(param) && 
                ControllerSession is ZipControllerSession &&
                ApplicationModel.SelectLocalIpAddressDialog.SecondaryPort > 0;
        }

        protected override void ExecuteInner(object param)
        {
            var zipControllerSession = (ZipControllerSession)ControllerSession;
            if (ApplicationModel.SelectLocalIpAddressDialog.IsSecondaryOn)
            {
                ControllerSessionsContainer.Config.ControllerConfiguration.IsSecondaryUnsolicitedEnabled = false;
                zipControllerSession.StopSecondaryUnsolicitedListener();
            }
            else
            {
                ControllerSessionsContainer.Config.ControllerConfiguration.IsSecondaryUnsolicitedEnabled = true;
                zipControllerSession.RestartSecondaryUnsolicitedListener();
            }
        }
    }
}