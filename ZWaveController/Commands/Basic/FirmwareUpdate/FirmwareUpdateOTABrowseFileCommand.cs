/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.IO;
using Utils;
using ZWaveController.Models;
using Utils.UI.Interfaces;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using System.Collections.Generic;

namespace ZWaveController.Commands
{
    public class FirmwareUpdateOTABrowseFileCommand : FirmwareUpdateCommandBase
    {
        public FirmwareUpdateOTABrowseFileCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsModelBusy = false;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                (ApplicationModel.IsActiveSessionZip || ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                FirmwareUpdateModel.FirmwareID != null &&
                FirmwareUpdateModel.SelectedFirmwareTarget != null;
        }

        protected override void ExecuteInner(object param)
        {
            if (param != null && param is string)
            {
                if (File.Exists((string)param))
                {
                    FirmwareUpdateModel.FirmwareFileName = (string)param;
                }
            }
            else
            {
                var openFileDlgVM = ControllerSession.ApplicationModel.OpenFileDialogModel;
                openFileDlgVM.Filter = "Hex files (*.hex;*.ota;*.otz;*.gbl)|*.hex;*.ota;*.otz;*.gbl|All Files|*.*";
                ((IDialog)openFileDlgVM).ShowDialog();
                if (openFileDlgVM.IsOk && !string.IsNullOrEmpty(openFileDlgVM.FileName))
                {
                    FirmwareUpdateModel.FirmwareFileName = openFileDlgVM.FileName;
                }
            }

            if (File.Exists(FirmwareUpdateModel.FirmwareFileName))
            {
                FirmwareUpdateModel.FirmwareDataFull = null;
                FirmwareUpdateModel.FirmwareChecksum = null;
                FirmwareUpdateModel.FirmwareData = null;
                int startAddress = 0;
                if (FirmwareUpdateModel.SelectedFirmwareTarget.Index == 0)
                {
                    try
                    {
                        FirmwareUpdateModel.FirmwareDataFull = HexFileHelper.GetBytes(HexFileHelper.ReadIntelHexFile(FirmwareUpdateModel.FirmwareFileName, 0xFF, out startAddress), 0xFF);
                    }
                    catch (FileFormatException)
                    {
                    }
                }
                if (FirmwareUpdateModel.FirmwareDataFull == null)
                {
                    FirmwareUpdateModel.FirmwareDataFull = File.ReadAllBytes(FirmwareUpdateModel.FirmwareFileName);
                    startAddress = 0;
                }
                FirmwareUpdateModel.FirmwareDataOffset = startAddress;
                FirmwareUpdateModel.FirmwareData = FirmwareUpdateModel.PrepareFirmwareData();
                if (FirmwareUpdateModel.FirmwareDataFull != null)
                {
                    FirmwareUpdateModel.FirmwareChecksum = Tools.CalculateCrc16Array(FirmwareUpdateModel.FirmwareDataFull);
                }
                else
                {
                    FirmwareUpdateModel.FirmwareChecksum = new byte[2];
                }

                var fwData = FirmwareUpdateModel.FragmentSize > 0 ?
                    Tools.ArraySplit(FirmwareUpdateModel.FirmwareData, FirmwareUpdateModel.FragmentSize) :
                    new List<byte[]>();
                if (fwData.Count == 0 || fwData.Count > short.MaxValue)
                {
                    //FirmwareUpdateModel.UpdateResultStatus = "The Loaded File is bigger than suppotrted by specification";
                    ControllerSession.ApplicationModel.ShowMessageDialog("File size to big",
                        "The fragments count of the uploaded file is more than supported by the specification, increase Fragment Size or use another file.");
                }
                ApplicationModel.NotifyControllerChanged(NotifyProperty.OtaModel);
            }
        }
    }
}
