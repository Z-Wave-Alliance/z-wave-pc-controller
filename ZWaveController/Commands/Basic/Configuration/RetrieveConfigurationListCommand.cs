/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using System.Text;
using Utils;
using ZWave.CommandClasses;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class RetrieveConfigurationListCommand : ConfigurationCommandBase
    {
        public RetrieveConfigurationListCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Retrieve Configuration List";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                 (ControllerSession.ApplicationModel.IsActiveSessionZip ?
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != ((ZipControllerSession)ControllerSession).Controller.Id :
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id) &&
                    ControllerSession.Controller.Network.HasCommandClass(ApplicationModel.SelectedNode.Item, COMMAND_CLASS_CONFIGURATION_V4.ID);
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Retrieve Configuration List in progress");
            Log("Retrieve Configuration List in progress");
            ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;

            ControllerSession.ApplicationModel.Invoke(() =>
            {
                ConfigurationModel.ClearParameters();
            });

            bool hasNextParameter = true;
            byte[] paramNumber = new byte[] { 0x00, 0x01 };
            while (hasNextParameter)
            {
                hasNextParameter = false;
                byte[] rptData = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_PROPERTIES_REPORT();
                var confPropGetCmd = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_PROPERTIES_GET();
                confPropGetCmd.parameterNumber = paramNumber;

                var configurationPropReport = ControllerSession.RequestData(TargetDevice, confPropGetCmd,
                    ref rptData, 10000, _token);
                if (configurationPropReport)
                {
                    COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_PROPERTIES_REPORT confPropRepCmd = rptData;
                    if (!confPropRepCmd.nextParameterNumber.SequenceEqual(new byte[] { 0x00, 0x00 }))
                    {
                        hasNextParameter = true;
                        paramNumber = confPropRepCmd.nextParameterNumber;
                    }

                    string confParamName = ConfigurationNameGet(TargetDevice, confPropRepCmd.parameterNumber);
                    if (string.IsNullOrEmpty(confParamName))
                        break;
                    string confParamInfo = ConfigurationInfoGet(TargetDevice, confPropRepCmd.parameterNumber);
                    if (string.IsNullOrEmpty(confParamInfo))
                        break;
                    byte[] confParamValue = ConfigurationGet(TargetDevice, confPropRepCmd.parameterNumber);
                    if (confParamValue == null)
                        break;
                    var valueSize = confPropRepCmd.properties1.size;
                    IConfigurationParamModel newConfPVM = ControllerSession.ApplicationModel.ConfigurationModel.GetConfigurationParam();
                    newConfPVM.Name = confParamName;
                    newConfPVM.Info = confParamInfo;
                    newConfPVM.ParameterNumber = confPropRepCmd.parameterNumber;
                    newConfPVM.DefaultValue = Tools.GetInt32(confPropRepCmd.defaultValue.ToArray());
                    newConfPVM.NextParameterNumber = confPropRepCmd.nextParameterNumber;
                    newConfPVM.Size = valueSize;
                    IDefaultConfigurationParamModel paramVM;

                    int minValue = Tools.GetInt32(confPropRepCmd.minValue.ToArray());
                    int maxValue = Tools.GetInt32(confPropRepCmd.maxValue.ToArray());
                    switch (confPropRepCmd.properties1.format)
                    {
                        case 0:
                            newConfPVM.Format = ConfigurationFormatTypes.SignedInteger;
                            minValue = Tools.GetSignedInt32(confPropRepCmd.minValue.ToArray());
                            maxValue = Tools.GetSignedInt32(confPropRepCmd.maxValue.ToArray());
                            newConfPVM.Item = ControllerSession.ApplicationModel.ConfigurationModel.GetSignedIntConfigParam(minValue, maxValue, Tools.GetInt32(confParamValue));
                            break;
                        case 1:
                            newConfPVM.Format = ConfigurationFormatTypes.UnsignedInteger;
                            newConfPVM.Item = ControllerSession.ApplicationModel.ConfigurationModel.GetUintConfigParam(minValue, maxValue, Tools.ByteArrayToUInt32(confParamValue));
                            break;
                        case 2:
                            newConfPVM.Format = ConfigurationFormatTypes.Enumerated;
                            paramVM = ControllerSession.ApplicationModel.ConfigurationModel.GetEnumConfigParam();
                            for (int i = minValue; i <= maxValue; i++)
                            {
                                var wr = new RadioButtonsWrapper
                                {
                                    GroupName = Encoding.UTF8.GetString(newConfPVM.ParameterNumber),
                                    ItemValue = i
                                };
                                var item = ControllerSession.ApplicationModel.CreateSelectableItem(wr);
                                if (i == Tools.ByteArrayToUInt32(confParamValue))
                                    item.IsSelected = true;
                                ((IEnumeratedConfigurationParam)paramVM).RadioButtons.Add(item);
                            }
                            newConfPVM.Item = paramVM;
                            break;
                        case 3:
                            newConfPVM.Format = ConfigurationFormatTypes.Bitfield;
                            paramVM = ControllerSession.ApplicationModel.ConfigurationModel.GetBitFieldConfigParam();
                            for (int i = minValue; i <= maxValue; i++)
                            {
                                var item = ControllerSession.ApplicationModel.CreateSelectableItem(i);
                                if (i == Tools.ByteArrayToUInt32(confParamValue))
                                    item.IsSelected = true;
                                ((IBitFieldConfigurationParam)paramVM).CheckBoxes.Add(item);
                            }
                            newConfPVM.Item = paramVM;
                            break;
                        default:
                            break;
                    }
                    ControllerSession.ApplicationModel.Invoke(() =>
                    {
                        ConfigurationModel.ConfigurationParameters.Add(newConfPVM);
                    });

                    ControllerSession.Logger.LogOk($"Param fetched - Node {TargetDevice.Id} - Succeeded: {Tools.GetHex(confPropRepCmd.parameterNumber)}");
                    ControllerSession.ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
                }
                else
                {
                    ControllerSession.Logger.LogFail($"Get Configuration - Node {TargetDevice.Id} - Failed");
                }
            }
        }
    }
}
