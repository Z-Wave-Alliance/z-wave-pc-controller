/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Threading;
using Utils;
using ZWave;
using ZWave.BasicApplication.Enums;
using ZWave.Xml.Application;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using System.Threading.Tasks;
using ZWave.Devices;
using ZWave.Enums;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTAUpdateCommand : FirmwareUpdateCommandBase
    {
        private bool _canCancelAction = true;
        private AutoResetEvent completedFirmwareUpdateSignal = new AutoResetEvent(false);
        const int MDGET_ON_CANCEL_WAIT = 20000;

        public FirmwareUpdateOTAUpdateCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "OTA Firmware Update";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                (ApplicationModel.IsActiveSessionZip || ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                FirmwareUpdateModel.FirmwareID != null &&
                FirmwareUpdateModel.FirmwareData != null &&
                FirmwareUpdateModel.SelectedFirmwareTarget != null &&
                Tools.ArraySplit(FirmwareUpdateModel.FirmwareData, FirmwareUpdateModel.FragmentSize).Count <= short.MaxValue;
        }

        protected override bool CanCancelAction(object param)
        {
            return _canCancelAction;
        }

        protected override void CancelAction(object param)
        {
            _canCancelAction = false;
            ControllerSession.ApplicationModel.SetBusyMessage("OTA Firmware Update cancelling…");
            if (responseMDReportActionToken != null)
            {
                ControllerSession.Cancel(TargetDevice, responseMDReportActionToken);
                responseMDReportActionToken.WaitCompletedSignal();
            }
            completedFirmwareUpdateSignal.Set();
            int elapsed = (int)(MDGET_ON_CANCEL_WAIT - (DateTime.Now - ControllerSession.ApplicationModel.FirmwareUpdateModel.LastMDGetTime).TotalMilliseconds);
            if (elapsed > 0)
            {
                var t = Task.Run(() =>
                {
                    if (FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 1)
                    {
                        ControllerSession.CancelFirmwareUpdateV1(TargetDevice, elapsed);
                    }
                    else if (FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 2)
                    {
                        ControllerSession.CancelFirmwareUpdateV2(TargetDevice, elapsed);
                    }
                    else if (FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 3)
                    {
                        ControllerSession.CancelFirmwareUpdateV3(TargetDevice, elapsed);
                    }
                    else if (FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 4)
                    {
                        ControllerSession.CancelFirmwareUpdateV4(TargetDevice, elapsed);
                    }
                    else if (FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 5)
                    {
                        ControllerSession.CancelFirmwareUpdateV5(TargetDevice, elapsed);
                    }
                });
            }

            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Canceled;
            Log("OTA Firmware Update cancelled.");
        }

        private void CancelTokens()
        {
            if (responseMDReportActionToken != null)
            {
                ControllerSession.Cancel(Device, responseMDReportActionToken);
                responseMDReportActionToken.WaitCompletedSignal();
                responseMDReportActionToken = null;
            }

            if (responseMDStatusReportActionToken != null)
            {
                ControllerSession.Cancel(Device, responseMDStatusReportActionToken);
                responseMDStatusReportActionToken.WaitCompletedSignal();
                responseMDStatusReportActionToken = null;
            }
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            _canCancelAction = true;
            CancelTokens();
            ControllerSession.OnMDStatusReportAction(Device, OnStatusReportCallback, responseMDStatusReportActionToken);

            ControllerSession.ApplicationModel.SetBusyMessage("OTA Firmware Update started.");
            Log("OTA Firmware Update started.");
            var fwData = ControllerSession.ApplicationModel.FirmwareUpdateModel.FragmentSize > 0 ?
                Tools.ArraySplit(ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareData, ControllerSession.ApplicationModel.FirmwareUpdateModel.FragmentSize) :
                new List<byte[]>();
            byte statusByte = 0;
            if (ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget != null)
            {
                if (ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 1)
                {
                    statusByte = ControllerSession.FirmwareUpdateV1(Device, fwData,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareChecksum,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.ManufacturerID,
                        out responseMDReportActionToken);
                }
                else if (ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 2)
                {
                    statusByte = ControllerSession.FirmwareUpdateV2(Device, fwData,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareChecksum,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.ManufacturerID,
                        out responseMDReportActionToken);
                }
                else if (ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 3)
                {
                    statusByte = ControllerSession.FirmwareUpdateV3(Device, fwData,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareChecksum,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.ManufacturerID,
                        (byte)ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FragmentSize,
                        out responseMDReportActionToken);
                }
                else if (ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 4)
                {
                    statusByte = ControllerSession.FirmwareUpdateV4(Device, fwData,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareChecksum,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.ManufacturerID,
                        (byte)ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FragmentSize,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.Activation,
                        out responseMDReportActionToken);
                }
                else if (ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 5)
                {
                    statusByte = ControllerSession.FirmwareUpdateV5(Device, fwData,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareChecksum,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.ManufacturerID,
                        (byte)ControllerSession.ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.FragmentSize,
                        ControllerSession.ApplicationModel.FirmwareUpdateModel.Activation,
                        (byte)ControllerSession.ApplicationModel.FirmwareUpdateModel.HardwareVersion,
                        out responseMDReportActionToken);
                }
                if (statusByte == 0xFF)
                {
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
                    completedFirmwareUpdateSignal.Reset();
                    completedFirmwareUpdateSignal.WaitOne();

                    ControllerSession.Cancel(Device, responseMDReportActionToken);
                    responseMDReportActionToken = null;

                    ControllerSession.Cancel(Device, responseMDStatusReportActionToken);
                    responseMDStatusReportActionToken = null;
                }
                else
                {
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
                    CancelTokens();
                }
            }

            ApplicationModel.NotifyControllerChanged(NotifyProperty.OtaUpdateComplete);
        }

        private byte[] OnStatusReportCallback(ReceiveStatuses options, NodeTag destNode, NodeTag srcNode, byte[] data)
        {
            CommandClassValue[] cmdClassValues = null;
            ControllerSession.ApplicationModel.ZWaveDefinition.ParseApplicationObject(data, out cmdClassValues);
            if (cmdClassValues != null && cmdClassValues.Length > 0)
            {
                CommandValue cmdValue = cmdClassValues[0].CommandValue; ;
                if (cmdValue != null && cmdValue.ParamValues != null && cmdValue.ParamValues.Count > 0)
                {
                    ControllerSession.ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = cmdValue.ParamValues[0].TextValue;
                    Log(cmdValue.ParamValues[0].TextValue);
                }
            }
            completedFirmwareUpdateSignal.Set();
            return null;
        }

        ActionToken responseMDReportActionToken = null;
        ActionToken responseMDStatusReportActionToken = null;
    }
}
