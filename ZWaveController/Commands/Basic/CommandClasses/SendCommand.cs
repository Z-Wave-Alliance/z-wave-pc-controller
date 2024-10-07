/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;


namespace ZWaveController.Commands
{
    public class SendCommand : CommandBasicBase
    {
        private ICommandClassesModel _ccModel { get; set; }
        private List<OperationData> _operationList { get; set; }

        public SendCommand(IControllerSession controllerSession)
           : base(controllerSession)
        {
            IsTargetSensitive = true;
            UseBackgroundThread = true;
            Text = "Send Data Command";
            _ccModel = controllerSession.ApplicationModel.CommandClassesModel;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return _ccModel != null && _ccModel.Payload != null && ControllerSession.ApplicationModel.SelectedNode != null;
        }

        public override CommandTypes CommandType {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override void ExecuteInner(object param)
        {
            _operationList.ForEach(i => {
                switch (i.Type)
                {
                    case OperationType.SerialApi:
                        ControllerSession.SendSerialApi(i.Data, out _token);
                        break;
                    case OperationType.SendData:
                        ControllerSession.SendData(i.NodeTag, i.Data, SessionDevice == null ? 0 : SessionDevice.Network.S0MaxBytesPerFrameSize, i.SubstituteFlag, _token, true);
                        break;
                    case OperationType.SendMulicastData:
                        ControllerSession.SendData(i.NodeTagArray, i.Data, SessionDevice == null ? 0 : SessionDevice.Network.S0MaxBytesPerFrameSize, i.SubstituteFlag, _token, true);
                        break;
                    case OperationType.RequestData:
                        byte[] expectCommand = { _ccModel.ExpectedCommand.Key, _ccModel.ExpectedCommand.Value };
                        ControllerSession.RequestData(i.NodeTag, i.Data, ref expectCommand, ControllerSession.ApplicationModel.SendDataSettingsModel.RequestsTimeoutMs, _token);
                        break;
                    default:
                        break;
                }
            });

            ControllerSession.ApplicationModel.Invoke(() => {
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SendCommand), Message = "Data Sent" });
            });
        }

        public override void PrepareData()
        {
            _operationList = new List<OperationData>();

            var data = _ccModel.Payload;
            ControllerSession.SenderHistoryService.Add(ControllerSession.GetPayloadItem(_ccModel));

            if (_ccModel.SecureType == SecureType.SerialApi)
                _operationList.Add(new OperationData { Data = data, Type = OperationType.SerialApi });
            else
            {
                if (_ccModel.IsSupervisionGetEnabled && data.Length > 1)
                {
                    COMMAND_CLASS_SUPERVISION.SUPERVISION_GET encapData = new COMMAND_CLASS_SUPERVISION.SUPERVISION_GET();
                    if (_ccModel.IsAutoIncSupervisionSessionId)
                    {
                        //Send to WebClien&???
                        _ccModel.SupervisionSessionId++;
                        if (_ccModel.SupervisionSessionId > 255)
                        {
                            _ccModel.SupervisionSessionId = 0;
                        }
                    }
                    encapData.properties1.statusUpdates = _ccModel.IsSupervisionGetStatusUpdatesEnabled ? (byte)1 : (byte)0;
                    encapData.properties1.sessionId = _ccModel.SupervisionSessionId;
                    encapData.encapsulatedCommandLength = (byte)data.Length;
                    encapData.encapsulatedCommand = new List<byte>(data);
                    data = encapData;
                }
                if ((_ccModel.IsMultiChannelEnabled && data.Length > 1) || TargetDevice.EndPointId > 0)
                {
                    TargetDevice = TargetDevice.EndPointId > 0 ? TargetDevice : new NodeTag(TargetDevice.Id);
                    COMMAND_CLASS_MULTI_CHANNEL_V3.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V3.MULTI_CHANNEL_CMD_ENCAP();

                    multiChannelCmd.commandClass = data[0];
                    multiChannelCmd.command = data[1];
                    multiChannelCmd.parameter = new List<byte>();
                    for (int i = 2; i < data.Length; i++)
                    {
                        multiChannelCmd.parameter.Add(data[i]);
                    }
                    multiChannelCmd.properties1.res = 0;
                    multiChannelCmd.properties1.sourceEndPoint = TargetDevice.EndPointId > 0 ? (byte)0 : _ccModel.SourceEndPoint;
                    multiChannelCmd.properties2.bitAddress = System.Convert.ToByte(_ccModel.IsBitAddress);
                    multiChannelCmd.properties2.destinationEndPoint = TargetDevice.EndPointId > 0 ? TargetDevice.EndPointId : _ccModel.DestinationEndPoint;
                    data = multiChannelCmd;
                }
                if (_ccModel.IsCrc16Enabled && data.Length > 1)
                {
                    COMMAND_CLASS_CRC_16_ENCAP.CRC_16_ENCAP encapData = new COMMAND_CLASS_CRC_16_ENCAP.CRC_16_ENCAP();
                    encapData.commandClass = data[0];
                    encapData.command = data[1];
                    encapData.data = new List<byte>();

                    for (int i = 2; i < data.Length; i++)
                    {
                        encapData.data.Add(data[i]);
                    }

                    encapData.checksum = new byte[] { 0, 0 };
                    byte[] tmp = encapData;
                    ushort crc = Tools.ZW_CreateCrc16(null, 0, tmp, (byte)(tmp.Length - 2));
                    encapData.checksum = new[] { (byte)(crc >> 8), (byte)crc };
                    data = encapData;
                }
                SubstituteFlags substituteFlag;
                if (_ccModel.SecureType == SecureType.DefaultSecurity)
                {
                    substituteFlag = SubstituteFlags.None;
                }
                else
                {
                    substituteFlag = _ccModel.SecureType == SecureType.Secure ?
                        SubstituteFlags.UseSecurity : SubstituteFlags.DenySecurity;
                }
                //Implement MultySelect on WebClient????
                if (_ccModel.IsForceMulticastEnabled || _ccModel.SelectedNodeItems?.Count > 1)
                {
                    if (_ccModel.IsSuppressMulticastFollowUp)
                    {
                        substituteFlag |= SubstituteFlags.DenyFollowup;
                    }
                    _operationList.Add(new OperationData {
                        Data = data,
                        Type = OperationType.SendMulicastData,
                        NodeTagArray = _ccModel.SelectedNodeItems.ToArray(),
                        SubstituteFlag = substituteFlag
                    });
                }
                else
                {
                    if (_ccModel.SecureType == SecureType.Broadcast)
                    {
                        _operationList.Add(new OperationData {
                            Data = data,
                            Type = OperationType.SendData,
                            NodeTag = new NodeTag(0xFF),
                            SubstituteFlag = substituteFlag
                        });
                    }
                    else
                    {
                        if (_ccModel.IsExpectCommand && _ccModel.ExpectedCommand.Key != 0)
                        {

                            _operationList.Add(new OperationData {
                                Data = data,
                                Type = OperationType.RequestData,
                                NodeTag = (NodeTag)TargetDevice.Clone()
                            });
                        }
                        else
                        {
                            _operationList.Add(new OperationData {
                                Data = data,
                                Type = OperationType.SendData,
                                NodeTag = (NodeTag)TargetDevice.Clone(),
                                SubstituteFlag = substituteFlag
                            });
                        }
                    }
                }
            }
        }


        protected override bool CanCancelAction(object param)
        {
            return true;
        }
    }

    public class OperationData
    {
        public NodeTag NodeTag { get; set; }
        public NodeTag[] NodeTagArray { get; set; }
        public byte[] Data { get; set; }
        public OperationType Type { get; set; }
        public SubstituteFlags SubstituteFlag { get; set; }
    }

    public enum OperationType
    {
        SerialApi,
        SendData,
        SendMulicastData,
        RequestData
    }
}