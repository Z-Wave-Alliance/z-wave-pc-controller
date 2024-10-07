/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;
using Utils;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class NVMBackupCommand : NVMCommandBase
    {
        public NVMBackupCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "NVM Backup";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !string.IsNullOrEmpty(NVMBackupRestoreModel.BackupFileName);
        }

        protected override bool CanCancelAction(object param)
        {
            return false;
        }

        public override CommandTypes CommandType
        {
            get
            {
                return CommandTypes.CmdZWaveNVMBackupRestore;
            }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("NVM Backup started.");
            Log("NVM Backup started.");

            NvmBackupRestoreOpenResult openResult = SessionDevice.NvmBackupRestoreOpen();
            if (openResult && openResult.Status == NvmBackupRestoreStatuses.OK)
            {
                byte packetLength = NVMBackupRestoreModel.PacketLength;
                byte actualPacketLength = 0;
                ushort offset = 0;
                var nvmData = new List<byte>();
                NvmBackupRestoreStatuses status = NvmBackupRestoreStatuses.Error;
                bool done = false;
                while (!done)
                {
                    NvmBackupRestoreReadResult readResult = SessionDevice.NvmBackupRestoreRead(packetLength, offset);
                    if (readResult && readResult.Data != null)
                    {
                        nvmData.AddRange(readResult.Data);
                        if (actualPacketLength != readResult.Length)
                        {
                            actualPacketLength = readResult.Length;
                            NVMBackupRestoreModel.ActualPacketLength = actualPacketLength;
                        }
                        offset += readResult.Length;
                        status = readResult.Status;
                        if (readResult.Status == NvmBackupRestoreStatuses.EOF ||
                            readResult.Status == NvmBackupRestoreStatuses.Error)
                        {
                            done = true;
                        }
                    }
                    else
                    {
                        ControllerSession.Logger.LogFail("NVM Backup operation failed");
                        done = true;
                    }
                }

                if (status == NvmBackupRestoreStatuses.EOF)
                {
                    // Save file
                    if (nvmData != null && nvmData.Count > 0)
                    {
                        try
                        {
                            string ext = Path.GetExtension(NVMBackupRestoreModel.BackupFileName);
                            switch (ext)
                            {
                                case (".zip"):
                                    BackupToZippedFile(nvmData);
                                    break;
                                case (".hex"):
                                // To write as hex file with specified extension
                                default:
                                    var nvmBytes = Encoding.UTF8.GetBytes(HexFileHelper.WriteIntelHexFile(nvmData.ToArray(), 0x10, 0xFF));
                                    File.WriteAllBytes(NVMBackupRestoreModel.BackupFileName, nvmBytes);
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            ControllerSession.Logger.LogFail("NVM Backup process failed. Error: " + e.Message);
#if DEBUG
                            throw e;
#endif
                        }
                        ControllerSession.Logger.LogOk("NVM Backup operation completed");
                    }
                }
                else if (status == NvmBackupRestoreStatuses.Error)
                {
                    ControllerSession.Logger.LogFail("NVM Backup operation failed");
                }
            }
            else
            {
                ControllerSession.Logger.LogFail("NVM Backup Open operation failed");
            }
            SessionDevice.NvmBackupRestoreClose();
        }

        private void BackupToZippedFile(List<byte> nvmData)
        {
            if (File.Exists(NVMBackupRestoreModel.BackupFileName))
            {
                File.Delete(NVMBackupRestoreModel.BackupFileName);
            }
            var nvmBytes = Encoding.UTF8.GetBytes(HexFileHelper.WriteIntelHexFile(nvmData.ToArray(), 0x10, 0xFF));
            AddByteArrayDataToZipArchive("ControllerNVMBackup.hex", nvmBytes);
            var ItemFileName = ConfigurationItem.GetItemFileName(ControllerSession.Controller.HomeId, ControllerSession.Controller.Network.NodeTag);
            if (File.Exists(ItemFileName))
            {
                string tempFile = File.ReadAllText(ItemFileName);
                var settingsBytes = Encoding.UTF8.GetBytes(tempFile);
                AddByteArrayDataToZipArchive(Path.GetFileName(ItemFileName), settingsBytes);
            }
        }

        private void AddByteArrayDataToZipArchive(string filename, byte[] byteArray)
        {
            using (Package zip = Package.Open(NVMBackupRestoreModel.BackupFileName, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + Path.GetFileName(filename);

                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                using (Stream dest = part.GetStream())
                {
                    MemoryStream stream = new MemoryStream(byteArray);

                    CopyStream(stream, dest);
                }
            }
        }

        private static void CopyStream(MemoryStream inputStream, Stream outputStream)
        {
            long bufferSize = inputStream.Length < 4096 ? inputStream.Length : 4096;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;
            }
        }
    }
}