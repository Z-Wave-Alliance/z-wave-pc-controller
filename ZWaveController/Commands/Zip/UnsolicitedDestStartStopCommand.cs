/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class UnsolicitedDestStartStopCommand : UnsolicitedDestBaseCommand
    {
        public UnsolicitedDestStartStopCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return base.CanExecuteModelDependent(param) && ControllerSession is ZipControllerSession;
        }

        protected override void ExecuteInner(object param)
        {
            var zipControllerSession = (ZipControllerSession)ControllerSession;
            if (ApplicationModel.SelectLocalIpAddressDialog.IsListening)
            {
                zipControllerSession.StopUnsolicitedListeners();
                ControllerSessionsContainer.Config.ControllerConfiguration.IsUnsolicitedDestinationEnabled = false;
            }
            else
            {
                RestartUnsolicitedDestination(ApplicationModel.SelectLocalIpAddressDialog.PrimaryAddress, 
                    ApplicationModel.SelectLocalIpAddressDialog.PrimaryPort);
                ControllerSessionsContainer.Config.ControllerConfiguration.IsUnsolicitedDestinationEnabled = true;
            }
        }
    }
}
