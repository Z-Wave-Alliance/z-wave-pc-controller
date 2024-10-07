/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class PingNodeCommand : IMACommandBase
    {
        public PingNodeCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health.";
            IsCancelAtController = true;
        }

        public override CommandTypes CommandType
        {
            get
            {
                return CommandTypes.CmdZWaveSendData;
            }
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Ping in progress.");
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
                    ControllerSession.SendNop(TargetDevice, out _token);
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
