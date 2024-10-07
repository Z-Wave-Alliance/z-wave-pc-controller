/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using System.Text;
using Utils;
using System.IO;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTASaveCommand : FirmwareUpdateCommandBase
    {
        public FirmwareUpdateOTASaveCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "OTA Firmware Save";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return FirmwareUpdateModel.DownloadFirmwareData != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            string fileName = string.Empty;
            ControllerSession.ApplicationModel.SaveFileDialogModel.Filter = "HEX File (*.hex)|*.hex|All Files|*.*";
            if (((IDialog)ControllerSession.ApplicationModel.SaveFileDialogModel).ShowDialog() && !string.IsNullOrEmpty(ControllerSession.ApplicationModel.SaveFileDialogModel.FileName))
            {
                fileName = ControllerSession.ApplicationModel.SaveFileDialogModel.FileName;
                var nvmBytes = Encoding.UTF8.GetBytes(HexFileHelper.WriteIntelHexFile(FirmwareUpdateModel.DownloadFirmwareData, 0x10, 0xFF));
                File.WriteAllBytes(fileName, nvmBytes);
                Log("Firmware saved to file: " + fileName);
            }
        }
    }
}
