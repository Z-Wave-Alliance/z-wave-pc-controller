/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using System.IO;
using System.Text;
using Utils;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;
using ZWaveController.Enums;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTADownloadCommand : FirmwareUpdateCommandBase
    {
        public FirmwareUpdateOTADownloadCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "OTA Firmware Download";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                (ApplicationModel.IsActiveSessionZip || ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                ControllerSession.ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion > 4 &&
                FirmwareUpdateModel.FirmwareID != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void CancelAction(object param)
        {
            ControllerSession.CancelFirmwareUpdateOTADownload();
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            FirmwareUpdateModel.DownloadFirmwareData = null;
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ControllerSession.FirmwareUpdateOTADownload(Device, out _token);
            if (FirmwareUpdateModel.DownloadFirmwareData != null)
            {
                string filePath = string.Empty;
                if (param != null)
                {
                    filePath = (string)param;
                }
                else
                {
                    ControllerSession.ApplicationModel.SaveFileDialogModel.Filter = "HEX File (*.hex)|*.hex|All Files|*.*";
                    if (((IDialog)ControllerSession.ApplicationModel.SaveFileDialogModel).ShowDialog() && !string.IsNullOrEmpty(ControllerSession.ApplicationModel.SaveFileDialogModel.FileName))
                    {
                        filePath = ControllerSession.ApplicationModel.SaveFileDialogModel.FileName;
                    }
                }

                var nvmBytes = Encoding.UTF8.GetBytes(HexFileHelper.WriteIntelHexFile(FirmwareUpdateModel.DownloadFirmwareData, 0x10, 0xFF));
                File.WriteAllBytes(filePath, nvmBytes);
                Log("Firmware saved to file: " + filePath);
            }

            ApplicationModel.NotifyControllerChanged(NotifyProperty.OtaDownloadComplete, ApplicationModel.FirmwareUpdateModel);
        }

    }
}
