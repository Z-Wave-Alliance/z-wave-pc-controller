/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Enums;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SendDataSettingsViewModel : DialogVMBase, ISendDataSettingsModel
    {
        #region Commands
        public override CommandBase CommandOk => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            base.CommandOk.Execute(param);
            ApplySendDataSettingsCommand.Execute(null);
        }, param => !ApplicationModel.IsBusy);
        public ApplySendDataSettingsCommand ApplySendDataSettingsCommand => CommandsFactory.CommandControllerSessionGet<ApplySendDataSettingsCommand>();
        #endregion

        #region Properties
        private int _delayWakeUpNoMoreInformationMs = 100;
        public int DelayWakeUpNoMoreInformationMs
        {
            get { return _delayWakeUpNoMoreInformationMs; }
            set
            {
                _delayWakeUpNoMoreInformationMs = value;
                Notify("DelayWakeUpNoMoreInformationMs");
            }
        }

        private int _s0MaxBytesPerFrameSize = 26;
        public int S0MaxBytesPerFrameSize
        {
            get { return _s0MaxBytesPerFrameSize; }
            set
            {
                _s0MaxBytesPerFrameSize = value;
                Notify("S0MaxBytesPerFrameSize");
            }
        }

        private int _transportServiceMaxSegmentSize;
        public int TransportServiceMaxSegmentSize
        {
            get { return _transportServiceMaxSegmentSize; }
            set
            {
                _transportServiceMaxSegmentSize = value;
                Notify("TransportServiceMaxSegmentSize");
            }
        }

        private int _requestsTimeoutMs = 5000;
        public int RequestsTimeoutMs
        {
            get { return _requestsTimeoutMs; }
            set
            {
                _requestsTimeoutMs = value;
                Notify("RequestsTimeoutMs");
            }
        }

        private int _delayResponseMs;
        public int DelayResponseMs
        {
            get { return _delayResponseMs; }
            set
            {
                _delayResponseMs = value;
                Notify("DelayResponseMs");
            }
        }

        private int _inclusionControllerInitiateRequestTimeoutMs;
        public int InclusionControllerInitiateRequestTimeoutMs
        {
            get { return _inclusionControllerInitiateRequestTimeoutMs; }
            set
            {
                _inclusionControllerInitiateRequestTimeoutMs = value;
                Notify("InclusionControllerInitiateRequestTimeoutMs");
            }
        }

        private SupervisionReportStatuses _supervisionResportStatusResponse;
        public SupervisionReportStatuses SupervisionReportStatusResponse
        {
            get
            {
                return _supervisionResportStatusResponse;
            }
            set
            {
                _supervisionResportStatusResponse = value;
                Notify("SupervisionResportStatusResponse");
            }
        }

        #endregion

        public SendDataSettingsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = false;
            Title = "Send Data Settings";
        }
    }
}
