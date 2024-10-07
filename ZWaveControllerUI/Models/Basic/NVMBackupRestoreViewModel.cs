/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class NVMBackupRestoreViewModel : VMBase, INVMBackupRestoreModel
    {
        #region Commands

        public CommandBase NVMBackupGetFileCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            string fileName = string.Empty;
            ApplicationModel.SaveFileDialogModel.Filter = "Zip archive with PC Controller backup included (*.zip)|*.zip|HEX File - Device NVM memory snapshot only (*.hex)|*.hex|All Files|*.*";
            if (((IDialog)ApplicationModel.SaveFileDialogModel).ShowDialog() && !string.IsNullOrEmpty(ApplicationModel.SaveFileDialogModel.FileName))
            {
                fileName = ApplicationModel.SaveFileDialogModel.FileName;
            }
            ApplicationModel.NVMBackupRestoreModel.BackupFileName = fileName;
        }, param => ApplicationModel.Controller is ZWave.Devices.IController);
        public CommandBase NVMRestoreGetFileCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            string fileName = string.Empty;
            ApplicationModel.OpenFileDialogModel.Filter = "Zip archive with PC Controller backup included(*.zip)|*.zip|Hex files - doesn't restore Security Settings(*.hex)|*.hex|All Files|*.*";
            if (((IDialog)ApplicationModel.OpenFileDialogModel).ShowDialog() && !string.IsNullOrEmpty(ApplicationModel.OpenFileDialogModel.FileName))
            {
                fileName = ApplicationModel.OpenFileDialogModel.FileName;
            }
            ApplicationModel.NVMBackupRestoreModel.RestoreFileName = fileName;
        }, param => ApplicationModel.Controller is ZWave.Devices.IController);
        public NVMBackupCommand NVMBackupCommand => CommandsFactory.CommandControllerSessionGet<NVMBackupCommand>();
        public NVMRestoreCommand NVMRestoreCommand => CommandsFactory.CommandControllerSessionGet<NVMRestoreCommand>();

        #endregion

        private string _backupFileName;
        public string BackupFileName
        {
            get { return _backupFileName; }
            set
            {
                _backupFileName = value;
                Notify("BackupFileName");
            }
        }

        private string _restoreFileName;
        public string RestoreFileName
        {
            get { return _restoreFileName; }
            set
            {
                _restoreFileName = value;
                Notify("RestoreFileName");
            }
        }

        private byte _packetLength = 0x40;
        public byte PacketLength
        {
            get { return _packetLength; }
            set
            {
                _packetLength = value;
                Notify("PacketLength");
            }
        }

        private byte _actualPacketLength;
        public byte ActualPacketLength
        {
            get { return _actualPacketLength; }
            set
            {
                _actualPacketLength = value;
                Notify("ActualPacketLength");
            }
        }

        public NVMBackupRestoreViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Backup/Restore";
        }
    }
}
