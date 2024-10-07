/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class StartStopCommand : CommandBasicBase
    {
        public StartStopCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsModelBusy = false;
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override void ExecuteInner(object param)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            StartStop();
            ret = CommandExecutionResult.OK;
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }

        internal void StartStop()
        {
            if (ControllerSession.ERTTService.IsTestReady)
            {
                ControllerSession.Logger.Log("Ertt Test started.");
                ControllerSession.ERTTService.Start();
            }
            else
            {
                ControllerSession.ERTTService.Stop();
                ControllerSession.Logger.Log("Ertt Test finished.");
            }
        }
    }
}