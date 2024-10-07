/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using Utils;
using ZWave;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Layers;
using ZWave.Xml.Application;
using ZWave.ZipApplication;
using ZWave.ZipApplication.Devices;
using ZWave.ZipApplication.Operations;
using ZWave.ZipApplication.CommandClasses;
using System.Collections;
using System.Collections.ObjectModel;
using ZWave.Security.S2;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWave.BasicApplication;
using ZWave.BasicApplication.TransportService;
using ZWave.Layers.Session;
using Utils.UI.Interfaces;
using ZWaveController.Services;
using ZWaveController.Commands;
using Utils.UI.Logging;
using ZWaveController.Configuration;
#if !NETCOREAPP
//using ZWaveController.Properties;
#endif

namespace ZWaveController.Models
{
    public class ZipControllerSession : IControllerSession, IUnsolicitedClientsFactory
    {
        private string _psk;
        private UnsolicitedDestinationHandler<IUnsolicitedClient> _primaryUnsolicitedClients;
        private UnsolicitedDestinationHandler<IUnsolicitedReceiveClient> _secondaryUnsolicitedClients;
        ISessionLayer _sessionLayer;
        ZipApplicationLayer _zipApplicationLayer;
        Dtls1ClientTransportLayer _dtls1ClientTransportLayer;
        private ActionToken _networkManagementInclusionSupportToken;

        public IApplicationModel ApplicationModel { get; set; }

        private SocketDataSource _socketDataSource;
        public IDataSource DataSource
        {
            get => _socketDataSource;
            private set
            {
                if (!(value is SocketDataSource) || ((SocketDataSource)value).Type != SoketSourceTypes.ZIP)
                {
                    throw new NotSupportedException("Invalid data source type");
                }
                _socketDataSource = (SocketDataSource)value;
                var cmdLineParser = new CommandLineParser(new[] { _socketDataSource.Args });
                _psk = cmdLineParser.GetArgumentString("psk");
            }
        }

        public string UserSessionId { get; set; }
        private ZipController _zipController;
        public IDevice Controller => _zipController as IDevice;
        public ILogService Logger { get; set; }
        public ISenderHistoryService SenderHistoryService { get; set; }
        public IPredefinedPayloadsService PredefinedPayloadsService { get; set; }
        public SecurityManager SecurityManager { get; private set; }
        public SupervisionManager SupervisionManager { get; private set; }
        public TransportServiceManager TransportServiceManager { get; private set; }
        public ISerialPortMonitor SerialPortMonitor { get; private set; }
        public SortedList<NodeTag, ZipDevice> Devices { get; set; }
        public IMAFullNetwork IMAFullNetwork { get; set; }
        public bool IsBridgeControllerLibrary => false;
        public bool IsEndDeviceLibrary => false;
        public IPollingService PollingService { get; private set; }
        public IERTTService ERTTService { get => throw new NotSupportedException(); private set => throw new NotSupportedException(); }
        public INodeInformationService NodeInformationService { get; private set; }
        public ZipControllerSession(IApplicationModel applicationModel)
        {
            ApplicationModel = applicationModel;
            Devices = new SortedList<NodeTag, ZipDevice>();
#if !NETCOREAPP
            SerialPortMonitor = new SerialPortMonitor(this);
#endif
            PollingService = new PollingService(this);
            IMAFullNetwork = new IMAFullNetwork();

            ControllerSessionsContainer.Config.ControllerConfiguration.IsUnsolicitedDestinationEnabled =
                ControllerSessionsContainer.Config.ControllerConfiguration.IsUnsolicitedDestinationEnabled ?? true;

            _sessionLayer = new SessionLayer();
            _dtls1ClientTransportLayer = new Dtls1ClientTransportLayer();
            _zipApplicationLayer = new ZipApplicationLayer(_sessionLayer, new ZipFrameLayer(), _dtls1ClientTransportLayer);
            _zipApplicationLayer.TransportLayer.DataTransmitted += ApplicationModel.TraceCapture.SaveTransmittedData;
        }

        #region IControllerModel Members
        byte[] _headerExtension = null;//new byte[] { 0x84, 0x02, 0x01, 0x00 };

        public ActionResult SendData(NodeTag node, byte[] txData, ActionToken token)
        {
            if (txData != null && txData.Length > 0)
            {
                var command = ApplicationModel.ZWaveDefinition.ParseApplicationStringName(txData);
                var caption = "Send " + command + " to node " + node;
                if (node.EndPointId > 0)
                {
                    var multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V3.MULTI_CHANNEL_CMD_ENCAP();
                    multiChannelCmd.commandClass = txData[0];
                    multiChannelCmd.command = txData[1];
                    multiChannelCmd.parameter = new List<byte>();
                    for (int i = 2; i < txData.Length; i++)
                    {
                        multiChannelCmd.parameter.Add(txData[i]);
                    }
                    multiChannelCmd.properties1.res = 0;
                    multiChannelCmd.properties1.sourceEndPoint = 0;
                    multiChannelCmd.properties2.bitAddress = 0;
                    multiChannelCmd.properties2.destinationEndPoint = node.EndPointId;
                    txData = multiChannelCmd;
                }
                using (var logAction = ReportAction(caption, null))
                {
                    SendDataResult ret;
                    if (node.Id == _zipController.Id)
                    {
                        token = _zipController.SendData(_headerExtension, txData, null);
                        ret = (SendDataResult)token.WaitCompletedSignal();
                    }
                    else
                    {
                        if (Devices.ContainsKey(node))
                        {
                            token = Devices[node].SendData(_headerExtension, txData, null);
                            ret = (SendDataResult)token.WaitCompletedSignal();
                        }
                        else
                        {
                            token = null;
                            return null;
                        }
                    }
                    logAction.State = ret?.State ?? ActionStates.Failed;
                    return ret;
                }
            }
            else
            {
                token = null;
                return null;
            }
        }

        public ActionResult SendData(NodeTag node, byte[] txData, TransmitOptions txOptions, ActionToken token)
        {
            return SendData(node, txData, token);
        }

        public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs, ActionToken token)
        {
            return RequestData(node, _headerExtension, txData, ref rxData, timeoutMs, token);
        }

        public RequestDataResult RequestData(NodeTag node, byte[] txExtension, byte[] txData, ref byte[] rxData, int timeoutMs, ActionToken token)
        {
            RequestDataResult ret = null;
            if (node == _zipController.Network.NodeTag)
            {
                token = _zipController.RequestData(txExtension, txData, rxData, timeoutMs, null);
                ret = (RequestDataResult)token.WaitCompletedSignal();
            }
            else
            {
                token = Devices[node].RequestData(txExtension, txData, rxData, timeoutMs, null);
                ret = (RequestDataResult)token.WaitCompletedSignal();
            }
            rxData = ret.ReceivedData;
            return ret;
        }

        public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs)
        {
            RequestDataResult ret = null;
            if (node == _zipController.Network.NodeTag)
            {
                ret = _zipController.RequestData(_headerExtension, txData, rxData, timeoutMs);
            }
            else
            {
                ret = Devices[node].RequestData(_headerExtension, txData, rxData, timeoutMs);
            }
            rxData = ret.ReceivedData;
            return ret;
        }

        //public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs, ActionToken token)
        //{
        //    return RequestData(node, txData, ref rxData, timeoutMs, null, token);
        //}

        public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs, Action sendDataSubstituteCallback, ActionToken token)
        {
            if (node.EndPointId > 0)
            {
                var multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V3.MULTI_CHANNEL_CMD_ENCAP();
                multiChannelCmd.commandClass = txData[0];
                multiChannelCmd.command = txData[1];
                multiChannelCmd.parameter = new List<byte>();
                for (int i = 2; i < txData.Length; i++)
                {
                    multiChannelCmd.parameter.Add(txData[i]);
                }
                multiChannelCmd.properties1.res = 0;
                multiChannelCmd.properties1.sourceEndPoint = 0;
                multiChannelCmd.properties2.bitAddress = 0;
                multiChannelCmd.properties2.destinationEndPoint = node.EndPointId;
                txData = multiChannelCmd;
            }

            return RequestData(node, txData, ref rxData, timeoutMs, token);
        }

        public void SetNodeInformation(out ActionToken token)
        {
            throw new NotImplementedException();
        }

        public void SetNodeInformation(DeviceOptions deviceOptions, byte generic, byte specific, byte[] commandClasses, out ActionToken token)
        {
            throw new NotImplementedException();
        }

        public ActionResult IsFailedNode(NodeTag node, out ActionToken requestToken)
        {
            throw new NotImplementedException();
        }

        public ActionResult SetSucNode(NodeTag node, out ActionToken mToken)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult AreNeighbors(NodeTag srcNode, NodeTag destNode)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult GetRoutingInfo(NodeTag node, out NodeTag[] routingNodes)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult DeleteReturnRoute(NodeTag srcNode)
        {
            return CommandExecutionResult.OK; // Do Nothing
        }

        public CommandExecutionResult DeleteReturnRoute(NodeTag srcNode, bool isSUCReturnRoute, out ActionToken token)
        {
            token = null;
            return CommandExecutionResult.OK; // Do Nothing
        }

        public CommandExecutionResult SetVirtualDeviceLearnMode(VirtualDeviceLearnModes mode, NodeTag node, out ActionToken token)
        {
            token = null;
            throw new NotImplementedException();
        }



        public CommandExecutionResult FirmwareUpdateOTAActivate(NodeTag node, ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("OTA Firmware Activate started.");
            Logger.Log("OTA Firmware Activate started.");
            byte[] activateCmd = null;
            if (ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 4)
            {
                var fwuActivationSetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_ACTIVATION_SET();
                fwuActivationSetData.manufacturerId = ApplicationModel.FirmwareUpdateModel.ManufacturerID;
                fwuActivationSetData.firmwareId = ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId;
                fwuActivationSetData.checksum = ApplicationModel.FirmwareUpdateModel.FirmwareChecksum;
                fwuActivationSetData.firmwareTarget = (byte)ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index;
                activateCmd = fwuActivationSetData;
            }
            else if (ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion == 5)
            {
                var fwuActivationSetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_ACTIVATION_SET();
                fwuActivationSetData.manufacturerId = ApplicationModel.FirmwareUpdateModel.ManufacturerID;
                fwuActivationSetData.firmwareId = ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId;
                fwuActivationSetData.checksum = ApplicationModel.FirmwareUpdateModel.FirmwareChecksum;
                fwuActivationSetData.firmwareTarget = (byte)ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index;
                fwuActivationSetData.hardwareVersion = (byte)ApplicationModel.FirmwareUpdateModel.HardwareVersion;
                activateCmd = fwuActivationSetData;
            }

            byte[] fwuActivationReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_ACTIVATION_STATUS_REPORT();

            //Start Activate
            var res = RequestData(node, activateCmd, ref fwuActivationReportData, 3000, token);
            if (res)
            {
                string msg = "";
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_ACTIVATION_STATUS_REPORT cmd = fwuActivationReportData;
                if (cmd.firmwareUpdateStatus == 0xFF)
                {
                    msg = $"Received status (0x{cmd.firmwareUpdateStatus.ToString("X2")}): Firmware update completed successfully";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
                else if (cmd.firmwareUpdateStatus == 0x00)
                {
                    msg = $"Received status (0x{cmd.firmwareUpdateStatus.ToString("X2")}): Invalid combination";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
                else if (cmd.firmwareUpdateStatus == 0x01)
                {
                    msg = $"Received status (0x{cmd.firmwareUpdateStatus.ToString("X2")}): Error activating the firmware";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
            }
            return ret;
        }

        public CommandExecutionResult FirmwareUpdateOTADownload(NodeTag node, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            token = null;
            if (ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget != null)
            {
                ApplicationModel.SetBusyMessage("OTA Firmware Download started.");
                Logger.Log("OTA Firmware Download started.");

                //var extension = new byte[] { 0x84, 0x02, 0x04, 0x00 };

                var getCmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_GET();
                getCmd.manufacturerId = ApplicationModel.FirmwareUpdateModel.ManufacturerID;
                getCmd.firmwareId = ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId;
                getCmd.firmwareTarget = (byte)ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index;
                getCmd.fragmentSize = Tools.GetBytes((ushort)ApplicationModel.FirmwareUpdateModel.FragmentSize);
                getCmd.hardwareVersion = (byte)ApplicationModel.FirmwareUpdateModel.HardwareVersion;

                byte[] expectCmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_REPORT();
                var res = RequestData(node, getCmd, ref expectCmd, 3000, token);
                if (res)
                {
                    COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_REPORT reportCmd = expectCmd;
                    string msg = "";
                    if (reportCmd.status == 0xFF)
                    {
                        msg = String.Format("Received status (0x{0}): Valid combination", reportCmd.status.ToString("X2"));
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);


                        ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = null;
                        isDownloadCompleted = false;
                        isDownloadCancelled = false;
                        int iteration = 0;
                        while (!isDownloadCompleted && !isDownloadCancelled)
                        {
                            var get2Cmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET();
                            get2Cmd.numberOfReports = ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports;
                            int reportNumber = iteration * ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports + 1;
                            get2Cmd.properties1.reportNumber1 = (byte)(reportNumber >> 8);
                            get2Cmd.reportNumber2 = (byte)reportNumber;
                            expectPackets = new BitArray(ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports);
                            token = ResponseMultiData(node, OnDownloadReceived, new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT());
                            var res2 = SendData(node, get2Cmd, token);
                            if (res2)
                            {
                                token.WaitCompletedSignal();
                                if (isDownloadCompleted && downloadFirmware != null)
                                {
                                    List<byte> tmp = new List<byte>();
                                    foreach (var item in downloadFirmware)
                                    {
                                        tmp.AddRange(item.Value);
                                    }
                                    ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = tmp.ToArray();
                                }
                            }
                            iteration++;
                        }
                    }
                    else if (reportCmd.status == 0x00)
                    {
                        msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Invalid combination";
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);
                    }
                    else if (reportCmd.status == 0x01)
                    {
                        msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Requires authentication";
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);
                    }
                    else if (reportCmd.status == 0x02)
                    {
                        msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Invalid Fragment Size";
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);
                    }
                    else if (reportCmd.status == 0x03)
                    {
                        msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Not upgradable";
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);
                    }
                    else if (reportCmd.status == 0x04)
                    {
                        msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Invalid Hardware Version";
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                        Logger.Log(msg);
                    }

                }
            }
            return ret;
        }

        SortedList<int, byte[]> downloadFirmware = new SortedList<int, byte[]>();
        BitArray expectPackets = null;
        bool isDownloadCompleted = false;
        bool isDownloadCancelled = false;

        private List<byte[]> OnDownloadReceived(ActionToken token, byte[] header, byte[] payload)
        {
            List<byte[]> ret = null;
            if (payload != null)
            {
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT cmd = payload;
                int reportNumber = (cmd.properties1.reportNumber1 << 8) + cmd.reportNumber2;
                expectPackets[(reportNumber - 1) % ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports] = true;
                bool isAllReceived = true;
                for (int i = 0; i < expectPackets.Count; i++)
                {
                    if (!expectPackets[i])
                    {
                        isAllReceived = false;
                        break;
                    }
                }
                if (cmd.data != null)
                {
                    if (downloadFirmware.ContainsKey(reportNumber))
                    {
                        downloadFirmware[reportNumber] = cmd.data.ToArray();
                    }
                    else
                    {
                        downloadFirmware.Add(reportNumber, cmd.data.ToArray());
                    }
                }

                if (cmd.properties1.last > 0)
                {
                    isDownloadCompleted = true;
                    _zipController.Cancel(token);
                }
                else if (isAllReceived)
                {
                    _zipController.Cancel(token);
                }
            }
            return ret;
        }

        public byte FirmwareUpdateV1(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            // Register ResponseData.
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = ResponseMultiData(node, (t, h, x) =>
            {
                ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
                fwuMDGetData = x;
                List<byte[]> fwuMDReportData = new List<byte[]>();
                short firmwareDataIndex = 0;
                for (byte i = 0; i < fwuMDGetData.numberOfReports; i++)
                {
                    firmwareDataIndex = (short)(((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2) + i);
                    if (fwData.Count > (firmwareDataIndex - 1))
                    {
                        List<byte> dataToSend = new List<byte>(fwData[firmwareDataIndex - 1]);
                        dataToSend.Insert(0, COMMAND_CLASS_FIRMWARE_UPDATE_MD.ID);
                        dataToSend.Insert(1, COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT.ID);
                        if (fwData.Count <= firmwareDataIndex)
                        {
                            dataToSend.Insert(2, (byte)((firmwareDataIndex >> 8) | 0x80));
                        }
                        else
                        {
                            dataToSend.Insert(2, (byte)(firmwareDataIndex >> 8));
                        }
                        dataToSend.Insert(3, (byte)firmwareDataIndex);
                        fwuMDReportData.Add(dataToSend.ToArray());
                    }
                }
                ApplicationModel.SetBusyMessage("Please wait until the OTA Firmware Update completed.\n" +
                    String.Format("packet# {0} of {1} written.", firmwareDataIndex, fwData.Count));
                return fwuMDReportData;
            }, fwuMDGetData);

            // Start Update.
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        public byte FirmwareUpdateV2(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            // Register ResponseData.
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = ResponseMultiData(node, (t, h, x) =>
            {
                ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
                fwuMDGetData = x;
                List<byte[]> fwuMDReportData = new List<byte[]>();
                short firmwareDataIndex = 0;
                for (byte i = 0; i < fwuMDGetData.numberOfReports; i++)
                {
                    firmwareDataIndex = (short)(((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2) + i);
                    if (fwData.Count > (firmwareDataIndex - 1))
                    {
                        List<byte> dataToSend = new List<byte>(fwData[firmwareDataIndex - 1]);
                        dataToSend.Insert(0, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.ID);
                        dataToSend.Insert(1, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT.ID);
                        if (fwData.Count <= firmwareDataIndex)
                        {
                            dataToSend.Insert(2, (byte)((firmwareDataIndex >> 8) | 0x80));
                        }
                        else
                        {
                            dataToSend.Insert(2, (byte)(firmwareDataIndex >> 8));
                        }
                        dataToSend.Insert(3, (byte)firmwareDataIndex);

                        short crc = Utils.Tools.CalculateCrc16(dataToSend);

                        dataToSend.Add((byte)(crc >> 8));
                        dataToSend.Add((byte)crc);

                        fwuMDReportData.Add(dataToSend.ToArray());
                    }
                }
                ApplicationModel.SetBusyMessage("Please wait until the OTA Firmware Update completed.\n" +
                    String.Format("packet# {0} of {1} written.", firmwareDataIndex, fwData.Count));
                return fwuMDReportData;
            }, fwuMDGetData);

            // Start Update.
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        public byte FirmwareUpdateV3(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            fwuMDRequestGetData.firmwareTarget = fwTarget;
            fwuMDRequestGetData.fragmentSize = Tools.GetBytes((ushort)fragmentSize);

            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            // Register ResponseData.
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = ResponseMultiData(node, (t, h, x) =>
            {
                ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
                fwuMDGetData = x;
                List<byte[]> fwuMDReportData = new List<byte[]>();
                short firmwareDataIndex = 0;
                for (byte i = 0; i < fwuMDGetData.numberOfReports; i++)
                {
                    firmwareDataIndex = (short)(((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2) + i);
                    if (fwData.Count > (firmwareDataIndex - 1))
                    {
                        List<byte> dataToSend = new List<byte>(fwData[firmwareDataIndex - 1]);
                        dataToSend.Insert(0, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.ID);
                        dataToSend.Insert(1, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT.ID);
                        if (fwData.Count <= firmwareDataIndex)
                        {
                            dataToSend.Insert(2, (byte)((firmwareDataIndex >> 8) | 0x80));
                        }
                        else
                        {
                            dataToSend.Insert(2, (byte)(firmwareDataIndex >> 8));
                        }
                        dataToSend.Insert(3, (byte)firmwareDataIndex);

                        short crc = Utils.Tools.CalculateCrc16(dataToSend);

                        dataToSend.Add((byte)(crc >> 8));
                        dataToSend.Add((byte)crc);

                        fwuMDReportData.Add(dataToSend.ToArray());
                    }
                }
                ApplicationModel.SetBusyMessage("Please wait until the OTA Firmware Update completed.\n" +
                    String.Format("packet# {0} of {1} written.", firmwareDataIndex, fwData.Count));
                return fwuMDReportData;
            }, fwuMDGetData);

            // Start Update.
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        public byte FirmwareUpdateV4(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, byte activation, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            fwuMDRequestGetData.firmwareTarget = fwTarget;
            fwuMDRequestGetData.fragmentSize = Tools.GetBytes((ushort)fragmentSize);
            fwuMDRequestGetData.properties1.activation = activation;

            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            // Register ResponseData.
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = ResponseMultiData(node, (t, h, x) =>
            {
                ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
                fwuMDGetData = x;
                List<byte[]> fwuMDReportData = new List<byte[]>();
                short firmwareDataIndex = 0;
                for (byte i = 0; i < fwuMDGetData.numberOfReports; i++)
                {
                    firmwareDataIndex = (short)(((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2) + i);
                    if (fwData.Count > (firmwareDataIndex - 1))
                    {
                        List<byte> dataToSend = new List<byte>(fwData[firmwareDataIndex - 1]);
                        dataToSend.Insert(0, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.ID);
                        dataToSend.Insert(1, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT.ID);
                        if (fwData.Count <= firmwareDataIndex)
                        {
                            dataToSend.Insert(2, (byte)((firmwareDataIndex >> 8) | 0x80));
                        }
                        else
                        {
                            dataToSend.Insert(2, (byte)(firmwareDataIndex >> 8));
                        }
                        dataToSend.Insert(3, (byte)firmwareDataIndex);

                        short crc = Utils.Tools.CalculateCrc16(dataToSend);

                        dataToSend.Add((byte)(crc >> 8));
                        dataToSend.Add((byte)crc);

                        fwuMDReportData.Add(dataToSend.ToArray());
                    }
                }
                ApplicationModel.SetBusyMessage("Please wait until the OTA Firmware Update completed.\n" +
                    String.Format("packet# {0} of {1} written.", firmwareDataIndex, fwData.Count));
                return fwuMDReportData;
            }, fwuMDGetData);

            // Start Update.
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        public byte FirmwareUpdateV5(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, byte activation, byte hardwareVersion, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            fwuMDRequestGetData.firmwareTarget = fwTarget;
            fwuMDRequestGetData.fragmentSize = Tools.GetBytes((ushort)fragmentSize);
            fwuMDRequestGetData.properties1.activation = activation;
            fwuMDRequestGetData.hardwareVersion = hardwareVersion;

            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            // Register ResponseData.
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = ResponseMultiData(node, (t, h, x) =>
            {
                ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
                fwuMDGetData = x;
                List<byte[]> fwuMDReportData = new List<byte[]>();
                short firmwareDataIndex = 0;
                for (byte i = 0; i < fwuMDGetData.numberOfReports; i++)
                {
                    firmwareDataIndex = (short)(((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2) + i);
                    if (fwData.Count > (firmwareDataIndex - 1))
                    {
                        List<byte> dataToSend = new List<byte>(fwData[firmwareDataIndex - 1]);
                        dataToSend.Insert(0, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.ID);
                        dataToSend.Insert(1, COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT.ID);
                        if (fwData.Count <= firmwareDataIndex)
                        {
                            dataToSend.Insert(2, (byte)((firmwareDataIndex >> 8) | 0x80));
                        }
                        else
                        {
                            dataToSend.Insert(2, (byte)(firmwareDataIndex >> 8));
                        }
                        dataToSend.Insert(3, (byte)firmwareDataIndex);

                        short crc = Utils.Tools.CalculateCrc16(dataToSend);

                        dataToSend.Add((byte)(crc >> 8));
                        dataToSend.Add((byte)crc);

                        fwuMDReportData.Add(dataToSend.ToArray());
                    }
                }
                ApplicationModel.SetBusyMessage("Please wait until the OTA Firmware Update completed.\n" +
                    String.Format("packet# {0} of {1} written.", firmwareDataIndex, fwData.Count));
                return fwuMDReportData;
            }, fwuMDGetData);

            // Start Update.
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        private byte MDRequestGet(NodeTag node, byte[] fwuMDRequestGetData, byte[] fwuMDRequestReportData)
        {
            byte ret = 0;
            ActionToken token = null;
            var res = RequestData(node, fwuMDRequestGetData, ref fwuMDRequestReportData, 3000, token);
            if (res)
            {
                CommandClassValue[] cmdClassValues = null;
                ApplicationModel.ZWaveDefinition.ParseApplicationObject(fwuMDRequestReportData, out cmdClassValues);
                if (cmdClassValues != null && cmdClassValues.Length > 0)
                {
                    var cmdValue = cmdClassValues[0].CommandValue;
                    if (cmdValue != null && cmdValue.ParamValues != null && cmdValue.ParamValues.Count > 0)
                    {
                        ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = cmdValue.ParamValues[0].TextValue;
                        if (cmdValue.ParamValues[0].ByteValueList != null && cmdValue.ParamValues[0].ByteValueList.Count > 0)
                        {
                            ret = cmdValue.ParamValues[0].ByteValueList[0];
                        }
                        Logger.Log(cmdValue.ParamValues[0].TextValue);
                    }
                }
            }
            return ret;
        }

        public void OnMDStatusReportAction(NodeTag node, Func<ReceiveStatuses, NodeTag, NodeTag, byte[], byte[]> responseCallback, ActionToken token)
        {
            Func<byte[], byte[], byte[]> c = (h, a) => { return responseCallback(0, NodeTag.Empty, NodeTag.Empty, a); };
            token = ResponseData(node, c, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT());
        }

        public void CancelFirmwareUpdateV1(NodeTag node, int waitTimeoutMs)
        {
            ActionToken token = null;
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = ExpectData(node, fwuMDGetData, waitTimeoutMs, out token);
            if (res && res.ReceivedData != null)
            {
                fwuMDGetData = res.ReceivedData;
                // Fake last data packet.
                var rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT();
                rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                rpt.properties1.last = 0x01;
                rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                var ret = SendData(node, rpt, token);
                if (ret)
                {
                    // Just wait for status, status is handled OnStatusReportCallback.
                    ExpectData(node, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000, out token);
                }
            }
        }


        public void CancelFirmwareUpdateV2(NodeTag node, int waitTimeoutMs)
        {
            ActionToken token = null;
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = ExpectData(node, fwuMDGetData, waitTimeoutMs, out token);
            if (res && res.ReceivedData != null)
            {
                fwuMDGetData = res.ReceivedData;
                // Fake last data packet.
                var rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT();
                rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                rpt.properties1.last = 0x01;
                rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                rpt.checksum = Tools.CalculateCrc16Array((byte[])rpt, 0, ((byte[])rpt).Length - 2);

                var ret = SendData(node, rpt, token);
                if (ret)
                {
                    // Just wait for status, status is handled OnStatusReportCallback.
                    ExpectData(node, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000, out token);
                }
            }
        }

        public void CancelFirmwareUpdateV3(NodeTag node, int waitTimeoutMs)
        {
            ActionToken token = null;
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = ExpectData(node, fwuMDGetData, waitTimeoutMs, out token);
            if (res && res.ReceivedData != null)
            {
                fwuMDGetData = res.ReceivedData;
                // Fake last data packet.
                var rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT();
                rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                rpt.properties1.last = 0x01;
                rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                rpt.checksum = Tools.CalculateCrc16Array((byte[])rpt, 0, ((byte[])rpt).Length - 2);

                var ret = SendData(node, rpt, token);
                if (ret)
                {
                    // Just wait for status, status is handled OnStatusReportCallback.
                    ExpectData(node, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000, out token);
                }

            }
        }

        public void CancelFirmwareUpdateV4(NodeTag node, int waitTimeoutMs)
        {
            ActionToken token = null;
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = ExpectData(node, fwuMDGetData, waitTimeoutMs, out token);
            if (res && res.ReceivedData != null)
            {
                fwuMDGetData = res.ReceivedData;
                // Fake last data packet.
                var rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT();
                rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                rpt.properties1.last = 0x01;
                rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                rpt.checksum = Tools.CalculateCrc16Array((byte[])rpt, 0, ((byte[])rpt).Length - 2);

                var ret = SendData(node, rpt, token);
                if (ret)
                {
                    // Just wait for status, status is handled OnStatusReportCallback.
                    ExpectData(node, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000, out token);
                }

            }
        }

        public void CancelFirmwareUpdateV5(NodeTag node, int waitTimeoutMs)
        {
            ActionToken token = null;
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = ExpectData(node, fwuMDGetData, waitTimeoutMs, out token);
            if (res && res.ReceivedData != null)
            {
                fwuMDGetData = res.ReceivedData;
                // Fake last data packet.
                var rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT();
                rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                rpt.properties1.last = 0x01;
                rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                rpt.checksum = Tools.CalculateCrc16Array((byte[])rpt, 0, ((byte[])rpt).Length - 2);

                var ret = SendData(node, rpt, token);
                if (ret)
                {
                    // Just wait for status, status is handled OnStatusReportCallback.
                    ExpectData(node, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000, out token);
                }
            }
        }

        public void CancelFirmwareUpdateOTADownload()
        {
            ApplicationModel.SetBusyMessage("OTA Firmware Download cancelling.");
            ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = null;
            isDownloadCancelled = true;
        }

        public CommandExecutionResult FirmwareUpdateOTAGet(NodeTag node)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Sending OTA Firmware Update Get command.");
            Logger.Log(ApplicationModel.BusyMessage);

            // Request FIRMWARE UPDATE MD COMMAND CLASS VERSION and wait for report.
            var versionCmdClassGetData = new COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_GET();
            versionCmdClassGetData.requestedCommandClass = COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.ID;

            byte[] versionCmdClassReportData = new COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_REPORT();
            ActionToken token = null;
            var res = RequestData(
                node,
                versionCmdClassGetData,
                ref versionCmdClassReportData, 3000, token);

            if (res)
            {
                COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_REPORT cmd = versionCmdClassReportData;
                if (cmd.requestedCommandClass == COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.ID)
                {

                    // Request current firmware VERSION and wait for report.
                    var verGetData = new COMMAND_CLASS_VERSION.VERSION_GET();
                    byte[] verRptData = new COMMAND_CLASS_VERSION.VERSION_REPORT();
                    var verRes = RequestData(
                        node,
                        verGetData,
                        ref verRptData, 3000, token);
                    if (verRes)
                    {
                        COMMAND_CLASS_VERSION.VERSION_REPORT cverRptData = verRptData;
                        ApplicationModel.FirmwareUpdateModel.CurrentFirmwareVersion = cverRptData.applicationVersion.ToString() + "." +
                            cverRptData.applicationSubVersion.ToString("00");
                    }
                    else
                    {
                        ApplicationModel.FirmwareUpdateModel.CurrentFirmwareVersion = "";
                    }
                    ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion = cmd.commandClassVersion;
                    if (cmd.commandClassVersion == 0x01)
                    {
                        // Request COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props.
                        var fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_GET();
                        byte[] fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT();
                        res = RequestData(
                            node,
                            fwuGetData,
                            ref fwuReportData, 3000, token);
                        if (res)
                        {
                            COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT cfwuReportData = fwuReportData;
                            ApplicationModel.FirmwareUpdateModel.Checksum = cfwuReportData.checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = cfwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = cfwuReportData.firmwareId;
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = 40; // default 40
                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0,
                                cfwuReportData.firmwareId));
                            ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = null;
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = firmwareTargets[0];
                        }
                    }
                    else if (cmd.commandClassVersion == 0x02)
                    {
                        //Request COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props
                        var fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_GET();
                        byte[] fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT();
                        res = RequestData(
                            node,
                            fwuGetData,
                            ref fwuReportData, 3000, token);
                        if (res)
                        {
                            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT cfwuReportData = fwuReportData;
                            ApplicationModel.FirmwareUpdateModel.Checksum = cfwuReportData.checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = cfwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = cfwuReportData.firmwareId;
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = 40; // default 40
                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, cfwuReportData.firmwareId));
                            ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = null;
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = firmwareTargets[0];
                        }
                    }
                    else if (cmd.commandClassVersion == 0x03 || cmd.commandClassVersion == 0x04) //V4 And V3
                    {
                        // Request COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props.
                        var fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_GET();
                        byte[] fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT();
                        res = RequestData(
                            node,
                            fwuGetData,
                            ref fwuReportData, 3000, token);
                        if (res)
                        {
                            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT cfwuReportData = fwuReportData;
                            ApplicationModel.FirmwareUpdateModel.Checksum = cfwuReportData.firmware0Checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = cfwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = cfwuReportData.firmware0Id;
                            ApplicationModel.FirmwareUpdateModel.IsFirmwareUpgradable = (cfwuReportData.firmwareUpgradable != 0x00);
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = Tools.GetInt32(cfwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.NumberOfFirmwareTargets = cfwuReportData.numberOfFirmwareTargets;

                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, cfwuReportData.firmware0Id));
                            if (cfwuReportData.vg1 != null)
                            {
                                foreach (var item in cfwuReportData.vg1)
                                {
                                    if (item.firmwareId != null)
                                    {
                                        firmwareTargets.Add(new FirmwareTarget(firmwareTargets.Count, item.firmwareId));
                                    }
                                }
                            }
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            if (ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget == null && firmwareTargets.Count > 0)
                            {
                                ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = firmwareTargets[0];
                            }
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else if (cmd.commandClassVersion == 0x05) //V5
                    {
                        var fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_MD_GET();
                        byte[] fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_MD_REPORT();
                        res = RequestData(
                            node,
                            fwuGetData,
                            ref fwuReportData, 3000, token);
                        if (res)
                        {
                            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_MD_REPORT cfwuReportData = fwuReportData;
                            ApplicationModel.FirmwareUpdateModel.Checksum = cfwuReportData.firmware0Checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = cfwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = cfwuReportData.firmware0Id;
                            ApplicationModel.FirmwareUpdateModel.IsFirmwareUpgradable = (cfwuReportData.firmwareUpgradable != 0x00);
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = Tools.GetInt32(cfwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = Tools.GetInt32(cfwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.HardwareVersion = cfwuReportData.hardwareVersion;
                            ApplicationModel.FirmwareUpdateModel.NumberOfFirmwareTargets = cfwuReportData.numberOfFirmwareTargets;

                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, cfwuReportData.firmware0Id));
                            if (cfwuReportData.vg1 != null)
                            {
                                foreach (var item in cfwuReportData.vg1)
                                {
                                    if (item.firmwareId != null)
                                    {
                                        firmwareTargets.Add(new FirmwareTarget(firmwareTargets.Count, item.firmwareId));
                                    }
                                }
                            }
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            if (ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget == null && firmwareTargets.Count > 0)
                            {
                                ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = firmwareTargets[0];
                            }
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else
                    {
                        ret = CommandExecutionResult.Failed;
                        Logger.Log("OTA Firmware Update is not supported");
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssignReturnRoute(NodeTag srcNode, NodeTag[] destNodes, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            token = null;
            ApplicationModel.SetBusyMessage("Assign Return Route started.");
            Logger.Log(ApplicationModel.BusyMessage);
            if (destNodes != null)
            {
                foreach (var destNode in destNodes)
                {
                    token = _zipController.RequestData(_headerExtension, new COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.RETURN_ROUTE_ASSIGN()
                    {
                        sourceNodeId = (byte)srcNode.Id,
                        destinationNodeId = (byte)destNode.Id
                    }, new COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.RETURN_ROUTE_ASSIGN_COMPLETE(), 10000, null);
                    var result = token.WaitCompletedSignal();
                    if (result)
                    {
                        Logger.Log($"Return Route from Node {srcNode} to Node {destNode} assigned successfully");
                    }
                    else
                    {
                        Logger.LogFail($"Return Route from Node {srcNode} to Node {destNode} assign failed");
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssignSUCReturnRoute(NodeTag srcNode, out ActionToken token)
        {
            token = null;
            return CommandExecutionResult.OK; // Do Nothing
        }

        public CommandExecutionResult AssignPriorityReturnRoute(NodeTag srcNode, NodeTag destNode, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token)
        {
            token = null;
            return CommandExecutionResult.OK; // Do Nothing
        }

        public CommandExecutionResult AssignPrioritySUCReturnRoute(NodeTag srcNode, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token)
        {
            token = null;
            return CommandExecutionResult.OK; // Do Nothing
        }

        public CommandExecutionResult AssociationGroupNameGet(NodeTag associativeDevice, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;
            ActionResult result;
            var cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_GET();
            cmd.groupingIdentifier = groupId;
            byte[] rptData = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_REPORT();
            if (associativeDevice.EndPointId == 0)
            {
                result = RequestData(associativeDevice, cmd, ref rptData, 3000);
            }
            else
            {
                result = RequestData(associativeDevice, EncapData(cmd, associativeDevice.EndPointId), ref rptData, 3000);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_REPORT rpt = rptData;
                var grName = Encoding.UTF8.GetString(rpt.name.ToArray());
                var ad = ApplicationModel.AssociationsModel.
                    AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                    Devices.FirstOrDefault(x => x.Device == associativeDevice);
                if (ad != null)
                {
                    var group = ad.Groups.FirstOrDefault(x => x.Id == groupId);
                    if (group != null)
                    {
                        group.GroupName = grName;
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGetGroupInfo(NodeTag associativeDevice, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;

            var cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_GET();
            cmd.groupingIdentifier = groupId;
            var rpt = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_REPORT();
            byte[] rptData = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_REPORT();
            ActionResult result;
            if (associativeDevice.EndPointId == 0)
            {
                result = RequestData(associativeDevice, cmd, ref rptData, 3000);
            }
            else
            {
                result = RequestData(associativeDevice, EncapData(cmd, associativeDevice.EndPointId), ref rptData, 3000);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                rpt = rptData;
                CommandClassValue[] ccv = null;
                ApplicationModel.ZWaveDefinition.ParseApplicationObject(rpt, out ccv);
                if (ccv != null && ccv.Length > 0)
                {
                    var pVal = ccv[0].CommandValue.ParamValues;
                    if (pVal.Count > 1)
                    {
                        string profile1 = pVal[1].InnerValues.Where(x => x.ParamDefinitionText == "Profile1").FirstOrDefault().TextValue;
                        string profile2 = pVal[1].InnerValues.Where(x => x.ParamDefinitionText == "Profile2").FirstOrDefault().TextValue;

                        var ad = ApplicationModel.AssociationsModel.
                                AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                                Devices.FirstOrDefault(x => x.Device == associativeDevice);
                        if (ad != null)
                        {
                            var group = ad.Groups.FirstOrDefault(x => x.Id == groupId);
                            if (group != null)
                            {
                                group.SetGroupInfo(profile1, profile2);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGetCommandList(NodeTag associativeDevice, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;
            var cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_GET();
            byte[] rptData = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_REPORT();
            cmd.groupingIdentifier = groupId;
            ActionResult result;
            if (associativeDevice.EndPointId == 0)
            {
                result = RequestData(associativeDevice, cmd, ref rptData, 3000);
            }
            else
            {
                result = RequestData(associativeDevice, EncapData(cmd, associativeDevice.EndPointId), ref rptData, 3000);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_REPORT rpt = rptData;
                List<string> tmpCommands = new List<string>();
                for (int i = 0; i < rpt.command.Count - 1; i += 2)
                {
                    var tmpCommand = ApplicationModel.ZWaveDefinition.ParseApplicationStringName(rpt.command.Skip(i).Take(2).ToArray());
                    tmpCommands.Add(tmpCommand);
                }
                var ad = ApplicationModel.AssociationsModel.
                    AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                    Devices.FirstOrDefault(x => x.Device == associativeDevice);
                if (ad != null)
                {
                    var group = ad.Groups.FirstOrDefault(x => x.Id == groupId);
                    if (group != null)
                    {
                        group.GroupCommandClasses = tmpCommands;
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationRemove(NodeTag node, byte groupId, IEnumerable<NodeTag> nodes)
        {
            var ret = CommandExecutionResult.Failed;
            if (node.EndPointId == 0)
            {
                var pairs = nodes.Select(x => Tools.GetBytes(x.Id)).ToArray();
                if (pairs.Where(x => x[0] == 0).Count() > 0)
                {
                    var cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REMOVE();
                    cmd.groupingIdentifier = groupId;
                    cmd.nodeId = pairs.Where(x => x[0] == 0).Select(x => x[1]).ToList();
                    ActionToken token = null;
                    if (SendData(node, cmd, token))
                    {
                        ret = CommandExecutionResult.OK;
                    }
                }
                if (_zipController.Network.HasCommandClass(node, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID) &&
                    pairs.Where(x => x[0] != 0).Count() > 0)
                {
                    var cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE();
                    cmd.groupingIdentifier = groupId;
                    foreach (var item in pairs.Where(x => x[0] != 0))
                    {
                        var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG.Tproperties1();
                        prop.endPoint = item[0];
                        var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG();
                        tvg.multiChannelNodeId = item[1];
                        tvg.properties1 = prop;
                        cmd.vg.Add(tvg);
                    }
                    ActionToken token = null;
                    if (SendData(node, cmd, token))
                    {
                        ret = CommandExecutionResult.OK;
                    }
                }
            }
            else
            {
                var pairs = nodes.Select(x => Tools.GetBytes(x.Id)).ToArray();
                var cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = pairs.Where(x => x[0] == 0).Select(x => x[1]).ToList();
                foreach (var item in pairs.Where(x => x[0] != 0))
                {
                    var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG.Tproperties1();
                    prop.endPoint = item[0];
                    var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG();
                    tvg.multiChannelNodeId = item[1];
                    tvg.properties1 = prop;
                    cmd.vg.Add(tvg);
                }
                ActionToken token = null;
                if (SendData(node, EncapData(cmd, node.EndPointId), token))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationCreate(NodeTag node, byte groupId, IEnumerable<NodeTag> nodes)
        {
            var ret = CommandExecutionResult.Failed;
            if (node.EndPointId == 0)
            {
                var pairs = nodes.Select(x => Tools.GetBytes(x.Id)).ToArray();
                if (pairs.Where(x => x[0] == 0).Count() > 0)
                {
                    COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET();
                    cmd.groupingIdentifier = groupId;
                    cmd.nodeId = pairs.Where(x => x[0] == 0).Select(x => x[1]).ToList();
                    ActionToken token = null;
                    if (SendData(node, cmd, token))
                    {
                        ret = CommandExecutionResult.OK;
                    }
                }
                if (_zipController.Network.HasCommandClass(node, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID) &&
                    pairs.Where(x => x[0] != 0).Count() > 0)
                {
                    var cmdM = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET();
                    cmdM.groupingIdentifier = groupId;
                    foreach (var item in pairs.Where(x => x[0] != 0))
                    {
                        var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG.Tproperties1();
                        prop.endPoint = item[0];
                        var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG();
                        tvg.multiChannelNodeId = item[1];
                        tvg.properties1 = prop;
                        cmdM.vg.Add(tvg);
                    }
                    ActionToken token = null;
                    if (SendData(node, cmdM, token))
                    {
                        ret = CommandExecutionResult.OK;
                    }
                }
            }
            else
            {
                var pairs = nodes.Select(x => Tools.GetBytes(x.Id)).ToArray();
                var cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = pairs.Where(x => x[0] == 0).Select(x => x[1]).ToList();
                foreach (var item in pairs.Where(x => x[0] != 0))
                {
                    var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG.Tproperties1();
                    prop.endPoint = item[0];
                    var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG();
                    tvg.multiChannelNodeId = item[1];
                    tvg.properties1 = prop;
                    cmd.vg.Add(tvg);
                }
                ActionToken token = null;
                if (SendData(node, EncapData(cmd, node.EndPointId), token))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGroupingsGet(NodeTag associativeDevice)
        {
            var ret = CommandExecutionResult.Failed;
            ActionResult result;
            byte[] rptData;
            if (associativeDevice.EndPointId == 0)
            {
                rptData = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_REPORT();
                result = RequestData(associativeDevice, new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_GET(), ref rptData, 3000);
            }
            else
            {
                rptData = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_REPORT();
                result = RequestData(associativeDevice,
                    EncapData(new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_GET(), associativeDevice.EndPointId),
                    ref rptData, 3000);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                byte supportedGroupings;
                if (associativeDevice.EndPointId == 0)
                {
                    COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_REPORT rpt = rptData;
                    supportedGroupings = rpt.supportedGroupings;
                }
                else
                {
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_REPORT rpt = rptData;
                    supportedGroupings = rpt.supportedGroupings;
                }
                var ad = ApplicationModel.AssociationsModel.
                    AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                    Devices.FirstOrDefault(x => x.Device == associativeDevice);
                if (ad != null)
                {
                    ad.SetGroups(supportedGroupings);
                    if (ad.Groups != null && ad.Groups.Count > 0)
                    {
                        foreach (var item in ad.Groups)
                        {
                            AssociationGet(ad.Device, item.Id);
                        }
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGet(NodeTag associativeDevice, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;
            if (associativeDevice.EndPointId == 0)
            {
                var cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GET();
                cmd.groupingIdentifier = groupId;
                byte[] rptData = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REPORT();
                var result = RequestData(associativeDevice, cmd, ref rptData, 3000);
                if (result)
                {
                    ret = CommandExecutionResult.OK;
                    COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REPORT rpt = rptData;
                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                        Devices.FirstOrDefault(x => x.Device == associativeDevice);
                    if (ad != null)
                    {
                        var nodePairIds = rpt.nodeid.Select(x => new NodeTag(x, 0)).ToList();
                        ad.UpdateGroup(rpt.groupingIdentifier, rpt.maxNodesSupported, nodePairIds);
                    }
                }

                if (_zipController.Network.HasCommandClass(associativeDevice, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
                {
                    var cmdm = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET();
                    cmdm.groupingIdentifier = groupId;
                    byte[] rptm = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT();
                    var resultm = RequestData(associativeDevice, cmdm, ref rptm, 3000);
                    if (resultm)
                    {
                        ret = CommandExecutionResult.OK;
                        COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT rpt = rptm;
                        var ad = ApplicationModel.AssociationsModel.
                            AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                            Devices.FirstOrDefault(x => x.Device == associativeDevice);
                        if (ad != null)
                        {
                            IList<NodeTag> nodeIds = rpt.nodeId.Select(x => new NodeTag(x, 0)).ToList();
                            nodeIds.AddRange(rpt.vg.Select(x => new NodeTag(x.multiChannelNodeId, x.properties1.endPoint)).ToArray());
                            ad.UpdateGroup(rpt.groupingIdentifier, rpt.maxNodesSupported, nodeIds);
                        }
                    }
                }
            }
            else
            {
                var cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET();
                cmd.groupingIdentifier = groupId;
                byte[] rptData = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT();
                var result = RequestData(associativeDevice, EncapData(cmd, associativeDevice.EndPointId), ref rptData, 3000);
                if (result)
                {
                    ret = CommandExecutionResult.OK;
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT rpt = rptData;
                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                        Devices.FirstOrDefault(x => x.Device == associativeDevice);
                    if (ad != null)
                    {
                        IList<NodeTag> nodeIds = rpt.nodeId.Select(x => new NodeTag(x, 0)).ToList();
                        nodeIds.AddRange(rpt.vg.Select(x => new NodeTag(x.multiChannelNodeId, x.properties1.endPoint)).ToArray());
                        ad.UpdateGroup(rpt.groupingIdentifier, rpt.maxNodesSupported, nodeIds);
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult RequestNodeNeighborUpdate(NodeTag node, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;

            ApplicationModel.SetBusyMessage("Request Neighbors Update");
            Logger.Log(ApplicationModel.BusyMessage);

            token = _zipController.RequestNodeNeighborUpdate(node, 65000, null);
            var result = token.WaitCompletedSignal();
            if (result.IsStateCompleted)
            {
                Logger.Log("Request Neighbors Update Done");
                ret = CommandExecutionResult.OK;
                ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeNeighborUpdateCommand), Message = "Neighbor Node Updated" }));
            }
            else
            {
                Logger.Log(string.Format("Request Neighbor Update Failed with state {0}", result.State));
            }
            return ret;
        }

        public CommandExecutionResult StartPowerLevelTest(NodeTag node, NodeTag destinationNode, byte powerLevel, ActionToken token)
        {
            var ret = CommandExecutionResult.Inconclusive;
            var powerLevelSet = new COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_SET();
            powerLevelSet.powerLevel = powerLevel;
            powerLevelSet.testFrameCount = new byte[] { 0x00, 0x0A }; //10 (NOPs)
            powerLevelSet.testNodeid = (byte)destinationNode.Id;
            var powerLevelGet = new COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_GET();
            byte[] rptData = new COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT();
            ActionResult requestDataResult = null;
            if (node.Id == _zipController.Id)
            {
                var sendDataResult = SendData(node, powerLevelSet, token);
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(3000);
                    requestDataResult = RequestData(
                        node,
                        powerLevelGet,
                        ref rptData, 10000, token); //10 Seconds
                    COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT powerLevelReport = rptData;
                    if (powerLevelReport.statusOfOperation != 0x02) // in progress
                        break;
                }
            }
            else
            {
                requestDataResult = RequestData(
                    node,
                    powerLevelSet,
                    ref rptData, 10000, token); //10 Seconds
            }

            if (requestDataResult)
            {
                COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT powerLevelReport = rptData;
                if (powerLevelReport.statusOfOperation == 0x01)
                {
                    Logger.Log(string.Format("LWRdB {0}dB nodes: {1} - {2} Passed", powerLevel, node, destinationNode));
                    ret = CommandExecutionResult.OK;
                }
                else
                {
                    Logger.Log(string.Format("LWRdB {0}dB nodes: {1} - {2} FAILED", powerLevel, node, destinationNode));
                    ret = CommandExecutionResult.Failed;
                }
            }
            else
            {
                if (requestDataResult.State == ActionStates.Cancelled)
                {
                    Logger.LogFail("Power Level Operation Cancelled");
                    ret = CommandExecutionResult.Canceled;
                }
                else
                    Logger.Log(string.Format("LWRdB {0}dB nodes: {1} - {2} FAILED. Report Missing", powerLevel, node, destinationNode));
            }
            return ret;
        }

        public ActionResult SendData(NodeTag node, byte[] data, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false)
        {
            return SendData(node, data, token);
        }

        public ActionResult SendData(NodeTag[] nodeIds, byte[] data, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false)
        {
            Logger.Log("Multicast send not implemented for ZIP");
            token = null;
            return null;
        }

        public ActionResult SetLearnMode(LearnModes mode, out ActionToken token)
        {
            var caption = "Controller is working in learning mode.";
            ControllerSessionsContainer.Config.DeleteConfiguration(_zipController);
            token = _zipController.SetLearnMode(mode, 65000, null);
            using (var logAction = ReportAction(caption, token))
            {
                SetLearnModeResult result = (SetLearnModeResult)token.WaitCompletedSignal();
                if (result)
                {
                    if (result.Status == 0x07)
                    {
                        logAction.Caption = "Learn failed";
                    }
                    else if (result.Status == 0x09)
                    {
                        logAction.Caption = "Learn mode completed (Security Failed)";
                    }
                }
                Update(false);
                return result;
            }
        }

        public ActionResult SetDefault(bool isDefaultCommandClasses, out ActionToken token)
        {
            var caption = "Reset Controller";
            var busyText = "The controller is being reset. Please wait.";
            ControllerSessionsContainer.Config.DeleteConfiguration(_zipController);
            token = _zipController.SetDefault(65000, null);
            using (ReportAction(caption, busyText, token))
            {
                var ret = token.WaitCompletedSignal();
                Disconnect();
                CommunicationStatuses reinitResult = Connect(DataSource);
                if (reinitResult == CommunicationStatuses.Done)
                {
                    ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetDefaultCommand), Message = "Switched to Default" }));
                }
                return ret;
            }
        }

        public ActionResult ControllerChange(out ActionToken token)
        {
            var caption = "Shift the primary role to another controller in the network";
            var busyText = "Shifting the primary role to another controller in the network. Set the receiving controller in the learning mode.";
            ControllerSessionsContainer.Config.DeleteConfiguration(_zipController);
            token = _zipController.ControllerChange(Modes.NodeAny, ControllerSessionsContainer.Config.TxOptions, 65000, null);
            using (ReportAction(caption, busyText, token))
            {
                ControllerChangeResult ret = (ControllerChangeResult)token.WaitCompletedSignal();
                if (ret)
                {
                    DeviceAdd(ret.Node);
                    if (ret.CommandClasses != null && ret.CommandClasses.Contains(COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID))
                    {
                        //GetNodeRoleType(ret.Id);
                    }
                    _zipController.Network.SetNodeInfo(ret.Node, ret.NodeInfo);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.AddOrUpdateNode(ret.Node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo | NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, "Controller Shift");
                    });
                    Update(false);
                }
                return ret;
            }
        }

        public ActionResult RequestNetworkUpdate(out ActionToken token)
        {
            var caption = "Request network update";
            token = _zipController.RequestNetworkUpdate(10000, null);
            using (ReportAction(caption, token))
            {
                var ret = (RequestNetworkUpdateResult)token.WaitCompletedSignal();
                if (ret)
                {
                    Update(false);
                    ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNetworkUpdateCommand), Message = "Network Updated" }));
                }
                return ret;
            }
        }

        private ExpectDataResult ExpectData(NodeTag node, byte[] cmdData, int waitTimeoutMs, out ActionToken token)
        {
            ExpectDataResult ret = null;
            if (node.Id == _zipController.Id)
            {
                token = _zipController.ExpectData(cmdData, waitTimeoutMs, null);
                ret = (ExpectDataResult)token.WaitCompletedSignal();
            }
            else
            {
                token = Devices[node].ExpectData(cmdData, waitTimeoutMs, null);
                ret = (ExpectDataResult)token.WaitCompletedSignal();
            }
            return ret;
        }

        private ActionToken ResponseData(NodeTag node, Func<byte[], byte[], byte[]> responseCallback, byte[] cmdData)
        {
            ActionToken ret = null;
            if (node.Id == _zipController.Id)
            {
                ret = _zipController.ResponseData(_headerExtension, responseCallback, cmdData);
            }
            else
            {
                ret = Devices[node].ResponseData(_headerExtension, responseCallback, cmdData);
            }
            return ret;
        }

        private ActionToken ResponseMultiData(NodeTag node, Func<ActionToken, byte[], byte[], List<byte[]>> responseCallback, byte[] cmdData)
        {
            ActionToken ret = null;
            if (node.Id == _zipController.Id)
            {
                ret = _zipController.ResponseMultiData(_headerExtension, responseCallback, cmdData);
            }
            else
            {
                ret = Devices[node].ResponseMultiData(_headerExtension, responseCallback, cmdData);
            }
            return ret;
        }

        private PingReply Ping(NodeTag node)
        {
            if (!Devices.ContainsKey(node))
            {
                DeviceAdd(node);
            }
            Ping ping = new Ping();
            PingReply reply = ping.Send(Devices[node].IpAddress);
            return reply;
        }

        public void Cancel(NodeTag cancelNode, ActionToken token)
        {
            if (cancelNode.Id > 0 && cancelNode != _zipController.Network.NodeTag)
            {
                if (Devices.ContainsKey(cancelNode))
                {
                    Devices[cancelNode].Cancel(token);
                }
            }
            else
                _zipController.Cancel(token);
        }

        public void SendNop(NodeTag node, out ActionToken token)
        {
            token = null;
            ApplicationModel.SetBusyMessage("Ping Node Started");
            Logger.Log(ApplicationModel.BusyMessage);
            PingReply reply = Ping(node);
            Logger.Log(string.Format("Ping Done - Node {0} - {1} - Latency {2} ms", node, reply.Status, reply.RoundtripTime));
        }

        public void RequestNodeInfo(NodeTag node, ActionToken token)
        {
            if (node.EndPointId == 0)
            {
                var caption = "Request Node information from the network";
                token = _zipController.GetCachedNodeInfo(node, 20000, null);
                using (ReportAction(caption, token))
                {
                    GetCachedNodeInfoResult ret = (GetCachedNodeInfoResult)token.WaitCompletedSignal();
                    if (ret)
                    {
                        _zipController.Network.SetNodeInfo(node, ret.NodeInfo);
                        _zipController.Network.SetSecuritySchemes(node, ret.SecuritySchemes);
                        _zipController.Network.SetCommandClasses(node, ret.CommandClasses);
                        _zipController.Network.SetSecureCommandClasses(node, ret.SecureCommandClasses);
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeInfoCommand), Message = "Node Info Received" });
                        });
                        ApplicationModel.AssociationsModel.UpdateAssociativeDevice(node);
                        if (_zipController.Network.HasCommandClass(node, COMMAND_CLASS_MULTI_CHANNEL_V4.ID))
                        {
                            AddEndPointsForMultiChannelNode(node, token);
                        }
                    }
                }
            }
            else
            {
                var caption = "Request Node information from the network";
                var res = GetEndPointCapability(Controller.Network.NodeTag, node, node.EndPointId, token);
                using (ReportAction(caption, token))
                {
                    _zipController.Network.SetNodeInfo(node, res.genericDeviceClass, res.specificDeviceClass);
                    byte[] cc;
                    byte[] sec_cc;
                    ZWaveDefinition.TryParseCommandClassRef(res.commandClass, out cc, out sec_cc);
                    _zipController.Network.SetCommandClasses(node, cc);
                    _zipController.Network.SetSecureCommandClasses(node, sec_cc);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeInfoCommand), Message = "Node Info Received" });
                    });
                }
            }
        }

        private void AddEndPointsForMultiChannelNode(NodeTag parentNode, ActionToken token)
        {
            bool isIdentical = true;
            byte endPoints = 0;
            // Get end points
            var cmd = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_END_POINT_GET();
            if (_zipController.Network.IsLongRangeEnabled(parentNode))
            {
                cmd.nodeid = 0xFF;
                cmd.extendedNodeid = new byte[] { (byte)(parentNode.Id >> 8), (byte)parentNode.Id };
            }
            else
            {
                cmd.nodeid = (byte)parentNode.Id;
                cmd.extendedNodeid = new byte[] { 0x00, (byte)parentNode.Id };
            }
            byte[] rptData = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V2.NM_MULTI_CHANNEL_END_POINT_REPORT();
            var result = RequestData(Controller.Network.NodeTag, cmd, ref rptData, 5000, token);
            if (result)
            {
                COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_END_POINT_REPORT rpt = rptData;
                isIdentical = rpt.properties2.aggregatedEndPoints == 0x01;
                endPoints = rpt.properties1.individualEndPoints;
            }
            // Get capabilities for each end point
            var capabilities = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_CAPABILITY_REPORT[endPoints];
            if (isIdentical)
            {
                var endPointCapabilityReport = GetEndPointCapability(Controller.Network.NodeTag, parentNode, 1, token);
                for (byte i = 0; i < endPoints; i++)
                {
                    capabilities[i] = endPointCapabilityReport;
                }
            }
            else
            {
                for (byte i = 1; i <= endPoints; i++)
                {
                    var endPointCapabilityReport = GetEndPointCapability(Controller.Network.NodeTag, parentNode, i, token);
                    capabilities[i - 1] = endPointCapabilityReport;
                }
            }
            for (byte i = 0; i < endPoints; i++)
            {
                byte endPoint = (byte)(i + 1);
                var node = new NodeTag(parentNode.Id, endPoint);
                var nInfo = _zipController.Network.GetNodeInfo(parentNode);
                nInfo.Generic = capabilities[i].genericDeviceClass;
                nInfo.Specific = capabilities[i].specificDeviceClass;
                _zipController.Network.SetNodeInfo(node, nInfo);
                byte[] cc;
                byte[] sec_cc;
                ZWaveDefinition.TryParseCommandClassRef(capabilities[i].commandClass, out cc, out sec_cc);
                _zipController.Network.SetCommandClasses(node, cc);
                _zipController.Network.SetSecureCommandClasses(node, sec_cc);
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                });
            }
        }

        private COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_CAPABILITY_REPORT GetEndPointCapability(NodeTag ctrl, NodeTag node, byte endPoint, ActionToken token)
        {
            var ret = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_CAPABILITY_REPORT();
            var cmd = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_CAPABILITY_GET();
            if (_zipController.Network.IsLongRangeEnabled(node))
            {
                cmd.nodeid = 0xFF;
                cmd.extendedNodeid = new byte[] { (byte)(node.Id >> 8), (byte)node.Id };
            }
            else
            {
                cmd.nodeid = (byte)node.Id;
                cmd.extendedNodeid = new byte[] { 0x00, (byte)node.Id };
            }
            cmd.properties1.res1 = 0x00;
            cmd.properties1.endPoint = endPoint;
            byte[] rptData = new COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NM_MULTI_CHANNEL_CAPABILITY_REPORT();
            var result = RequestData(ctrl, cmd, ref rptData, 5000, token); //Controller.Id
            if (result)
            {
                ret = rptData;
            }
            return ret;
        }

        public ActionResult ReplaceFailedNode(NodeTag node, ActionToken token)
        {
            var caption = "Replace Failed Node";
            var busyText = "Replacing the non-responding node. Press shortly the pushbutton on the replacement node to be used instead of the failed one.";
            token = _zipController.ReplaceFailedNode(_headerExtension, node, 65000, null);
            using (ReportAction(caption, busyText, token))
            {
                var ret = token.WaitCompletedSignal();
                if (ret)
                {
                    RequestNodeInfo(node, token);
                    _zipController.Network.SetFailed(node, false);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ReplaceFailedCommand), Message = "Failed Replaced" });
                    });
                }
                return ret;
            }
        }

        public ActionResult RemoveFailedNode(NodeTag node, out ActionToken token)
        {
            var caption = "Remove Failed Node";
            var busyText = "Removing the non-responding node.";
            token = _zipController.RemoveFailedNodeId(_headerExtension, node, 10000, null);
            using (ReportAction(caption, busyText, token))
            {
                var ret = (RequestDataResult)token.WaitCompletedSignal();
                if (ret)
                {
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.FAILED_NODE_REMOVE_STATUS cmd = ret.ReceivedData;
                    if (cmd.status == 1)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.RemoveNode(node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveFailedCommand), Message = "Failed Removed" });
                        });
                    }
                }
                return ret;
            }
        }

        public void RemoveNodeNWE(out ActionToken token)
        {
            var caption = "Remove Node";
            var busyText = "Network Wide Exclusion. Controller is waiting for the node information. Press 'Abort' button when the NWE operation is completed.";
            token = null;
            bool isCanceled = false;
            while (!isCanceled)
            {
                token = _zipController.RemoveNodeFromNetwork(Modes.NodeAny | Modes.NodeOptionNetworkWide, 65000, null);
                using (ReportAction(caption, busyText, token))
                {
                    RemoveNodeResult result = (RemoveNodeResult)token.WaitCompletedSignal();
                    if (result)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.RemoveNode(result.Node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveNodeFromNetworkWideCommand), Message = "Node from Network Wide Removed" });
                        });
                    }
                    else if (result.State == ActionStates.Cancelled)
                    {
                        isCanceled = true;
                    }
                }
            }
        }

        public void AddNodeNWI(out ActionToken token)
        {
            var caption = "Add Node";
            var busyText = "Network Wide Inclusion. Controller is waiting for the node information. Press 'Abort' button when the NWI operation is completed.";
            token = null;
            bool isCanceled = false;
            while (!isCanceled)
            {
                token = _zipController.AddNodeToNetwork(Modes.NodeReserved, ControllerSessionsContainer.Config.TxOptions, 65000, null);
                using (var logAction = ReportAction(caption, busyText, token))
                {
                    AddNodeResult result = (AddNodeResult)token.WaitCompletedSignal();
                    if (result && result.Node.Id > 0)
                    {
                        AddNodeToController(result);
                        bool setupResult = SetupLifeLineForNode(result.Node);
                        if (result.Status == 0x09)
                        {
                            logAction.Caption = caption + " (Security Failed)";
                        }
                    }
                    else
                    {
                        if (result.State == ActionStates.Cancelled)
                        {
                            isCanceled = true;
                        }
                    }
                }
            }
            ((IDialog)ApplicationModel.CsaPinDialog).Close();
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddNodeToNetworkNWICommand), Message = "Node Added to NWI Network" }));
        }

        private void AddNodeToController(AddNodeResult result)
        {
            DeviceAdd(result.Node);
            _zipController.Network.SetNodeInfo(result.Node, result.NodeInfo);
            _zipController.Network.SetSecuritySchemes(result.Node, result.SecuritySchemes);
            _zipController.Network.SetCommandClasses(result.Node, result.CommandClasses);
            _zipController.Network.SetSecureCommandClasses(result.Node, result.SecureCommandClasses);
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.AddOrUpdateNode(result.Node);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo | NotifyProperty.NodesList);
            });
        }

        private bool SetupLifeLineForNode(NodeTag node)
        {
            bool ret = false;
            COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET();
            cmd.groupingIdentifier = 0x01;
            cmd.nodeId = new List<byte>();
            cmd.nodeId.Add((byte)_zipController.Network.NodeTag.Id);
            ActionToken token = null;
            if (SendData(node, cmd, token))
            {
                ret = true;
            }
            if (ret)
            {
                Logger.LogOk("Lifeline setup completed for Node: " + node);
            }
            else
            {
                Logger.LogWarning("Lifeline setup failed fro Node: " + node);
            }
            return ret;
        }

        private void DeviceAdd(NodeTag node)
        {
            if (Devices.ContainsKey(node))
                return;

            var device = _zipApplicationLayer.CreateZipDevice();
            InitSubstituteManager(device);
            var ipRes = _zipController.GetIpAddress(node);
            if (ipRes)
            {
                var ds = _zipController.DataSource as SocketDataSource;
                if (ControllerSessionsContainer.Config.ControllerConfiguration != null && ds != null)
                {
                    device.DataSource = new SocketDataSource(ipRes.IpAddress.ToString(), ds.Port, ds.Args)
                    {
                        CanReconnect = true
                    };
                    Devices.Add(node, device);
                    Devices[node].Connect();
                }
            }
        }

        public ActionResult AddNode(out ActionToken token)
        {
            var caption = "Add Node";
            var busyText = "Controller is waiting for the node information. Press shortly the pushbutton on the node to be included in the network.";
            token = _zipController.AddNodeToNetwork(Modes.NodeReserved, ControllerSessionsContainer.Config.TxOptions, 65000, null);
            using (var logAction = ReportAction(caption, busyText, token))
            {
                var ret = (AddNodeResult)token.WaitCompletedSignal();
                if (ret && ret.Node.Id > 0)
                {
                    AddNodeToController(ret);
                    bool setupResult = SetupLifeLineForNode(ret.Node);
                    if (ret.Status == 0x09)
                    {
                        logAction.Caption = caption + " (Security Failed)";
                    }
                }
                ((IDialog)ApplicationModel.CsaPinDialog).Close();
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddNodeCommand), Message = "Node Added" });
                return ret;
            }
        }

        private void ApplyCallbacks(out Func<byte[], byte[]> dskNeededCallback, out Func<IEnumerable<SecuritySchemes>, bool, KEXSetConfirmResult> kexSetConfirmCallback, out Action csaPinCallback)
        {
            dskNeededCallback = (dskFull) =>
            {
                var sb = new StringBuilder();
                for (int i = 2; i < dskFull.Length; i += 2)
                {
                    if (i >= dskFull.Length - 2)
                    {
                        sb.AppendLine(Tools.GetInt32(dskFull.Skip(i).Take(2).ToArray()).ToString("00000"));
                    }
                    else
                    {
                        sb.AppendLine(Tools.GetInt32(dskFull.Skip(i).Take(2).ToArray()).ToString("00000") + "-");
                    }
                }
                ApplicationModel.DskNeededDialog.State.IsHexInputChecked = false;
                ApplicationModel.DskNeededDialog.State.InputData = Tools.GetInt32(dskFull.Take(2).ToArray()).ToString("00000");
                ApplicationModel.DskNeededDialog.State.AdditionalText = sb.ToString();
                return ((IDialog)ApplicationModel.DskNeededDialog).ShowDialog() ? ApplicationModel.DskNeededDialog.GetBytesFromInput() : null;
            };

            kexSetConfirmCallback = (requestedSchemes, isClientSideAuthRequested) =>
            {
                ApplicationModel.KEXSetConfirmDialog.SetInputParameters(requestedSchemes,
                    isClientSideAuthRequested, true, true, true, true);

                return ApplicationModel.KEXSetConfirmDialog.ShowDialog() ? ApplicationModel.KEXSetConfirmDialog.GetResult() : KEXSetConfirmResult.Default;
            };
            csaPinCallback = () =>
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    // Dynamic DSK should be obtained from Z/IP before each inclusion
                    InitZipDsk();
                    if (_zipController.DSK != null && _zipController.DSK.Length == 16)
                    {
                        ApplicationModel.CsaPinDialog.State.AdditionalText = Tools.GetInt32(_zipController.DSK.Take(2).ToArray()).ToString("00000") + " "
                            + Tools.GetInt32(_zipController.DSK.Skip(2).Take(2).ToArray()).ToString("00000");
                    }
                    else
                    {
                        ApplicationModel.CsaPinDialog.State.AdditionalText = "Warning: Could not obtain Z/IP Gateway DSK";
                    }
                    ApplicationModel.CsaPinDialog.ShowDialog();
                });
            };
        }


        public ActionResult RemoveNode(NodeTag removeNode, out ActionToken token)
        {
            var caption = "Remove Node";
            var busyText = "Controller is waiting for the node information. Press shortly the pushbutton on the node to be excluded from the network.";
            token = _zipController.RemoveNodeFromNetwork(Modes.NodeAny, 65000, null);
            using (ReportAction(caption, busyText, token))
            {
                RemoveNodeResult ret = (RemoveNodeResult)token.WaitCompletedSignal();
                if (ret)
                {
                    if (Devices.ContainsKey(ret.Node))
                    {
                        Devices[ret.Node].Dispose();
                        Devices.Remove(ret.Node);
                    }
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.RemoveNode(ret.Node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                    });
                }
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveNodeCommand), Message = "Node Removed" });
                return ret;
            }
        }

        public ActionResult SendNodeInformation(out ActionToken token)
        {
            var caption = "Send Node Information";
            var busyText = "Sending node information.";
            token = _zipController.SendNodeInformation(_headerExtension, 255, ControllerSessionsContainer.Config.TxOptions, null);
            using (ReportAction(caption, busyText, token))
            {
                return token.WaitCompletedSignal();
            }
        }

        public CommunicationStatuses Connect(IDataSource dataSource)
        {
            DataSource = dataSource ?? throw new ArgumentNullException("dataSource");
            ApplicationModel.IsNeedFirstReloadTopology = true;
            _zipController = _zipApplicationLayer.CreateZipController();
            if (_zipController.Connect(DataSource) == CommunicationStatuses.Done)
            {
                ApplicationModel.ClearCommandQueueCollection();
#if !NETCOREAPP
                //throw new NotImplementedException();
                //Settings.Default.LastUsedDevice = DataSource.SourceName;
                //Settings.Default.Save();
#endif
                GetVersionResult getVersionRes = null;
                if (_zipController.GetId() && (getVersionRes = _zipController.GetVersion()))
                {
                    if (getVersionRes.FirmwareVersions.Count > 0)
                    {
                        _zipController.AppVersion = getVersionRes.FirmwareVersions[0];
                    }

                    var getNodesRes = _zipController.GetNodeList(10000);
                    if (getNodesRes.IsBusyReceived)
                    {
                        Logger.LogFail($"Application Busy status: '{getNodesRes.BusyStatus}', wait {getNodesRes.BusyWaitTime} sec.");
                        Thread.Sleep(getNodesRes.BusyWaitTime * 1000);
                        getNodesRes = _zipController.GetNodeList(10000);
                    }

                    if (getNodesRes)
                    {
                        CommandsFactory.CurrentSourceId = DataSource.SourceId;
                        ApplicationModel.ConfigurationItem = ConfigurationItem.Load(_zipController.Network, ApplicationModel.CreateSelectableItem);
                        ApplicationModel.Controller = _zipController;
                        ApplicationModel.DataSource = DataSource;

                        InitSubstituteManager(_zipController);

                        var imaViewSettings = ApplicationModel.ConfigurationItem.ViewSettings.IMAView;
                        if (imaViewSettings != null)
                        {
                            ApplicationModel.IMAFullNetworkModel.SetLayout(NetworkCanvasLayout.FromString(imaViewSettings.NetworkLayoutString));

                            ApplicationModel.IMAFullNetworkModel.UseBackgroundColor = imaViewSettings.UseNetworkBackgroundColor;
                            string bgImgPath = imaViewSettings.NetworkBackgroundImagePath;
                            if (!File.Exists(bgImgPath))
                            {
                                if (!string.IsNullOrEmpty(bgImgPath))
                                {
                                    Logger.LogFail($"IMAFullNetwork. Background image file does not exists {bgImgPath}");
                                }
                                ApplicationModel.IMAFullNetworkModel.BackgroundImagePath = null;
                            }
                            else
                            {
                                ApplicationModel.IMAFullNetworkModel.BackgroundImagePath = bgImgPath;
                            }
                            ApplicationModel.IMAFullNetworkModel.BackgroundColor = imaViewSettings.NetworkBackgroundColor;
                        }
                        ApplicationModel.SmartStartModel.IsMetadataEnabled = true;

                        _dtls1ClientTransportLayer.Listener = new Dtls1ClientTransportListener();
                        _dtls1ClientTransportLayer.ListenerSecond = new Dtls1ClientTransportListener();
                        _primaryUnsolicitedClients = new UnsolicitedDestinationHandler<IUnsolicitedClient>(_dtls1ClientTransportLayer.Listener, this);
                        _secondaryUnsolicitedClients = new UnsolicitedDestinationHandler<IUnsolicitedReceiveClient>(_dtls1ClientTransportLayer.ListenerSecond, this);

                        InitUnsolicitedDestination();
                        InitNodes(getNodesRes);
                        InitZipDsk();
                        StopCallbacksListeners();
                        StartCallbacksListeners();
                        SerialPortMonitor?.Open();
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo | NotifyProperty.NodesList);
                        return CommunicationStatuses.Done;
                    }
                }
            }
            _zipController.Disconnect();
            _zipController.Dispose();
            _zipController = null;
            OnDeviceInitFailed();
            return CommunicationStatuses.Failed;
        }

        public IUnsolicitedClient CreatePrimatyClient(string address, ushort portNo) => CreateUnsolicitedClient();

        public IUnsolicitedReceiveClient CreateSecondaryClient(string address, ushort portNo) => CreateUnsolicitedClient();

        private UnsolicitedClient CreateUnsolicitedClient()
        {
            ApplyCallbacks(out Func<byte[], byte[]> dskNeededCallback,
                out Func<IEnumerable<SecuritySchemes>, bool, KEXSetConfirmResult> kexSetConfirmCallback,
                out Action csaPinCallback);
            var unsolicitedClient = new UnsolicitedClient(_zipApplicationLayer.CreateZipController());
            unsolicitedClient.AddSupportedOperation(new NetworkManagementInclusionSupport(
                new byte[]
                {
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.NODE_ADD_KEYS_REPORT.ID,
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.NODE_ADD_DSK_REPORT.ID,
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.NODE_ADD_STATUS.ID,
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V4.SMART_START_JOIN_STARTED_REPORT.ID,
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V4.EXTENDED_NODE_ADD_STATUS.ID
                },
                dskNeededCallback, kexSetConfirmCallback, csaPinCallback, UnsolicitedControllerUpdateCallback));
            unsolicitedClient.AddSupportedOperation(new NetworkManagementProxySupport(
                new byte[]
                {
                    COMMAND_CLASS_NETWORK_MANAGEMENT_PROXY_V4.NODE_LIST_REPORT.ID
                },
                UnsolicitedControllerUpdateListCallback));
            return unsolicitedClient;
        }

        private object _unsolicitedCallbackLocker = new object();
        private void UnsolicitedControllerUpdateCallback(AddNodeResult result)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                lock (_unsolicitedCallbackLocker)
                {
                    bool needLifelineSetup = false;
                    needLifelineSetup = ApplicationModel.ConfigurationItem.Nodes.FirstOrDefault(p => p.Item == result.Node &&
                        p.Item.EndPointId == 0) == null ? true : false;
                    AddNodeToController(result);
                    if (needLifelineSetup)
                    {
                        bool setupResult = SetupLifeLineForNode(result.Node);
                    }
                }
            });
        }

        private void UnsolicitedControllerUpdateListCallback(GetNodeListResult result)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                lock (_unsolicitedCallbackLocker)
                {
                    foreach (var node in Devices.Keys.ToArray())
                    {
                        if (!result.Nodes.Contains(node))
                        {
                            Devices[node].Dispose();
                            Devices.Remove(node);
                            ApplicationModel.Invoke(() =>
                            {
                                ApplicationModel.ConfigurationItem.RemoveNode(node);
                                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            });
                        }
                    }
                }
            });
        }

        private void FinalizeUnsolicitedClients()
        {
            _primaryUnsolicitedClients?.Dispose();
            _secondaryUnsolicitedClients?.Dispose();
        }

        private void StartCallbacksListeners()
        {
            Func<byte[], byte[]> dskNeededCallback;
            Func<IEnumerable<SecuritySchemes>, bool, KEXSetConfirmResult> kexSetConfirmCallback;
            Action csaPinCallback;
            ApplyCallbacks(out dskNeededCallback, out kexSetConfirmCallback, out csaPinCallback);

            _networkManagementInclusionSupportToken = _zipController.SessionClient.ExecuteAsync(new NetworkManagementInclusionSupport(
                new byte[]
                {
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.NODE_ADD_KEYS_REPORT.ID,
                    COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.NODE_ADD_DSK_REPORT.ID
                },
                dskNeededCallback,
                kexSetConfirmCallback, csaPinCallback, UnsolicitedControllerUpdateCallback));
        }

        private void StopCallbacksListeners()
        {
            if (_networkManagementInclusionSupportToken != null && _zipController != null)
                _zipController.Cancel(_networkManagementInclusionSupportToken);
        }

        private void InitZipDsk()
        {
            var dskGetRes = _zipController.RequestData(null,
                new COMMAND_CLASS_NETWORK_MANAGEMENT_BASIC_V2.DSK_GET
                {
                    properties1 = new COMMAND_CLASS_NETWORK_MANAGEMENT_BASIC_V2.DSK_GET.Tproperties1
                    {
                        addMode = 1
                    }
                },
                new COMMAND_CLASS_NETWORK_MANAGEMENT_BASIC_V2.DSK_REPORT(),
                2000);
            if (dskGetRes && dskGetRes.ReceivedData != null && dskGetRes.ReceivedData.Length > 2)
            {
                var dskReport = (COMMAND_CLASS_NETWORK_MANAGEMENT_BASIC_V2.DSK_REPORT)dskGetRes.ReceivedData;
                if (dskReport.dsk.Length == 16)
                {
                    _zipController.DSK = dskReport.dsk;
                    _zipController.DSK = dskReport.dsk;
                }
            }
        }

        private void InitUnsolicitedDestination()
        {
            ApplicationModel.SelectLocalIpAddressDialog.SecondaryPort = ControllerSessionsContainer.Config.ControllerConfiguration.UnsolicitedPortNoSecondary;
            if (ControllerSessionsContainer.Config.ControllerConfiguration.IsUnsolicitedDestinationEnabled.Value)
            {
                var udc = new UnsolicitedDestinationConfigurator(_zipController);
                bool startListeners = udc.IsConfigured() ? true : udc.AutoConfig();
                if (startListeners)
                    RestartUnsolicitedListeners(udc.ConfiguredEndpoint.Item1, udc.ConfiguredEndpoint.Item2);
                else
                    Logger.LogFail($"Failed to config unsolicited destination");
            }
        }

        public void RestartUnsolicitedListeners(string localAddress, ushort primatyPortNo)
        {
            if (primatyPortNo == 0)
                return;

            _dtls1ClientTransportLayer.Listener.Stop();
            ApplicationModel.SelectLocalIpAddressDialog.IsListening =
                _dtls1ClientTransportLayer.Listener.Listen(new Dtls1StartListenParams { PskKey = _psk, IpAddress = localAddress, PortNo = primatyPortNo });
            if (ApplicationModel.SelectLocalIpAddressDialog.IsListening)
            {
                CreateUnsolicitedClient();
                ApplicationModel.SelectLocalIpAddressDialog.PrimaryAddress = localAddress;
                ApplicationModel.SelectLocalIpAddressDialog.PrimaryPort = primatyPortNo;
                if (ControllerSessionsContainer.Config.ControllerConfiguration.IsSecondaryUnsolicitedEnabled)
                    RestartSecondaryUnsolicitedListener();
            }
            else
            {
                Logger.LogFail($"Failed to start DTLSv1 listener on adress: {localAddress}, port: {primatyPortNo}");
            }
        }

        public void RestartSecondaryUnsolicitedListener()
        {
            StopSecondaryUnsolicitedListener();
            var secondaryPortNo = ApplicationModel.SelectLocalIpAddressDialog.SecondaryPort;
            if (secondaryPortNo > 0)
            {
                var listenParams = _dtls1ClientTransportLayer.Listener.ListenParams;
                listenParams.PortNo = secondaryPortNo;
                _dtls1ClientTransportLayer.ListenerSecond.Stop();
                ApplicationModel.SelectLocalIpAddressDialog.IsSecondaryOn = _dtls1ClientTransportLayer.ListenerSecond.Listen(listenParams);
                if (!ApplicationModel.SelectLocalIpAddressDialog.IsSecondaryOn)
                {
                    Logger.LogFail($"Failed to start DTLSv1 listener on adress: {listenParams.IpAddress}, port: {secondaryPortNo}");
                }
            }
            ApplicationModel.NotifyControllerChanged(NotifyProperty.UnsolicitedDestination, ApplicationModel.SelectLocalIpAddressDialog);
        }

        public void StopSecondaryUnsolicitedListener()
        {
            if (_dtls1ClientTransportLayer != null &&
                _dtls1ClientTransportLayer.ListenerSecond != null)
            {
                if (_dtls1ClientTransportLayer.ListenerSecond.IsListening)
                    _dtls1ClientTransportLayer.ListenerSecond.Stop();
                ApplicationModel.SelectLocalIpAddressDialog.IsSecondaryOn = _dtls1ClientTransportLayer.ListenerSecond.IsListening;
                if (_dtls1ClientTransportLayer.ListenerSecond.IsListening)
                {
                    Logger.LogFail($"Failed to stop secondary DTLSv1 listener");
                }
            }
            ApplicationModel.NotifyControllerChanged(NotifyProperty.UnsolicitedDestination, ApplicationModel.SelectLocalIpAddressDialog);
        }

        public void StopUnsolicitedListeners()
        {
            if (_dtls1ClientTransportLayer != null &&
                _dtls1ClientTransportLayer.Listener != null)
            {
                if (_dtls1ClientTransportLayer.Listener.IsListening)
                    _dtls1ClientTransportLayer.Listener.Stop();
                ApplicationModel.SelectLocalIpAddressDialog.IsListening = _dtls1ClientTransportLayer.Listener.IsListening;
                if (_dtls1ClientTransportLayer.Listener.IsListening)
                {
                    Logger.LogFail($"Failed to stop primary DTLSv1 listener");
                }
            }
            StopSecondaryUnsolicitedListener();
            ApplicationModel.NotifyControllerChanged(NotifyProperty.UnsolicitedDestination, ApplicationModel.SelectLocalIpAddressDialog);
        }

        public void Disconnect()
        {
            StopUnsolicitedListeners();
            _dtls1ClientTransportLayer?.Dispose();
            StopCallbacksListeners();
            //_applicationModel.NetworkManagement.StopToggleBasicGet();
            PollingService.PollingStop();
            SaveConfigurations();
            FinalizeUnsolicitedClients();
            if (_zipController != null)
            {
                _zipController.Disconnect();
                _zipController.Dispose();
                _zipController = null;
                SerialPortMonitor?.Close();
            }
            foreach (var item in Devices.Values)
            {
                item.Disconnect();
                item.Dispose();
            }
            Devices.Clear();
            ApplicationModel.Controller = null;
            CommandsFactory.CurrentSourceId = null;
            ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleSource, new { SourceId = ApplicationModel.DataSource?.SourceId, IsActive = false });
        }

        private void SaveConfigurations()
        {
            if (ApplicationModel.IMAFullNetworkModel.Layout != null)
            {
                ApplicationModel.ConfigurationItem.ViewSettings.IMAView.NetworkLayoutString = ApplicationModel.IMAFullNetworkModel.Layout.ToString();
            }

            ApplicationModel.SaveDialogSettings();
            ControllerSessionsContainer.Config.ControllerConfiguration.Save();
            ApplicationModel.ConfigurationItem?.Save();
        }

        public void Update(bool isDefaultCommandClasses)
        {
            ApplicationModel.IsNeedFirstReloadTopology = true;
            _zipController.GetId();
            var getVersionRes = _zipController.GetVersion();
            if (getVersionRes.FirmwareVersions.Count > 0)
            {
                _zipController.AppVersion = getVersionRes.FirmwareVersions[0];
            }
            var getNodesRes = _zipController.GetNodeList(10000);
            if (getNodesRes.IsBusyReceived)
            {
                Logger.LogFail($"Application Busy status: '{getNodesRes.BusyStatus}', wait {getNodesRes.BusyWaitTime} sec.");
                Thread.Sleep(getNodesRes.BusyWaitTime * 1000);
                getNodesRes = _zipController.GetNodeList(10000);
            }
            if (getNodesRes)
            {
                InitNodes(getNodesRes);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo | NotifyProperty.NodesList);
            }
            else
            {
                Logger.LogFail($"ZIP communication failed");
            }
        }

        #endregion

        private void InitNodes(GetNodeListResult nodeListResult)
        {
            foreach (var item in Devices.Values)
            {
                item.Disconnect();
                item.Dispose();
            }
            Devices.Clear();
            if (_zipController == null)
                throw new Exception("Controller isn't initialized");

            if (_zipController.IncludedNodes == null)
                return;

            foreach (var node in _zipController.IncludedNodes)
            {
                if (node != _zipController.Network.NodeTag)
                {
                    DeviceAdd(node);
                }
            }

            var configurationItem = ApplicationModel.ConfigurationItem;
            ApplicationModel.Invoke(() => configurationItem.FillNodes(_zipController.IncludedNodes));
            var ctrlDevice = _zipController as ZipController;
            if (ctrlDevice != null)
            {
                var addedNodes = configurationItem.Nodes.ToArray();
                foreach (var item in addedNodes)
                {
                    var node = item.Item;
                    var infoRes = ctrlDevice.GetCachedNodeInfo(node, 10000);
                    _zipController.Network.SetNodeInfo(node, infoRes.NodeInfo);
                    _zipController.Network.SetSecuritySchemes(node, infoRes.SecuritySchemes);
                    _zipController.Network.SetCommandClasses(node, infoRes.CommandClasses);
                    _zipController.Network.SetSecureCommandClasses(node, infoRes.SecureCommandClasses);
                    if (node.Id == _zipController.Id)
                    {
                        UpdateNetworkRole(nodeListResult);
                    }
                    ApplicationModel.Invoke(() =>
                    {
                        configurationItem.AddOrUpdateNode(node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                    });
                }
                configurationItem.RefreshNodes();
                var selfNode = configurationItem.Nodes.Where(x => x.Item.Id == _zipController.Id);
                if (selfNode.Any())
                {
                    ApplicationModel.SelectedNode = selfNode.First();
                }
                ApplicationModel.Controller = _zipController;
            }
        }

        private void UpdateNetworkRole(GetNodeListResult nodeListResult)
        {
            _zipController.NetworkRole = ControllerRoles.None;
            if (!nodeListResult)
                return;
            var secureCCs = _zipController.Network.GetSecureCommandClasses();
            if (secureCCs.Contains(COMMAND_CLASS_NETWORK_MANAGEMENT_INCLUSION_V3.ID))
            {
                if (secureCCs.Contains(COMMAND_CLASS_NODE_PROVISIONING.ID) && _zipController.Id == nodeListResult.ControllerId)
                    _zipController.NetworkRole = ControllerRoles.SIS;
                else if (_zipController.Id != nodeListResult.ControllerId)
                    _zipController.NetworkRole = ControllerRoles.Inclusion;
            }
            else if (_zipController.Id != nodeListResult.ControllerId)
                _zipController.NetworkRole = ControllerRoles.Secondary;
        }

        private SubstituteManager _substituteManager;
        private void InitSubstituteManager(IDevice device)
        {
            if (device == null)
                throw new Exception("device is not initialized");

            if (_substituteManager == null)
            {
                _substituteManager = new SubstituteManager(
                    (i, d) => SendDataSubstitutionCallback(device.Network.NodeTag, d),
                    (i, d, s) => ReceiveDataSubstitutionCallback(device.Network.NodeTag, d, s));
            }

            device.SessionClient.AddSubstituteManager(_substituteManager);
        }

        private bool SendDataSubstitutionCallback(NodeTag node, byte[] data)
        {
            bool ret = false;
            LogCommand(node, data, SecuritySchemes.NONE, false);
            return ret;
        }

        private void ReceiveDataSubstitutionCallback(NodeTag srcNode, byte[] data, bool isSubstituted)
        {
            LogCommand(srcNode, data, SecuritySchemes.NONE, true);
        }

        public byte[] EncapData(byte[] data, byte destinationEndPoint)
        {
            if (destinationEndPoint > 0)
            {
                var multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V3.MULTI_CHANNEL_CMD_ENCAP();
                multiChannelCmd.commandClass = data[0];
                multiChannelCmd.command = data[1];
                multiChannelCmd.parameter = new List<byte>();
                for (int i = 2; i < data.Length; i++)
                {
                    multiChannelCmd.parameter.Add(data[i]);
                }
                multiChannelCmd.properties1.res = 0;
                multiChannelCmd.properties1.sourceEndPoint = 0;
                multiChannelCmd.properties2.bitAddress = 0;
                multiChannelCmd.properties2.destinationEndPoint = destinationEndPoint;
                data = multiChannelCmd;
            }
            return data;
        }

        public CommandExecutionResult SetInifUserInput(byte[] uiid, byte[] uilr)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult GetInifUserInput(out byte[] uiid, out byte[] uilr)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult ProvisioningListSet(byte[] DSK, byte grantSchemes, byte nodeOptions, ProvisioningListItemData[] itemMetaData, ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            if (DSK != null)
            {
                var cmd = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_SET();
                cmd.dsk = DSK;
                cmd.properties1.dskLength = (byte)DSK.Length;

                foreach (var metaData in itemMetaData)
                {
                    var ext = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_SET.TVG1();
                    ext.value = metaData.Value;
                    ext.length = (byte)metaData.Value.Length;
                    ext.properties1.critical = (byte)(metaData.IsCritical ? 0x01 : 0x00);
                    ext.properties1.metaDataType = (byte)metaData.Type;
                    cmd.vg1.Add(ext);
                }

                if (SendData(new NodeTag(_zipController.Id), cmd, token))
                {
                    ProvisioningItem newItem = new ProvisioningItem();
                    newItem.Dsk = DSK;
                    newItem.State = PreKittingState.Pending;
                    newItem.Metadata = new ObservableCollection<ProvisioningItemExtension>();
                    foreach (var metaData in itemMetaData)
                    {
                        var extItem = new ProvisioningItemExtension()
                        {
                            IsElective = false,
                            Type = (ProvisioningItemExtensionTypes)metaData.Type,
                            Text = metaData.ToStringFromValue(),
                            IsCritical = metaData.IsCritical
                        };
                        newItem.Metadata.Add(extItem);
                    }
                    ApplicationModel.Invoke(() =>
                    {
                        var updStateTxt = "Added";
                        var exi = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault(x => x.Dsk.SequenceEqual(newItem.Dsk));
                        if (exi != null)
                        {
                            updStateTxt = "Modified";
                            exi.GrantSchemes = newItem.GrantSchemes;
                            exi.Metadata = newItem.Metadata;
                            exi.NodeOptions = newItem.NodeOptions;
                            exi.State = newItem.State;
                            exi.Comment = newItem.Comment;
                        }
                        else
                        {
                            ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Add(newItem);
                        }
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = "Provisioning List Set", Message = $"DSK {updStateTxt} to Provisioning List" });
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.RefreshProvisioningList);
                    });
                    ret = CommandExecutionResult.OK;
                }
            }
            else
            {
                token = null;
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListDelete(byte[] DSK, ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            if (DSK != null)
            {
                var cmd = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_DELETE();
                cmd.dsk = DSK;
                cmd.properties1.dskLength = (byte)DSK.Length;
                if (SendData(new NodeTag(_zipController.Id), cmd, token))
                {
                    var selectedItem = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault(x => x.Dsk != null && x.Dsk.SequenceEqual(DSK));
                    if (selectedItem != null)
                    {
                        ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Remove(selectedItem));
                    }
                    ret = CommandExecutionResult.OK;
                }
            }
            else
            {
                token = null;
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListClear(ActionToken token)
        {
            token = null;
            var ret = CommandExecutionResult.Failed;
            var dsks = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Select(x => x.Dsk).ToArray();
            foreach (var dsk in dsks)
            {
                ProvisioningListDelete(dsk, token);
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListGet()
        {
            var ret = CommandExecutionResult.Failed;
            const byte START_ITERATION = 0xFF;
            byte currentIter = START_ITERATION;
            ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Clear());

            while (currentIter > 0)
            {
                var cmd = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_LIST_ITERATION_GET() { remainingCounter = currentIter };
                byte[] rptData = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_LIST_ITERATION_REPORT();
                var result = RequestData(_zipController.Network.NodeTag, cmd, ref rptData, 3000);
                if (result)
                {
                    COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_LIST_ITERATION_REPORT report = rptData;
                    currentIter = report.remainingCount;
                    if (report.dsk != null && report.dsk.Count > 0)
                    {
                        var pi = new ProvisioningItem();
                        pi.Dsk = report.dsk.ToArray();
                        pi.State = PreKittingState.Pending;
                        if (report.vg1 != null && report.vg1.Count > 0)
                        {
                            pi.Metadata = new ObservableCollection<ProvisioningItemExtension>();
                            foreach (var item in report.vg1)
                            {
                                if (item.value != null && item.value.Count > 0)
                                {
                                    var type = (ProvisioningItemExtensionTypes)item.properties1.metaDataType;
                                    if (type != ProvisioningItemExtensionTypes.Status &&
                                        type != ProvisioningItemExtensionTypes.BootstrappingMode)
                                    {
                                        pi.Metadata.Add(new ProvisioningItemExtension()
                                        {
                                            Type = (ProvisioningItemExtensionTypes)item.properties1.metaDataType,
                                            IsElective = item.properties1.critical == 1 ? false : true,
                                            Text = Encoding.ASCII.GetString(item.value.ToArray())
                                        });
                                    }
                                    else if (type == ProvisioningItemExtensionTypes.Status)
                                    {
                                        pi.State = (PreKittingState)(item.value[0]);
                                    }
                                }
                            }
                        }
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Add(pi);
                        });

                        //_applicationModel.Invoke(() =>
                        //{
                        //    _applicationModel.NotifyPropertiesChanged("DSK");
                        //});
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListGet(byte[] DSK)
        {
            var ret = CommandExecutionResult.Failed;
            var cmd = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_GET();
            cmd.dsk = DSK;
            cmd.properties1.dskLength = (byte)DSK.Length;
            byte[] rptData = new COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_REPORT();
            var result = RequestData(_zipController.Network.NodeTag, cmd, ref rptData, 3000);
            if (result)
            {
                COMMAND_CLASS_NODE_PROVISIONING.NODE_PROVISIONING_REPORT report = rptData;
                var selectedItem = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault(x => x.Dsk != null && x.Dsk.SequenceEqual(DSK));
                if (selectedItem != null)
                {
                    ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Remove(selectedItem));
                }
                var pi = new ProvisioningItem();
                pi.Dsk = report.dsk.ToArray();
                pi.State = PreKittingState.Pending;
                if (report.vg1 != null && report.vg1.Count > 0)
                {
                    foreach (var item in report.vg1)
                    {
                        if (item.value != null && item.value.Count > 0)
                        {
                            var type = (ProvisioningItemExtensionTypes)item.properties1.metaDataType;
                            if (type != ProvisioningItemExtensionTypes.Status &&
                                type != ProvisioningItemExtensionTypes.BootstrappingMode)
                            {
                                pi.Metadata.Add(new ProvisioningItemExtension()
                                {
                                    Type = (ProvisioningItemExtensionTypes)item.properties1.metaDataType,
                                    IsElective = item.properties1.critical == 1 ? false : true,
                                    Text = Encoding.ASCII.GetString(item.value.ToArray())
                                });
                            }
                            else if (type == ProvisioningItemExtensionTypes.Status)
                            {
                                pi.State = (PreKittingState)(item.value[0]);
                            }
                        }
                    }
                }
                ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Add(pi));
            }
            return ret;
        }

        public CommandExecutionResult SetRFReceiveMode(bool isEnabled)
        {
            throw new NotImplementedException();
        }

        public CommandExecutionResult SetSleepMode(SleepModes mode, byte intEnable)
        {
            throw new NotImplementedException();
        }

        public void SendSerialApi(byte[] data, out ActionToken mToken)
        {
            mToken = null;
        }

        public ActionToken ExpectData(NodeTag name, byte[] rxData, int timeoutMs)
        {
            throw new NotImplementedException();
        }

        public void SaveSecurityKeys(bool isClearFile)
        {
            throw new NotImplementedException();
        }

        public LogAction ReportAction(string caption, ActionToken token, LogRawData logRawData = null)
        {
            return new LogAction(this, caption, null, token, logRawData);
        }

        public LogAction ReportAction(string caption, string busyText, ActionToken token, LogRawData logRawData = null)
        {
            return new LogAction(this, caption, busyText, token, logRawData);
        }
        public void LogCommand(NodeTag srcNode, byte[] data, SecuritySchemes scheme, bool isIncome)
        {
            if (srcNode.Id > 0 && data != null && data.Length > 0)
            {
                string cmdInfo = string.Empty;
                string commandName = string.Empty;
                string commandClassName = string.Empty;
                string logMsg = string.Empty;
                if (data[0] == 0)
                {
                    commandClassName = "NOP";
                    cmdInfo = " + " + Tools.GetHex(data.ToArray());
                }
                else
                {
                    CommandClassValue[] ccv;
                    ApplicationModel.ZWaveDefinition.ParseApplicationObject(data, out ccv);
                    if (data.Length > 2)
                    {
                        cmdInfo = " + " + Tools.GetHex(data.Skip(2).ToArray());
                    }
                    if (ccv.Length > 0)
                    {
                        var comDef = ccv[0].CommandValue.CommandDefinition;
                        if (comDef.Parent != null && comDef.Name != null)
                        {
                            commandClassName = comDef.Parent.Name;
                            commandName = string.Format("{0}({1:X2} {2:X2})", comDef.Name, comDef.Parent.KeyId, comDef.KeyId);
                        }
                        else
                        {
                            if (data.Length > 1)
                            {
                                commandName = "command class : 0x"
                                    + Tools.GetHex(data.Take(1).ToArray()) + " not found";
                            }
                        }

                    }
                }
                string schemeStr = string.Format(" [{0}]", scheme.ToString());
                if (scheme == SecuritySchemes.NONE)
                {
                    schemeStr = "";
                }
                logMsg = string.Format("{0}{1} {2}{3}", isIncome ? "Rx" : "Tx", schemeStr, commandName, cmdInfo);
                ApplicationModel.Invoke(() => { Logger.Log(logMsg); });
            }
        }

        public void LogError(Exception exception)
        {
            Logger.LogFail(exception.Message);
            ApplicationModel.ShowMessageDialog("Application run into error", exception.Message);
        }

        public void LogError(string errorMessage, params object[] args)
        {
            LogError(new Exception(string.Format(errorMessage, args)));
        }

        public void OnDeviceInitFailed()
        {
            LogError("The required Z-Wave controller device was not found.");
        }

        public void StopSmartListener()
        {

        }

        public void StartSmartListener()
        {

        }

        public void SetupSecurityManagerInfo(SecuritySettings securitySettings)
        {
            throw new NotImplementedException();
        }

        public ActionResult AddNodeWithCustomSettings(SetupNodeLifelineSettings setupNodeLifelineSettings, out ActionToken token)
        {
            throw new NotImplementedException();
        }

        public void SetControllerResponseDelayMs(int delayMs)
        {
            throw new NotImplementedException();
        }

        public void LoadMpan()
        {
            var mpanTableConfigurationModel = ApplicationModel.MpanTableConfigurationDialog;
            var mpanTable = SecurityManager.SecurityManagerInfo.MpanTable;
            var records = mpanTable.GetExistingRecords();

            ApplicationModel.Invoke(() =>
            {
                mpanTableConfigurationModel.ClearMpanTable();

                foreach (var record in records)
                {
                    mpanTableConfigurationModel.AddMpanItem(record);
                }
                ApplicationModel.NotifyControllerChanged(NotifyProperty.MpanTable);
            });
        }

        public string ConfigurationNameGet(NodeTag node, byte[] parameterNumber, ActionToken _token)
        {
            var name = new StringBuilder();
            var getNameCmd = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_NAME_GET();
            byte[] rptData = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_NAME_REPORT();
            getNameCmd.parameterNumber = parameterNumber;
            var configurationNameReport = RequestData(node,
                getNameCmd, ref rptData, 10000, _token);
            if (configurationNameReport)
            {
                COMMAND_CLASS_CONFIGURATION_V3.CONFIGURATION_NAME_REPORT getNameRpt = rptData;
                name.Append(Encoding.UTF8.GetString(getNameRpt.name.ToArray()));
                for (int i = getNameRpt.reportsToFollow - 1; i >= 0; i--)
                {
                    var expRes = ExpectData(node, rptData, 500, out _token);
                    if (expRes)
                    {
                        getNameRpt = expRes.ReceivedData;
                        name.Append(Encoding.UTF8.GetString(getNameRpt.name.ToArray()));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return name.ToString();
        }

        public string ConfigurationInfoGet(NodeTag node, byte[] parameterNumber, ActionToken _token)
        {
            var info = new StringBuilder();
            var getInfoCmd = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_INFO_GET();
            byte[] rptData = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_INFO_REPORT();
            getInfoCmd.parameterNumber = parameterNumber;

            var configurationInfoReport = RequestData(node,
                getInfoCmd, ref rptData, 10000, _token);
            if (configurationInfoReport)
            {
                COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_INFO_REPORT getInfoRpt = rptData;
                info.Append(Encoding.UTF8.GetString(getInfoRpt.info.ToArray()));
                for (int i = getInfoRpt.reportsToFollow - 1; i >= 0; i--)
                {
                    var expRes = ExpectData(node, rptData, 500, out _token);
                    if (expRes)
                    {
                        getInfoRpt = expRes.ReceivedData;
                        info.Append(Encoding.UTF8.GetString(getInfoRpt.info.ToArray()));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return info.ToString();
        }

        public PayloadItem GetPayloadItem(ICommandClassesModel model)
        {
            var name = "Unknown - no cmd";
            var classId = model.Payload[0];
            byte commId = 0;
            byte ccVersion = 0;
            if (model.SecureType == SecureType.SerialApi)
            {
                name = "Serial API";
            }
            else
            {
                var commandClass = ApplicationModel.ZWaveDefinition.FindCommandClasses(model.Payload[0]);
                Command command = null;
                if (commandClass != null && commandClass.Count > 0 && model.Payload.Length > 1)
                {
                    command = ApplicationModel.ZWaveDefinition.FindCommand(commandClass.OrderByDescending(x => x.Version).First(), model.Payload[1]);
                }
                if (command != null)
                {
                    name = command.Text;
                    classId = command.Parent.KeyId;
                    commId = command.KeyId;
                    ccVersion = commandClass.Max(x => x.Version);
                }
            }
            var ret = new PayloadItem
            {
                Payload = model.Payload,
                CommandName = name,
                ClassId = classId,
                CommandId = commId,
                Version = ccVersion,
                //Send Data options:
                IsCrc16Enabled = model.IsCrc16Enabled,
                IsSuppressMulticastFollowUp = model.IsSuppressMulticastFollowUp,
                IsForceMulticastEnabled = model.IsForceMulticastEnabled,
                IsSupervisionGetEnabled = model.IsSupervisionGetEnabled,
                SupervisionSessionId = model.SupervisionSessionId,
                IsSupervisionGetStatusUpdatesEnabled = model.IsSupervisionGetStatusUpdatesEnabled,
                IsAutoIncSupervisionSessionId = model.IsAutoIncSupervisionSessionId,
                IsMultiChannelEnabled = model.IsMultiChannelEnabled,
                IsBitAddress = model.IsBitAddress,
                SecureType = model.SecureType,
            };
            return ret;
        }
    }
}
