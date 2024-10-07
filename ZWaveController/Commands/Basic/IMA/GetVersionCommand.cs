/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.CommandClasses;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class GetVersionCommand : IMACommandBase
    {
        const int REQUEST_TIMEOUT_10_SEC = 10000;
        public GetVersionCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health. Get Version.";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Get Version in progress.");

            var imaItems = ImaViewModel.GetSelectedItems();
            if (imaItems == null)
                imaItems = ImaViewModel.GetItems();
            if (imaItems != null)
            {
                ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
                var res = CommandExecutionResult.OK;
                foreach (IIMADeviceInfo item in imaItems)
                {
                    TargetDevice = item.Device;
                    byte[] rcmd = new COMMAND_CLASS_VERSION.VERSION_REPORT();
                    ControllerSession.RequestData(TargetDevice, new COMMAND_CLASS_VERSION.VERSION_GET(), ref rcmd, ControllerSession.Controller.Network.RequestTimeoutMs, _token);
                    if (IsCancelling)
                    {
                        res = CommandExecutionResult.Failed;
                        break;
                    }
                }
                ControllerSession.ApplicationModel.LastCommandExecutionResult = res;
            }
        }
    }
}

