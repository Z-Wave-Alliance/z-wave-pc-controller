/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class StopPollingCommand : CommandBasicBase
    {
        public StopPollingCommand (IControllerSession controllerSession) : base(controllerSession)
        {
            UseBackgroundThread = true;
            _canExecute = param =>
                ApplicationModel.PollingModel.Nodes != null &&
                ApplicationModel.PollingModel.Nodes.Count > 0;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.PollingModel.IsTestReady;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.PollingService.PollingStop();
        }
    }
}
