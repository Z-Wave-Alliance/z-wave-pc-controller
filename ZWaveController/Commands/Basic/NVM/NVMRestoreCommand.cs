/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Threading;
using Utils;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class NVMRestoreCommand : NVMCommandBase
    {
        public NVMRestoreCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "NVM Restore";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !string.IsNullOrEmpty(NVMBackupRestoreModel.RestoreFileName);
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
            ControllerSession.ApplicationModel.SetBusyMessage("NVM Restore started.");
            Log("NVM Restore started.");
            var tempSrc = string.Empty;
            try
            {
                NvmBackupRestoreOpenResult openResult = SessionDevice.NvmBackupRestoreOpen();
                if (openResult && openResult.Status == NvmBackupRestoreStatuses.OK)
                {
                    if (Path.GetExtension(NVMBackupRestoreModel.RestoreFileName) == ".zip")
                    {
                        using (Package package = Package.Open(NVMBackupRestoreModel.RestoreFileName, FileMode.Open, FileAccess.Read))
                        {
                            var parts = package.GetParts();
                            if (!parts.IsNullOrEmpty())
                            {
                                foreach (PackagePart part in parts)
                                {
                                    var target = part.Uri.OriginalString;
                                    using (StreamReader source = new StreamReader(part.GetStream(FileMode.Open, FileAccess.Read)))
                                    {
                                        if (Path.GetExtension(target) == ".hex")
                                        {
                                            SortedList<short, List<byte>> _data = HexFileHelper.ReadIntelHexStream(source, 0xFF);
                                            byte[] _nvmData = HexFileHelper.GetBytes(_data, 0xFF);
                                            WriteNVMData(_nvmData);
                                        }
                                        else if (Path.GetExtension(target) == ".xml")
                                        {
                                            var settingsPath = ConfigurationItem.CONFIG_PATH;
                                            var appdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), settingsPath);
                                            var destFileName = Path.Combine(appdataPath, Path.GetFileName(target));
                                            if (File.Exists(destFileName))
                                            {
                                                tempSrc = source.ReadToEnd();
                                                File.WriteAllText(destFileName, tempSrc);
                                            }
                                            else
                                            {
                                                tempSrc = string.Empty;
                                                File.WriteAllText(destFileName, source.ReadToEnd());
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var ItemFileName = ConfigurationItem.GetItemFileName(ControllerSession.Controller.HomeId, ControllerSession.Controller.Network.NodeTag);
                                tempSrc = File.ReadAllText(ItemFileName);
                                ControllerSession.Logger.LogFail($"Restore File {Path.GetFileName(NVMBackupRestoreModel.RestoreFileName)} is empty.");
                            }
                        }
                    }
                    else if (Path.GetExtension(NVMBackupRestoreModel.RestoreFileName) == ".hex")
                    {
                        SortedList<short, List<byte>> _data = HexFileHelper.ReadIntelHexFile(NVMBackupRestoreModel.RestoreFileName, 0xFF);
                        byte[] _nvmData = HexFileHelper.GetBytes(_data, 0xFF);
                        WriteNVMData(_nvmData);
                    }
                    else
                    {
                        Log("Try to load NVM from file " + Path.GetFileName(NVMBackupRestoreModel.RestoreFileName));
                        SortedList<short, List<byte>> _data = HexFileHelper.ReadIntelHexFile(NVMBackupRestoreModel.RestoreFileName, 0xFF);
                        byte[] _nvmData = HexFileHelper.GetBytes(_data, 0xFF);
                        WriteNVMData(_nvmData);
                    }
                }
                else
                {
                    ControllerSession.Logger.LogFail("NVM Restore Open operation failed");
                }
            }
            catch (FileFormatException ffex)
            {
                ControllerSession.Logger.LogFail(ffex.Message);
            }
            finally
            {
                SessionDevice.NvmBackupRestoreClose();
            }

            var configFileName = ConfigurationItem.GetItemFileName(ControllerSession.Controller.HomeId, ControllerSession.Controller.Network.NodeTag);
            SessionDevice.SoftReset();
            ControllerSession.Disconnect();
            if (!string.IsNullOrEmpty(tempSrc))
            {
                //ControllerSessionsContainer.Config.DeleteConfiguration(Controller);
                // If HomeId is not changed to avoid recrating config after ControllerSession.Disconnect()
                // And use config from restored file.
                File.Delete(configFileName);
                File.WriteAllText(configFileName, tempSrc);
            }
            Log("Reopen device");
            Thread.Sleep(2000);
            var openRes = ControllerSession.Connect(ControllerSession.DataSource);
            if (openRes != ZWave.Enums.CommunicationStatuses.Done)
            {
                ControllerSession.LogError("Power cycle connected device and press Ok");
                ControllerSession.Connect(ControllerSession.DataSource);
            }
        }

        private void WriteNVMData(byte[] _nvmData)
        {
            byte packetLength = NVMBackupRestoreModel.PacketLength;
            byte actualPacketLength = 0;
            int offset = 0;
            bool done = false;
            while (!done)
            {
                byte pLength = offset + packetLength >= _nvmData.Length ? (byte)(_nvmData.Length - offset) : packetLength;
                done = offset + packetLength >= _nvmData.Length;
                byte[] nvmDataPacket = _nvmData.Skip(offset).Take(pLength).ToArray();

                NvmBackupRestoreWriteResult writeResult = SessionDevice.NvmBackupRestoreWrite(pLength, offset, nvmDataPacket);
                if (writeResult)
                {
                    if (actualPacketLength != writeResult.Length)
                    {
                        actualPacketLength = writeResult.Length;
                        NVMBackupRestoreModel.ActualPacketLength = actualPacketLength;
                    }
                    offset += writeResult.Length;
                    if (writeResult.Status == NvmBackupRestoreStatuses.Error)
                    {
                        ControllerSession.Logger.LogFail("NVM Restore Write operation failed");
                        break;
                    }
                    else if (writeResult.Status == NvmBackupRestoreStatuses.EOF)
                    {
                        done = true;
                        ControllerSession.Logger.LogOk("NVM Restrore finished");
                    }
                }
                else
                {
                    ControllerSession.Logger.LogFail("NVM Resore operation failed");
                    done = true;
                }
            }
        }

        private List<byte[]> ArraySplit(byte[] data, int fragmentSize)
        {
            if (fragmentSize != 0)
            {
                List<byte[]> result = new List<byte[]>();
                if (data.Length <= fragmentSize)
                {
                    result.Add(data);
                }
                else
                {
                    for (int i = 0; i < data.Length; i += fragmentSize)
                    {
                        byte[] arr = new byte[fragmentSize];
                        if (i < (data.Length - fragmentSize))
                        {
                            Buffer.BlockCopy(data, i, arr, 0, fragmentSize);
                        }
                        else
                        {
                            arr = new byte[data.Length - i];
                            Buffer.BlockCopy(data, i, arr, 0, arr.Length);
                        }
                        result.Add(arr);
                    }
                }
                return result;
            }
            return null;
        }
    }
}