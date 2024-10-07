/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class UnsolicitedDestApplyCommand : UnsolicitedDestBaseCommand
    {
        public UnsolicitedDestApplyCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return base.CanExecuteModelDependent(param) &&
                ControllerSession is ZipControllerSession &&
                !string.IsNullOrEmpty(ApplicationModel.SelectLocalIpAddressDialog.CustomPrimaryAddress) &&
                ApplicationModel.SelectLocalIpAddressDialog.CustomPrimaryPort > 0;
        }

        protected override void ExecuteInner(object param)
        {
            RestartUnsolicitedDestination(ApplicationModel.SelectLocalIpAddressDialog.CustomPrimaryAddress, 
                ApplicationModel.SelectLocalIpAddressDialog.CustomPrimaryPort);
        }
    }
}
