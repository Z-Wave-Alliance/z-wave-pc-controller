/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Commands
{
    public class ShowSendDataSettingsCommand : ControllerSessionCommandBase
    {
        public ShowSendDataSettingsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Show Send Data Settings command";
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        protected override void ExecuteInner(object param)
        {
            var device = ((ZWaveController.Models.BasicControllerSession)ControllerSession)._device;
            ApplicationModel.SendDataSettingsModel.DelayWakeUpNoMoreInformationMs = device.Network.DelayWakeUpNoMoreInformationMs;
            ApplicationModel.SendDataSettingsModel.S0MaxBytesPerFrameSize = device.Network.S0MaxBytesPerFrameSize;
            ApplicationModel.SendDataSettingsModel.TransportServiceMaxSegmentSize = device.Network.TransportServiceMaxSegmentSize;
            ApplicationModel.SendDataSettingsModel.RequestsTimeoutMs = device.Network.RequestTimeoutMs;
            ApplicationModel.SendDataSettingsModel.DelayResponseMs = device.Network.DelayResponseMs;
            ApplicationModel.SendDataSettingsModel.InclusionControllerInitiateRequestTimeoutMs = device.Network.InclusionControllerInitiateRequestTimeoutMs ?? 0;
            ApplicationModel.SendDataSettingsModel.SupervisionReportStatusResponse = device.Network.SupervisionReportStatusResponse;
            ((IDialog)ApplicationModel.SendDataSettingsModel).ShowDialog();
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsBusy && !ApplicationModel.IsActiveSessionZip;
        }
    }
}
