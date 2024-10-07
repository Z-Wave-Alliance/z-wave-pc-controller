/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Collections.Generic;
using Utils;
using Utils.UI.Interfaces;
using ZWave.BasicApplication.Operations;
using ZWave.Enums;
using ZWaveController.Models;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateLocalCommand : CommandBasicBase
    {
        private const string BASIC_DIALOG_FILTERS = "Hex files (*.hex;*.ota;*.otz)|*.hex;*.ota;*otz|All Files|*.*";
        private const string XMODEM_DIALOG_FILTERS = "XModem Hex files (*.gbl)|*.gbl";
        public FirmwareUpdateLocalCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Firmware Update Local";
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsActiveSessionZip;
        }

        protected override bool CanCancelAction(object param)
        {
            return false;
        }

        protected override void ExecuteInner(object param)
        {
            string filesFilter = null;
            bool isXModemSupported = false;
            if (ChipTypeSupported.XModem(ControllerSession.ApplicationModel.Controller.ChipType))
            {
                filesFilter = XMODEM_DIALOG_FILTERS;
                isXModemSupported = true;
            }
            else if (ControllerSession.ApplicationModel.Controller.ChipType == ChipTypes.ZW050x)
            {
                filesFilter = BASIC_DIALOG_FILTERS;
            }
            else
            {
                ControllerSession.LogError("Firmware update failed: unsupported Device Chip Type.");
                return;
            }
            string fwFileName = string.Empty;
            ControllerSession.ApplicationModel.SetBusyMessage("Local Firmware Update started.");
            if (param != null && param is string)
            {
                if (File.Exists((string)param))
                {
                    fwFileName = (string)param;
                }
            }
            else
            {
                var openFileDlgVM = ControllerSession.ApplicationModel.OpenFileDialogModel;
                openFileDlgVM.Filter = filesFilter;
                ((IDialog)openFileDlgVM).ShowDialog();
                if (openFileDlgVM.IsOk && !string.IsNullOrEmpty(openFileDlgVM.FileName))
                {
                    fwFileName = openFileDlgVM.FileName;
                }
            }
            if (!string.IsNullOrEmpty(fwFileName))
            {
                Log("Local Firmware Update started.");
                if (isXModemSupported)
                {
                    FirmwareUpdateXModem(fwFileName);
                }
                else
                {
                    FirmwareUpdateNwm(fwFileName);
                }
            }
        }

        private void FirmwareUpdateNwm(string fwFileName)
        {
            int LOCAL_FIRMWARE_FRAGMENT_SIZE = 0x80;
            FirmwareUpdateNvmInitResult actionResult = SessionDevice.FirmwareUpdateNvmInit();
            bool updateCompleted = false;
            if (actionResult.IsStateCompleted && actionResult.IsSupported)
            {
                var fwFileExt = Path.GetExtension(fwFileName);
                bool isFillWithBlanksNeeded = fwFileExt != ".ota" && fwFileExt != ".otz";
                byte[] hexData = HexFileHelper.GetBytes(HexFileHelper.ReadIntelHexFile(fwFileName, 0xFF), 128 * 1024, 0xFF, isFillWithBlanksNeeded);
                SessionDevice.FirmwareUpdateNvmSetNewImage(false);
                List<byte[]> firmwareData = Tools.ArraySplit(hexData, LOCAL_FIRMWARE_FRAGMENT_SIZE);

                int firmwareOffset = 0;
                for (int i = 0; i < firmwareData.Count; i++)
                {
                    ControllerSession.ApplicationModel.SetBusyMessage("Please wait until the Local Firmware Update completed.\n" +
                    $"packet# {(i + 1).ToString()} of {firmwareData.Count} written.", i == 0 || i % 100 == 0 ? (int)(Math.Round((double)i / firmwareData.Count, 2) * 100) : -1);

                    FirmwareUpdateNvmWriteResult updateNvmWrite = SessionDevice.FirmwareUpdateNvmWrite(firmwareOffset, (ushort)firmwareData[i].Length, firmwareData[i]);
                    if (!updateNvmWrite.IsStateCompleted)
                    {
                        ControllerSession.LogError("Firmware update failed.");
                        ((BasicControllerSession)ControllerSession).SetNewImageCompletedCallback(false);
                        return;
                    }
                    firmwareOffset += LOCAL_FIRMWARE_FRAGMENT_SIZE;
                }
                updateCompleted = SessionDevice.FirmwareUpdateNvmSetNewImage(true).IsStateCompleted;
                if (updateCompleted)
                {
                    Log("Firmware Update Locally succeeded.");
                    ControllerSession.ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(FirmwareUpdateLocalCommand), Message = "Firmware Update Locally succeeded." });
                }
                else
                {
                    ControllerSession.LogError("Firmware update failed.");
                }
                ((BasicControllerSession)ControllerSession).SetNewImageCompletedCallback(updateCompleted);
            }
            else
            {
                ControllerSession.LogError("Firmware update initialization failed.");
            }
        }

        private void FirmwareUpdateXModem(string fwFileName)
        {
            if (!ApplicationModel.IsActiveSessionZip)
            {
                var hexData = File.ReadAllBytes(fwFileName);
                SessionDevice.Disconnect();
                var res = ((BasicControllerSession)ControllerSession).FirmwareUpdateXModemCallback(hexData);
                if ((res.UpdateStatus || res.IsKeyValidationFailed) && SessionDevice.Connect() == CommunicationStatuses.Done) // Can reconnect.
                {
                    ((BasicControllerSession)ControllerSession).SetNewImageCompletedCallback(res.UpdateStatus);
                }
            }
        }
    }
}
