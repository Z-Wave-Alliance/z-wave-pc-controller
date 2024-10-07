/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ApplySendDataSettingsCommand : ControllerSessionCommandBase
    {
        public ApplySendDataSettingsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Apply Send Data Settings command";
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsBusy;
        }

        protected override void ExecuteInner(object param)
        {
            var targetNetwork = ControllerSession is BasicControllerSession ? ((BasicControllerSession)ControllerSession)._device.Network : ControllerSession.Controller.Network;
            targetNetwork.DelayWakeUpNoMoreInformationMs = ApplicationModel.SendDataSettingsModel.DelayWakeUpNoMoreInformationMs;
            targetNetwork.S0MaxBytesPerFrameSize = ApplicationModel.SendDataSettingsModel.S0MaxBytesPerFrameSize;
            targetNetwork.TransportServiceMaxSegmentSize = ApplicationModel.SendDataSettingsModel.TransportServiceMaxSegmentSize;
            targetNetwork.RequestTimeoutMs = ApplicationModel.SendDataSettingsModel.RequestsTimeoutMs;
            targetNetwork.DelayResponseMs = ApplicationModel.SendDataSettingsModel.DelayResponseMs;
            if (ApplicationModel.SendDataSettingsModel.InclusionControllerInitiateRequestTimeoutMs == 0)
            {
                targetNetwork.InclusionControllerInitiateRequestTimeoutMs = null;
            }
            else
            {
                targetNetwork.InclusionControllerInitiateRequestTimeoutMs = ApplicationModel.SendDataSettingsModel.InclusionControllerInitiateRequestTimeoutMs;
            }
            targetNetwork.SupervisionReportStatusResponse = ApplicationModel.SendDataSettingsModel.SupervisionReportStatusResponse;
            ControllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ApplySendDataSettingsCommand), Message = "Send Data Settings Applied" });
        }
    }
}
