/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using ZWave.CommandClasses;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWave.Devices;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class ConfigurationSetCommand : ConfigurationCommandBase
    {
        private COMMAND_CLASS_CONFIGURATION_V3.CONFIGURATION_SET _cmd;
        public ConfigurationSetCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Configuration Set";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                 (ControllerSession.ApplicationModel.IsActiveSessionZip ?
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != ((ZipControllerSession)ControllerSession).Controller.Id :
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                    ControllerSession.Controller.Network.HasCommandClass(ApplicationModel.SelectedNode.Item, COMMAND_CLASS_CONFIGURATION_V4.ID);
        }

        public override void PrepareData()
        {
            base.PrepareData();
            try
            {
                List<byte> resultingValue = new List<byte>();
                foreach (var configParam in ConfigurationModel.ConfigurationParameters)
                {
                    byte[] valueToSend = new byte[configParam.Size];
                    switch (configParam.Format)
                    {
                        case ConfigurationFormatTypes.SignedInteger:
                            var item = configParam.Item as ISignedIntegerConfigurationParam;
                            if (item != null)
                            {
                                valueToSend = Tools.GetBytes(item.Value).Skip(4 - configParam.Size).Take(configParam.Size).ToArray();
                            }
                            break;
                        case ConfigurationFormatTypes.UnsignedInteger:
                            var uItem = configParam.Item as IUnsignedIntegerConfigurationParam;
                            if (uItem != null)
                            {
                                valueToSend = Tools.UInt32ToByteArray(uItem.Value).Skip(4 - configParam.Size).Take(configParam.Size).ToArray();
                            }
                            break;
                        case ConfigurationFormatTypes.Enumerated:
                            var eItem = configParam.Item as IEnumeratedConfigurationParam;
                            if (eItem != null)
                            {
                                int value = eItem.RadioButtons.Where(x => x.IsSelected).Select(y => y.Item.ItemValue).FirstOrDefault();
                                valueToSend = Tools.GetBytes(value).Skip(4 - configParam.Size).Take(configParam.Size).ToArray();
                            }
                            break;
                        case ConfigurationFormatTypes.Bitfield:
                            var bItem = configParam.Item as IBitFieldConfigurationParam;
                            if (bItem != null)
                            {
                                BitArray bitArray = new BitArray(8 * configParam.Size);
                                foreach (var checkBox in bItem.CheckBoxes)
                                {
                                    if (checkBox.IsSelected)
                                    {
                                        bitArray.Set(checkBox.Item - 1, true);
                                    }
                                }
                                bitArray.CopyTo(valueToSend, 0);
                            }
                            break;
                        default:
                            break;
                    }
                    resultingValue.AddRange(valueToSend);

                    _cmd = new COMMAND_CLASS_CONFIGURATION_V3.CONFIGURATION_SET();
                    _cmd.parameterNumber = configParam.ParameterNumber.Last(); //TODO!!!!
                    _cmd.configurationValue = valueToSend;
                    _cmd.properties1.size = (byte)configParam.Size;
                    _cmd.properties1.mdefault = 0;
                }
            }
            catch
            {
                ControllerSession.Logger.LogFail($"Configuration Set - Node {Device} - Failed");
            }
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Configuration Set in progress");
            Log("Configuration Set in progress");

            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
            var sendRes = ControllerSession.SendData(Device, _cmd, _token);

            if (!sendRes)
            {
                ControllerSession.Logger.LogFail($"Configuration Set - Node {Device} - Failed");
            }
            else
            {
                ControllerSession.Logger.LogOk($"Configuration Set - Node {Device} - Succeeded");
                ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
            }
        }
    }
}
