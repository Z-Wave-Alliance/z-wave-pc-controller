/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ReloadXmlCommand : CommandBasicBase
    {
        public ReloadXmlCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Reloading Xml.");
            Log("Reloading Xml started.");

            var ret = CommandExecutionResult.OK;
            try
            {
                ControllerSession.ApplicationModel.ZWaveDefinition = ControllerSession.ApplicationModel.Config.LoadZWaveDefinition();
                Log("Reloading Xml completed");
            }
            catch
            {
                ret = CommandExecutionResult.Failed;
                ControllerSession.LogError("Reloading Xml failed with state {0}", ret);
            }
        }
    }
}