/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Utils;
using ZWave;
using ZWave.BasicApplication;
using ZWave.BasicApplication.CommandClasses;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Operations;
using ZWave.BasicApplication.Tasks;
using ZWave.CommandClasses;
using ZWave.Configuration;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;
using ZWaveController.Enums;
using System.Threading.Tasks;
using ZWave.BasicApplication.TransportService;
using ZWave.BasicApplication.Security;
using ZWave.Security;
using ZWave.Layers.Session;
using ZWave.Layers.Transport;
using ZWave.Security.S2;
using ZWaveController.Commands;
using Utils.UI.Interfaces;
using ZWaveController.Services;
using ZWaveController.Commands.Basic.Misc;
using Utils.UI.Logging;
using ZWaveController.Configuration;
using System.Collections;
using ZWaveController.Interfaces;
using System.Runtime.CompilerServices;
using ZWave.Layers;
#if !NETCOREAPP
//using ZWaveController.Properties;
#endif

[assembly: InternalsVisibleTo("ZWaveControllerUI")]
[assembly: InternalsVisibleTo("ZWaveControllerTests")]

namespace ZWaveController.Models
{
    public class BasicControllerSession : IControllerSession
    {
        public int ZWAVEPLUS_WAKE_UP_INTERVAL = 5;
        private const int WAIT_AFTER_ADD_REMOVE_NODE_TIMEOUT = 300;
        private const int SMART_START_STATE_CHANGED_TIMEOUT = 500;

        public IApplicationModel ApplicationModel { get; set; }

        public string UserSessionId { get; set; }
        private ActionToken _listenDataToken;
        private ActionToken _versionCCGetResponseToken;
        private ActionToken _versionGetResponseToken;
        private ActionToken _versionCapabilitiesGetResponseToken;
        private ActionToken _basicSupportToken;
        private ActionToken _serialApiStartedToken;
        private ActionToken _powerLevelSupportToken;
        private ActionToken _manufacturerSpecificResponseToken;
        private ActionToken _wakeUpResponseToken;
        private ActionToken _deviceResetLocallyResponseToken;
        private ActionToken _configurationSupportToken;
        private ActionToken _removeFailedToken;
        private ActionToken _ZWavePlusInfoGetResponseToken;
        private ActionToken _associationSupportToken;
        private ActionToken _associationGroupInfoSupportToken;
        private ActionToken _firmwareUpdateMdSupportToken;
        private ActionToken _multiChannelSupportToken;
        private ActionToken _multiChannelAssociationSupportToken;
        private ActionToken _wakeupSupportToken;
        private ActionToken _timeSupportToken;
        private ActionToken _timeParametersSupportToken;

        private byte[] _securedCommandClassesVirtual = new byte[]
        {
            COMMAND_CLASS_BASIC.ID,
            COMMAND_CLASS_VERSION.ID,
            COMMAND_CLASS_MANUFACTURER_SPECIFIC.ID
        };
        public ReadOnlyCollection<byte> SECURED_COMMAND_CLASSES_VIRTUAL => Array.AsReadOnly(_securedCommandClassesVirtual);

        public IDataSource DataSource { get; set; }
        ISessionLayer _sessionLayer;
        internal /*private*/ Device _device;
        public IDevice Controller => _device as IDevice;
        public ILogService Logger { get; set; }
        public ISenderHistoryService SenderHistoryService { get; set; }
        public IPredefinedPayloadsService PredefinedPayloadsService { get; set; }
        public SecurityManager SecurityManager { get; set; }
        public SupervisionManager SupervisionManager { get; private set; }
        public TransportServiceManager TransportServiceManager { get; set; }
        public ISerialPortMonitor SerialPortMonitor { get; private set; }
        public IPollingService PollingService { get; private set; }
        public IERTTService ERTTService { get; private set; }
        public INodeInformationService NodeInformationService { get; private set; }
        public IWakeUpNodesService WakeUpNodesService { get; private set; }
        public IJammingDetectionService JammingDetectionService { get; private set; }
        public IMAFullNetwork IMAFullNetwork { get; set; }
        public bool IsBridgeControllerLibrary => _device != null && _device.Library == Libraries.ControllerBridgeLib;
        public bool IsEndDeviceLibrary => _device != null && _device.IsEndDeviceApi;

        public BasicControllerSession(IApplicationModel applicationModel)
        {
            ApplicationModel = applicationModel;
            WakeUpNodesService = new WakeUpNodesService(SendWakeUpNoMore, SendWakeupSetInterval, ApplicationModel.Dispatcher, ApplicationModel.CommandQueueCollection,
                ApplicationModel.CreateSelectableItem, LogWarning, LogOk);
#if !NETCOREAPP
            SerialPortMonitor = new SerialPortMonitor(this);
#endif
            PollingService = new PollingService(this);
            ERTTService = new ERTTService(this);
            NodeInformationService = new NodeInformationService(ApplicationModel);
            IMAFullNetwork = new IMAFullNetwork();
            JammingDetectionService = new JammingDetectionService(this);
            _sessionLayer = new SessionLayer();
        }

        #region IControllerSession Members

        public virtual CommunicationStatuses Connect(IDataSource dataSource)
        {
            DataSource = dataSource ?? throw new ArgumentNullException("dataSource");
            var appLayer = ControllerSessionsContainer.ControllerSessionCreator.CreateBasicApplicationLayer(_sessionLayer, dataSource);
            appLayer.TransportLayer.DataTransmitted += ApplicationModel.TraceCapture.SaveTransmittedData;

            _device = appLayer?.CreateController(false);
            if (_device != null && _device.Connect(DataSource) == CommunicationStatuses.Done)
            {
                ApplicationModel.ClearCommandQueueCollection();
                VersionResult versionRes = _device.GetVersion(false, 500);
                if (!versionRes)
                {
                    versionRes = _device.GetVersion(true, 500);
                }
                if (versionRes)
                {
                    if (versionRes.Library == Libraries.ControllerBridgeLib)
                    {
                        _device = appLayer.CreateBridgeController(_device, false);
                        _device.Library = versionRes.Library;
                        _device.Version = versionRes.Version;
                    }
                    else if (versionRes.Library == Libraries.EndDeviceLib || versionRes.Library == Libraries.EndDeviceSysTestLib)
                    {
                        _device = appLayer.CreateEndDevice(_device, false);
                        _device.Library = versionRes.Library;
                        _device.Version = versionRes.Version;
                        _device.DSK = _device.GetSecurityS2PublicDSK().DSK;
                    }
                    _device.SerialApiSetNodeIdBaseType(2);
                    _device.GetPRK();
                    _device.SerialApiGetInitData();
                    _device.SerialApiGetCapabilities();
                    if (_device.Library == Libraries.EndDeviceSysTestLib ||
                        _device.Library == Libraries.EndDeviceLib ||
                        _device.Library == Libraries.ControllerStaticLib ||
                        _device.Library == Libraries.ControllerPortableLib ||
                        _device.Library == Libraries.ControllerBridgeLib)
                    {
                        _device.SerialApiGetLRNodes();
                        GetControllerCapabilitiesResult controllerCap = null;
                        if (!_device.IsEndDeviceApi)
                        {
                            controllerCap = (_device as Controller).GetControllerCapabilities();
                            if (controllerCap)
                            {
                                (_device as Controller).GetSucNodeId();
                            }
                        }
                        if ((_device.IsEndDeviceApi || controllerCap) && _device.MemoryGetId())
                        {
                            // Get Hardware Version from NVR (-10 offset - hided lockbits)
                            const byte HW_ADDRESS = 0x70;
                            var NVRGetValueResult = _device.NVRGetValue(HW_ADDRESS, 0x01);
                            if (NVRGetValueResult)
                            {
                                _device.HardwareVersion = NVRGetValueResult.NVRValue[0];
                            }
                            if (ApplicationModel.TraceCapture.TraceCaptureSettingsModel.IsWatchdogEnabled)
                            {
                                _device.WatchDogStart();
                                Logger.LogWarning("Watchdog started.");
                            }
                            CommandsFactory.CurrentSourceId = DataSource.SourceId;
                            ApplicationModel.DataSource = DataSource;
                            LoadConfigurationItem();
                            ApplicationModel.Controller = _device;
                            InitNodes(false);

                            LoadIMAFullNetworkConfiguration();
                            InitSubstituteManagers();
                            ApplicationModel.FirmwareUpdateModel.UseFirmwareDataTruncated = ControllerSessionsContainer.Config.ControllerConfiguration.UseFirmwareDataTruncated;

                            RestartSupportTasks();
                            StopListener();
                            StartListener();
                            StopSmartListener();
                            StartSmartListener();

                            SerialPortMonitor?.Open();
                            ApplicationModel.IsNeedFirstReloadTopology = true;

                            ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo);
                            _controllerUpdateActionToken = _device.HandleControllerUpdate(ControllerUpdateCallback);
                            if (_device.SupportedSerialApiCommands != null)
                            {
                                ApplicationModel.ERTTModel.IsTxControlledByModuleEnabled = Enumerable.Contains(_device.SupportedSerialApiCommands, (byte)CommandTypes.CmdSerialApiTest);
                                if (!ApplicationModel.ERTTModel.IsTxControlledByModuleEnabled)
                                    ApplicationModel.ERTTModel.IsTxControlledByModule = false;
                            }
                            ApplicationModel.SmartStartModel.IsMetadataEnabled = false;
                            if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetDcdcMode))
                            {
                                var res = _device.GetDcdcMode();
                                ApplicationModel.TransmitSettingsModel.DcdcMode = res.DcdcMode;
                            }
                            if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSerialApiSetup))
                            {
                                var res = _device.GetSupportedSetupSubCommands();
                                if (res)
                                {
                                    _device.ExtendedSetupSupportedSubCommands = res.ExtendedSetupSupportedSubCommands;
                                }
                                if (GetDefaultTxPowerLevel(out short normal, out short measured) == CommandExecutionResult.OK)
                                {
                                    ApplicationModel.TransmitSettingsModel.NormalTxPower = normal;
                                    ApplicationModel.TransmitSettingsModel.Measured0dBmPower = measured;
                                }
                                if (GetMaxLrTxPower(out short maxLrTxPowerMode) == CommandExecutionResult.OK)
                                {
                                    ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode = (MaxLrTxPowerModes)maxLrTxPowerMode;
                                }
                                if (GetRfRegion(out RfRegions rfRegion) == CommandExecutionResult.OK)
                                {
                                    ApplicationModel.TransmitSettingsModel.RfRegion = rfRegion;
                                    ApplicationModel.TransmitSettingsModel.IsRfRegionLR = (rfRegion == RfRegions.US_LR || rfRegion == RfRegions.EU_LR);
                                }
                                var maxPayloadSizeRes = _device.GetMaxPayloadSize();
                                Logger.Log($"Max Payload Size = {_device.Network.TransportServiceMaxSegmentSize}");
                                var maxLRPayloadSizeRes = _device.GetMaxLRPayloadSize();
                                Logger.Log($"Max LR Payload Size = {_device.Network.TransportServiceMaxLRSegmentSize}");
                            }
                            if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetLRChannel) &&
                                ApplicationModel.TransmitSettingsModel.IsRfRegionLR)
                            {
                                var lrChRes = _device.GetLRChannel();
                                if (lrChRes)
                                {
                                    ApplicationModel.TransmitSettingsModel.LRChannel = lrChRes.Channel;
                                }
                            }
                            if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetRadioPTI))
                            {
                                var isRadioPTIres = _device.IsRadioPTI();
                                if (isRadioPTIres)
                                {
                                    ApplicationModel.TransmitSettingsModel.IsRadioPTIEnabled = isRadioPTIres.IsEnabled;
                                }
                            }
                            return CommunicationStatuses.Done;
                        }
                    }
                }
            }
            _device.Disconnect();
            _device.Dispose();
            _device = null;
            OnDeviceInitFailed();
            return CommunicationStatuses.Failed;
        }

        public virtual void Disconnect()
        {
            //_applicationModel.NetworkManagement.StopToggleBasicGet();
            ERTTService.Stop();
            PollingService.PollingStop();
            JammingDetectionService.Stop();
            SaveConfigurations();
            if (_device != null)
            {
                SerialPortMonitor?.Close();
                _device.Disconnect();
                _device.Dispose();
                _device = null;
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.ConfigurationItem.FillNodes(null);
                    ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Clear();
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.ToggleSource, new { SourceId = ApplicationModel.DataSource.SourceId, IsActive = false });
                });
                ApplicationModel.Controller = null;
                CommandsFactory.CurrentSourceId = null;
            }
        }

        public ActionResult SendData(NodeTag node, byte[] txData, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false)
        {
            SubstituteSettings substituteSettings = new SubstituteSettings();
            substituteSettings.S0MaxBytesPerFrameSize = maxBytesPerFrame;
            substituteSettings.SubstituteFlags = substituteFlag;
            return SendDataInternal(node, txData, substituteSettings, ControllerSessionsContainer.Config.TxOptions, token, isMultiChanelProcessed);
        }

        public ActionResult SendData(NodeTag[] nodes, byte[] txData, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false)
        {
            if (txData != null && txData.Length > 0 && nodes != null && nodes.Length > 0)
            {
                var command = ApplicationModel.ZWaveDefinition.ParseApplicationStringName(txData);
                var caption = "Send " + command + " to node(s) " + nodes.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y);
                var isEndpointsDestination = nodes.Any(y => y.EndPointId > 0) && nodes.Select(x => x.Id).Distinct().Count() == 1;
                var isEndpointsBitAddress = isEndpointsDestination && nodes.Max(x => x.EndPointId) < 8;
                if (isEndpointsBitAddress)
                {
                    var endpointsBitmask = new BitArray8();
                    foreach (var endpoint in nodes)
                    {
                        if (endpoint.EndPointId > 0 && endpoint.EndPointId < 8)
                        {
                            endpointsBitmask[endpoint.EndPointId - 1] = true;
                        }
                    }
                    return SendData(new NodeTag(nodes[0].Id, (byte)endpointsBitmask, true), txData, maxBytesPerFrame, substituteFlag, token, isMultiChanelProcessed);
                }
                else
                {
                    token = null;
                    SubstituteSettings substituteSettings = new SubstituteSettings();
                    substituteSettings.S0MaxBytesPerFrameSize = maxBytesPerFrame;
                    substituteSettings.SubstituteFlags = substituteFlag;
                    var txOptions = ControllerSessionsContainer.Config.TxOptions;
                    if (substituteSettings.SubstituteFlags.HasFlag(SubstituteFlags.DenyFollowup))
                    {
                        txOptions = ControllerSessionsContainer.Config.TxOptions & (~TransmitOptions.TransmitOptionAcknowledge);
                    }
                    using (var logAction = ReportAction(caption, token))
                    {
                        ActionResult ret;
                        if (_device is BridgeController)
                        {
                            ret = ((BridgeController)_device).SendDataMultiBridge(_device.Network.NodeTag, nodes, txData, txOptions, substituteSettings, out token);
                        }
                        else
                        {
                            ret = _device.SendDataMulti(nodes, txData, txOptions, substituteSettings, out token);
                        }
                        logAction.Token = token;
                        return ret;
                    }
                }
            }
            else
            {
                token = null;
                return null;
            }
        }

        public ActionResult SendData(NodeTag node, byte[] txData, ActionToken token)
        {
            return SendDataInternal(node, txData, null, ControllerSessionsContainer.Config.TxOptions, token, false);
        }

        public ActionResult SendData(NodeTag node, byte[] txData, TransmitOptions txOptions, ActionToken token)
        {
            return SendDataInternal(node, txData, null, txOptions, token, false);
        }

        private ActionResult SendDataInternal(NodeTag node, byte[] txData, SubstituteSettings substituteSettings, TransmitOptions txOptions, ActionToken token, bool isMultiChanelProcessed)
        {
            if (txData != null && txData.Length > 0)
            {
                var command = ApplicationModel.ZWaveDefinition.ParseApplicationStringName(txData);
                var caption = "Send " + command + " to node " + node;
                if (!node.IsBitAddress && node.Id != 0xFF && _device.Network.GetCommandClasses(node) == null && !_device.IsEndDeviceApi)
                {
                    RequestNodeInfo(node, token);
                }
                if (node.EndPointId > 0 && !isMultiChanelProcessed)
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();

                    multiChannelCmd.commandClass = txData[0];
                    multiChannelCmd.command = txData[1];
                    multiChannelCmd.parameter = new List<byte>();
                    for (int i = 2; i < txData.Length; i++)
                    {
                        multiChannelCmd.parameter.Add(txData[i]);
                    }
                    multiChannelCmd.properties1.res = 0;
                    multiChannelCmd.properties1.sourceEndPoint = 0;
                    multiChannelCmd.properties2.bitAddress = (byte)(node.IsBitAddress ? 1 : 0);
                    multiChannelCmd.properties2.destinationEndPoint = node.EndPointId;
                    txData = multiChannelCmd;
                }
                using (var logAction = ReportAction(caption, null, new LogRawData { RawData = txData, SourceId = node.Id }))
                {
                    var ret = _device.SendData(node, txData, txOptions, substituteSettings, out token);
                    logAction.State = ret.State;


                    if (ret.TransmitStatus == TransmitStatuses.CompleteOk)
                    {
                        if (txData[0] == COMMAND_CLASS_WAKE_UP.ID && txData[1] == COMMAND_CLASS_WAKE_UP.WAKE_UP_INTERVAL_SET.ID)
                        {
                            var interval = ((COMMAND_CLASS_WAKE_UP.WAKE_UP_INTERVAL_SET)txData).seconds;
                            WakeUpNodesService.WakeUpNodeHealthStatuses.AddOrUpdate(node, new WakeUpMonitorContainer { WakeUpInterval = (int)Tools.ByteArrayToUInt32(interval) },
                            (nodeKey, container) =>
                            {
                                container.WakeUpInterval = (int)Tools.ByteArrayToUInt32(interval);
                                container.LastReceivedTimestamp = DateTime.Now;
                                container.IsAlive = true;
                                return container;
                            });
                        }
                    }
                    return ret;
                }
            }
            else
            {
                token = null;
                return null;
            }
        }

        public ActionToken ExpectData(NodeTag node, byte[] rxData, int timeoutMs)
        {
            if (node.EndPointId > 0)
            {
                COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                //rxData
                multiChannelCmd.commandClass = rxData[0];
                multiChannelCmd.command = rxData[1];
                multiChannelCmd.parameter = new List<byte>();
                for (int i = 2; i < rxData.Length; i++)
                {
                    multiChannelCmd.parameter.Add(rxData[i]);
                }
                multiChannelCmd.properties1.res = 0;
                multiChannelCmd.properties1.sourceEndPoint = node.EndPointId;
                multiChannelCmd.properties2.bitAddress = 0;
                multiChannelCmd.properties2.destinationEndPoint = 0;
                rxData = multiChannelCmd;
            }
            var expectToken = _device.ExpectData(node, rxData, timeoutMs, null);
            return expectToken;
        }

        public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs, ActionToken token)
        {
            return RequestData(node, txData, ref rxData, timeoutMs, null, token);
        }

        public ActionResult RequestData(NodeTag node, byte[] txData, ref byte[] rxData, int timeoutMs, Action sendDataSubstituteCallback, ActionToken token)
        {
            if (_device.Network.GetCommandClasses(node) == null && !_device.IsEndDeviceApi)
            {
                RequestNodeInfo(node, token);
            }
            if (node.EndPointId > 0)
            {
                COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                //txData
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

                //rxData
                multiChannelCmd.commandClass = rxData[0];
                multiChannelCmd.command = rxData[1];
                multiChannelCmd.parameter = new List<byte>();
                for (int i = 2; i < rxData.Length; i++)
                {
                    multiChannelCmd.parameter.Add(rxData[i]);
                }
                multiChannelCmd.properties1.res = 0;
                multiChannelCmd.properties1.sourceEndPoint = node.EndPointId;
                multiChannelCmd.properties2.bitAddress = 0;
                multiChannelCmd.properties2.destinationEndPoint = 0;
                rxData = multiChannelCmd;
            }
            RequestDataOperation operation = new RequestDataOperation(_device.Network, NodeTag.Empty, node, txData, ControllerSessionsContainer.Config.TxOptions, rxData, 2, timeoutMs);
            operation.SendDataSubstituteCallback = sendDataSubstituteCallback;
            token = _device.ExecuteAsync(operation, null);
            var res = (RequestDataResult)token.WaitCompletedSignal();
            rxData = res.Command;
            return res;
        }

        public CommandExecutionResult FirmwareUpdateOTAActivate(NodeTag node, ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("OTA Firmware Activate started.");
            Logger.Log("OTA Firmware Activate started.");

            var fwuActivationSetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_ACTIVATION_SET();
            fwuActivationSetData.manufacturerId = ApplicationModel.FirmwareUpdateModel.ManufacturerID;
            fwuActivationSetData.firmwareId = ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId;
            fwuActivationSetData.checksum = ApplicationModel.FirmwareUpdateModel.FirmwareChecksum;
            fwuActivationSetData.firmwareTarget = (byte)ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index;
            fwuActivationSetData.hardwareVersion = (byte)ApplicationModel.FirmwareUpdateModel.HardwareVersion;

            var fwuActivationReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_ACTIVATION_STATUS_REPORT();

            // Start Activate.
            RequestDataResult res = _device.RequestData(node, fwuActivationSetData, ControllerSessionsContainer.Config.TxOptions, fwuActivationReportData, 60000, out token);
            if (res && res.Command != null)
            {
                fwuActivationReportData = res.Command;

                var msg = string.Empty;
                var statusHexCode = fwuActivationReportData.firmwareUpdateStatus.ToString("X2");
                if (fwuActivationReportData.firmwareUpdateStatus == 0xFF)
                {
                    msg = $"Received status (0x{statusHexCode}): Firmware update completed successfully";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
                else if (fwuActivationReportData.firmwareUpdateStatus == 0x00)
                {
                    msg = $"Received status (0x{statusHexCode}): Invalid combination";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
                else if (fwuActivationReportData.firmwareUpdateStatus == 0x01)
                {
                    msg = $"Received status (0x{statusHexCode}): Error activating the firmware";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);
                }
            }
            return ret;
        }

        public CommandExecutionResult FirmwareUpdateOTADownload(NodeTag node, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("OTA Firmware Download started.");
            Logger.Log("OTA Firmware Download started.");

            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_GET getCmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_GET();
            getCmd.manufacturerId = ApplicationModel.FirmwareUpdateModel.ManufacturerID;
            getCmd.firmwareId = ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.FirmwareId;
            getCmd.firmwareTarget = (byte)ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget.Index;
            getCmd.fragmentSize = Tools.GetBytes((ushort)ApplicationModel.FirmwareUpdateModel.FragmentSize);
            getCmd.hardwareVersion = (byte)ApplicationModel.FirmwareUpdateModel.HardwareVersion;

            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_REPORT reportCmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_PREPARE_REPORT();

            RequestDataResult res = _device.RequestData(node, getCmd, ControllerSessionsContainer.Config.TxOptions, reportCmd, 30000, out token);
            if (res && res.Command != null)
            {
                reportCmd = res.Command;

                string msg = "";
                if (reportCmd.status == 0xFF)
                {
                    msg = $"Received status (0x{reportCmd.status.ToString("X2")}): Valid combination";
                    ApplicationModel.FirmwareUpdateModel.UpdateResultStatus = msg;
                    Logger.Log(msg);

                    ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = null;
                    _isDownloadCompleted = false;
                    _isDownloadCancelled = false;
                    int iteration = 0;
                    while (!_isDownloadCompleted && !_isDownloadCancelled)
                    {
                        var get2Cmd = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET();
                        get2Cmd.numberOfReports = ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports;
                        int reportNumber = iteration * ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports + 1;
                        get2Cmd.properties1.reportNumber1 = (byte)(reportNumber >> 8);
                        get2Cmd.reportNumber2 = (byte)reportNumber;
                        _expectPackets = new BitArray(ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports);
                        _responseDownloadToken = _device.ResponseMultiData(OnDownloadReceived,
                            ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT());
                        token = _responseDownloadToken;
                        ActionToken sendDataToken = null;
                        var res2 = SendData(node, get2Cmd, sendDataToken);
                        if (res2)
                        {
                            _responseDownloadToken.WaitCompletedSignal();
                            if (_isDownloadCompleted && _downloadFirmware != null)
                            {
                                List<byte> tmp = new List<byte>();
                                foreach (var item in _downloadFirmware)
                                {
                                    tmp.AddRange(item.Value);
                                }
                                ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = tmp.ToArray();
                            }
                        }
                        else
                        {
                            _device.Cancel(_responseDownloadToken);
                            _responseDownloadToken.WaitCompletedSignal();
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
            return ret;
        }

        private ActionToken _responseDownloadToken = null;
        private SortedList<int, byte[]> _downloadFirmware = new SortedList<int, byte[]>();
        private BitArray _expectPackets = null;
        private bool _isDownloadCompleted = false;
        private bool _isDownloadCancelled = false;
        private List<byte[]> OnDownloadReceived(ReceiveStatuses options, NodeTag destNode, NodeTag srcNode, byte[] payload)
        {
            List<byte[]> ret = null;
            if (payload != null)
            {
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT cmd = payload;
                int reportNumber = (cmd.properties1.reportNumber1 << 8) + cmd.reportNumber2;
                _expectPackets[(reportNumber - 1) % ApplicationModel.FirmwareUpdateModel.DownloadNumberOfReports] = true;
                bool isAllReceived = true;
                for (int i = 0; i < _expectPackets.Count; i++)
                {
                    if (!_expectPackets[i])
                    {
                        isAllReceived = false;
                        break;
                    }
                }
                if (cmd.data != null)
                {
                    if (_downloadFirmware.ContainsKey(reportNumber))
                    {
                        _downloadFirmware[reportNumber] = cmd.data.ToArray();
                    }
                    else
                    {
                        _downloadFirmware.Add(reportNumber, cmd.data.ToArray());
                    }
                }

                if (cmd.properties1.last > 0)
                {
                    _isDownloadCompleted = true;
                    _device.Cancel(_responseDownloadToken);
                }
                else if (isAllReceived)
                {
                    _device.Cancel(_responseDownloadToken);
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
            //Register ResponseData
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = _device.ResponseMultiDataEx((a, b, c, x) =>
            {
                return FwuMultiResponseHandler(fwData, x, false, out fwuMDGetData);
            }, ControllerSessionsContainer.Config.TxOptions, fwuMDGetData);

            //Start Update
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        public byte FirmwareUpdateV2(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, out ActionToken token)
        {
            var fwuMDRequestGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REQUEST_GET();
            fwuMDRequestGetData.checksum = fwChecksum;
            fwuMDRequestGetData.firmwareId = fwId;
            fwuMDRequestGetData.manufacturerId = mfId;
            var fwuMDRequestReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REQUEST_REPORT();
            //Register ResponseData
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = _device.ResponseMultiDataEx((a, b, c, x) =>
            {
                return FwuMultiResponseHandler(fwData, x, true, out fwuMDGetData);
            }, ControllerSessionsContainer.Config.TxOptions, fwuMDGetData);

            //Start Update
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
            //Register ResponseData
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = _device.ResponseMultiDataEx((a, b, c, x) =>
            {
                return FwuMultiResponseHandler(fwData, x, true, out fwuMDGetData);
            }, ControllerSessionsContainer.Config.TxOptions, fwuMDGetData);

            //Start Update
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
            //Register ResponseData
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node

            token = _device.ResponseMultiDataEx((a, b, c, x) =>
            {
                return FwuMultiResponseHandler(fwData, x, true, out fwuMDGetData);
            }, ControllerSessionsContainer.Config.TxOptions, fwuMDGetData);

            //Start Update
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
            //Register ResponseData
            var fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET(); //response from the node
            bool isStopOnNak = ApplicationModel?.FirmwareUpdateModel?.IsStopOnNak ?? false;
            byte reportsLimit = 0;
            if (ApplicationModel?.FirmwareUpdateModel != null && ApplicationModel.FirmwareUpdateModel.IsReportsLimited)
            {
                reportsLimit = ApplicationModel.FirmwareUpdateModel.ReportsLimit;
            }
            token = _device.ResponseMultiDataEx((a, b, c, x) =>
            {
                return FwuMultiResponseHandler(fwData, x, true, out fwuMDGetData);
            }, ControllerSessionsContainer.Config.TxOptions, isStopOnNak, fwuMDGetData);

            //Start Update
            return MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData);
        }

        private List<byte[]> FwuMultiResponseHandler(List<byte[]> fwData, byte[] x, bool isAddCrc, out COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET fwuMDGetData)
        {
            ApplicationModel.FirmwareUpdateModel.LastMDGetTime = DateTime.Now;
            var discardLastReportsCount = ApplicationModel.FirmwareUpdateModel.IsDiscardLastReports
                ? ApplicationModel.FirmwareUpdateModel.DiscardLastReportsCount : 0;
            var fwDataCount = fwData.Count - discardLastReportsCount;
            fwuMDGetData = x;
            byte numberOfReports = ApplicationModel.FirmwareUpdateModel.IsReportsLimited
                ? ApplicationModel.FirmwareUpdateModel.ReportsLimit : fwuMDGetData.numberOfReports.Value;
            var fwuMDReportData = new List<byte[]>();
            short firmwareDataIndex = 0;
            for (byte i = 0; i < numberOfReports; i++)
            {
                firmwareDataIndex = (short)((fwuMDGetData.properties1.reportNumber1 << 8) + fwuMDGetData.reportNumber2 + i);
                if (firmwareDataIndex > 0 && firmwareDataIndex < (fwDataCount + 1))
                {
                    List<byte> dataToSend = new List<byte>(new[] { COMMAND_CLASS_FIRMWARE_UPDATE_MD.ID, COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT.ID });
                    if (firmwareDataIndex == fwDataCount)
                    {
                        dataToSend.Add((byte)((firmwareDataIndex >> 8) | 0x80)); // last packet
                    }
                    else
                    {
                        dataToSend.Add((byte)(firmwareDataIndex >> 8));
                    }
                    dataToSend.Add((byte)firmwareDataIndex);
                    dataToSend.AddRange(fwData[firmwareDataIndex - 1]);
                    if (isAddCrc)
                    {
                        short crc = Tools.CalculateCrc16(dataToSend);
                        dataToSend.Add((byte)(crc >> 8));
                        dataToSend.Add((byte)crc);
                    }
                    fwuMDReportData.Add(dataToSend.ToArray());
                    ApplicationModel.SetBusyMessage($"Please wait until the OTA Firmware Update completed.{Environment.NewLine}" +
                        $"packet# {firmwareDataIndex} of {fwData.Count} written.");
                }
            }

            return fwuMDReportData;
        }

        private byte MDRequestGet(NodeTag node, byte[] fwuMDRequestGetData, byte[] fwuMDRequestReportData, bool isRetransmit = false)
        {
            byte ret = 0;
            RequestDataResult res = _device.RequestData(node, fwuMDRequestGetData, ControllerSessionsContainer.Config.TxOptions, fwuMDRequestReportData, _device.Network.RequestTimeoutMs);
            if (res == true && res.Command != null)
            {
                fwuMDRequestReportData = res.Command;
                CommandClassValue[] cmdClassValues = null;
                ApplicationModel.ZWaveDefinition.ParseApplicationObject(res.Command, out cmdClassValues);
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
            else if (!isRetransmit)
            {
                ret = MDRequestGet(node, fwuMDRequestGetData, fwuMDRequestReportData, true);
            }
            return ret;
        }

        public void OnMDStatusReportAction(NodeTag node, Func<ReceiveStatuses, NodeTag, NodeTag, byte[], byte[]> responseCallback, ActionToken token)
        {
            ResponseDataDelegate s = (a, b, c, d) => { return responseCallback(a, b, c, d); };
            token = _device.ResponseDataEx(s, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT());
        }

        public void CancelFirmwareUpdateV1(NodeTag node, int waitTimeoutMs)
        {
            COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = _device.ExpectData(fwuMDGetData, waitTimeoutMs);
            if (res && res.Command != null)
            {
                fwuMDGetData = res.Command;
                //fake last data packet
                COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT _rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT();
                _rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                _rpt.properties1.last = 0x01;
                _rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                _rpt.reportNumber2 = fwuMDGetData.reportNumber2;

                var ret = _device.SendData(node, _rpt, ControllerSessionsContainer.Config.TxOptions);
                if (ret)
                {
                    //just wait for status, status is handled OnStatusReportCallback
                    _device.ExpectData(new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000);
                }
            }
        }

        public void CancelFirmwareUpdateV2(NodeTag node, int waitTimeoutMs)
        {
            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_GET fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = _device.ExpectData(fwuMDGetData, waitTimeoutMs);
            if (res && res.Command != null)
            {
                fwuMDGetData = res.Command;
                //fake last data packet
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT _rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT();
                _rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                _rpt.properties1.last = 0x01;
                _rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                _rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                _rpt.checksum = Tools.CalculateCrc16Array((byte[])_rpt, 0, ((byte[])_rpt).Length - 2);

                var ret = _device.SendData(node, _rpt, ControllerSessionsContainer.Config.TxOptions);
                if (ret)
                {
                    //just wait for status, status is handled OnStatusReportCallback
                    _device.ExpectData(new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000);
                }
            }
        }

        public void CancelFirmwareUpdateV3(NodeTag node, int waitTimeoutMs)
        {
            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_GET fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = _device.ExpectData(fwuMDGetData, waitTimeoutMs);
            if (res && res.Command != null)
            {
                fwuMDGetData = res.Command;
                //fake last data packet
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT _rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT();
                _rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                _rpt.properties1.last = 0x01;
                _rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                _rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                _rpt.checksum = Tools.CalculateCrc16Array((byte[])_rpt, 0, ((byte[])_rpt).Length - 2);

                var ret = _device.SendData(node, _rpt, ControllerSessionsContainer.Config.TxOptions);
                if (ret)
                {
                    //just wait for status, status is handled OnStatusReportCallback
                    _device.ExpectData(new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000);
                }
            }
        }

        public void CancelFirmwareUpdateV4(NodeTag node, int waitTimeoutMs)
        {
            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_GET fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = _device.ExpectData(fwuMDGetData, waitTimeoutMs);
            if (res && res.Command != null)
            {
                fwuMDGetData = res.Command;
                //fake last data packet
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT _rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT();
                _rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V4.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                _rpt.properties1.last = 0x01;
                _rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                _rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                _rpt.checksum = Tools.CalculateCrc16Array((byte[])_rpt, 0, ((byte[])_rpt).Length - 2);

                var ret = _device.SendData(node, _rpt, ControllerSessionsContainer.Config.TxOptions);
                if (ret)
                {
                    //just wait for status, status is handled OnStatusReportCallback
                    _device.ExpectData(new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000);
                }
            }
        }

        public void CancelFirmwareUpdateV5(NodeTag node, int waitTimeoutMs)
        {
            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET fwuMDGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_GET();
            ExpectDataResult res = _device.ExpectData(fwuMDGetData, waitTimeoutMs);
            if (res && res.Command != null)
            {
                fwuMDGetData = res.Command;
                //fake last data packet
                COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT _rpt = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT();
                _rpt.properties1 = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_UPDATE_MD_REPORT.Tproperties1();
                _rpt.properties1.last = 0x01;
                _rpt.properties1.reportNumber1 = fwuMDGetData.properties1.reportNumber1;
                _rpt.reportNumber2 = fwuMDGetData.reportNumber2;
                _rpt.checksum = Tools.CalculateCrc16Array((byte[])_rpt, 0, ((byte[])_rpt).Length - 2);

                var ret = _device.SendData(node, _rpt, ControllerSessionsContainer.Config.TxOptions);
                if (ret)
                {
                    //just wait for status, status is handled OnStatusReportCallback
                    _device.ExpectData(new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_UPDATE_MD_STATUS_REPORT(), 10000);
                }
            }
        }

        public CommandExecutionResult FirmwareUpdateOTAGet(NodeTag node)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Sending OTA Firmware Update Get command.");
            Logger.Log(ApplicationModel.BusyMessage);

            ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = null;
            //Request FIRMWARE UPDATE MD COMMAND CLASS VERSION and wait for report
            COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_GET versionCmdClassGetData = new COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_GET();
            versionCmdClassGetData.requestedCommandClass = COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.ID;

            COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_REPORT versionCmdClassReportData = new COMMAND_CLASS_VERSION.VERSION_COMMAND_CLASS_REPORT();

            RequestDataResult res = _device.RequestData(
                node,
                versionCmdClassGetData,
                ControllerSessionsContainer.Config.TxOptions,
                versionCmdClassReportData,
                _device.Network.RequestTimeoutMs);

            if (res == true && res.Command != null)
            {
                versionCmdClassReportData = res.Command;
                if (versionCmdClassReportData.requestedCommandClass == COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.ID)
                {

                    //Request current firmware VERSION and wait for report
                    COMMAND_CLASS_VERSION.VERSION_GET verGetData = new COMMAND_CLASS_VERSION.VERSION_GET();
                    COMMAND_CLASS_VERSION.VERSION_REPORT verRptData = new COMMAND_CLASS_VERSION.VERSION_REPORT();
                    RequestDataResult verRes = _device.RequestData(
                        node,
                        verGetData,
                        ControllerSessionsContainer.Config.TxOptions,
                        verRptData,
                        _device.Network.RequestTimeoutMs);
                    if (verRes == true && verRes.Command != null)
                    {
                        verRptData = verRes.Command;
                        ApplicationModel.FirmwareUpdateModel.CurrentFirmwareVersion = verRptData.applicationVersion.ToString() + "." +
                            verRptData.applicationSubVersion.ToString("00");
                    }
                    else
                    {
                        ApplicationModel.FirmwareUpdateModel.CurrentFirmwareVersion = "";
                    }
                    ApplicationModel.FirmwareUpdateModel.FirmwareUpdateCommandClassVersion = versionCmdClassReportData.commandClassVersion;
                    ApplicationModel.FirmwareUpdateModel.FirmwareTargets = new List<FirmwareTarget>();
                    if (versionCmdClassReportData.commandClassVersion == 0x01)
                    {
                        //Request COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_GET fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_GET();
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD.FIRMWARE_MD_REPORT();
                        res = _device.RequestData(
                            node,
                            fwuGetData,
                            ControllerSessionsContainer.Config.TxOptions,
                            fwuReportData,
                            _device.Network.RequestTimeoutMs);
                        if (res == true && res.Command != null)
                        {
                            fwuReportData = res.Command;
                            ApplicationModel.FirmwareUpdateModel.Checksum = fwuReportData.checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = fwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = fwuReportData.firmwareId;
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = 80;
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = 40; // default 40
                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0,
                                fwuReportData.firmwareId));
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else if (versionCmdClassReportData.commandClassVersion == 0x02)
                    {
                        //Request COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_GET fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_GET();
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V2.FIRMWARE_MD_REPORT();
                        res = _device.RequestData(
                            node,
                            fwuGetData,
                            ControllerSessionsContainer.Config.TxOptions,
                            fwuReportData,
                            _device.Network.RequestTimeoutMs);
                        if (res == true && res.Command != null)
                        {
                            fwuReportData = res.Command;
                            ApplicationModel.FirmwareUpdateModel.Checksum = fwuReportData.checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = fwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = fwuReportData.firmwareId;
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = 80;
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = 40; // default 40
                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, fwuReportData.firmwareId));
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else if (versionCmdClassReportData.commandClassVersion == 0x03 || versionCmdClassReportData.commandClassVersion == 0x04) //V4 And V3
                    {
                        //Request COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_GET
                        // and wait for COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT
                        // fill FirmwareViewModel props
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_GET fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_GET();
                        COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT();
                        res = _device.RequestData(
                            node,
                            fwuGetData,
                            ControllerSessionsContainer.Config.TxOptions,
                            fwuReportData,
                            _device.Network.RequestTimeoutMs);
                        if (res == true && res.Command != null)
                        {
                            fwuReportData = res.Command;
                            ApplicationModel.FirmwareUpdateModel.Checksum = fwuReportData.firmware0Checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = fwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = fwuReportData.firmware0Id;
                            ApplicationModel.FirmwareUpdateModel.IsFirmwareUpgradable = (fwuReportData.firmwareUpgradable != 0x00);
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = Tools.GetInt32(fwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.NumberOfFirmwareTargets = fwuReportData.numberOfFirmwareTargets;

                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, fwuReportData.firmware0Id));
                            if (fwuReportData.vg1 != null)
                            {
                                foreach (COMMAND_CLASS_FIRMWARE_UPDATE_MD_V3.FIRMWARE_MD_REPORT.TVG1 item in fwuReportData.vg1)
                                {
                                    if (item.firmwareId != null)
                                    {
                                        firmwareTargets.Add(new FirmwareTarget(firmwareTargets.Count, item.firmwareId));
                                    }
                                }
                            }
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ApplicationModel.FirmwareUpdateModel.HardwareVersion = 0;
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else if (versionCmdClassReportData.commandClassVersion == 0x05) //V5
                    {
                        var fwuGetData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_MD_GET();
                        var fwuReportData = new COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.FIRMWARE_MD_REPORT();
                        res = _device.RequestData(
                            node,
                            fwuGetData,
                            ControllerSessionsContainer.Config.TxOptions,
                            fwuReportData,
                            _device.Network.RequestTimeoutMs);
                        if (res == true && res.Command != null)
                        {
                            fwuReportData = res.Command;
                            ApplicationModel.FirmwareUpdateModel.Checksum = fwuReportData.firmware0Checksum;
                            ApplicationModel.FirmwareUpdateModel.ManufacturerID = fwuReportData.manufacturerId;
                            ApplicationModel.FirmwareUpdateModel.FirmwareID = fwuReportData.firmware0Id;
                            ApplicationModel.FirmwareUpdateModel.IsFirmwareUpgradable = (fwuReportData.firmwareUpgradable != 0x00);
                            ApplicationModel.FirmwareUpdateModel.MaxFragmentSize = Tools.GetInt32(fwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.FragmentSize = Tools.GetInt32(fwuReportData.maxFragmentSize);
                            ApplicationModel.FirmwareUpdateModel.HardwareVersion = fwuReportData.hardwareVersion;
                            ApplicationModel.FirmwareUpdateModel.NumberOfFirmwareTargets = fwuReportData.numberOfFirmwareTargets;

                            List<FirmwareTarget> firmwareTargets = new List<FirmwareTarget>();
                            firmwareTargets.Add(new FirmwareTarget(0, fwuReportData.firmware0Id));
                            if (fwuReportData.vg1 != null)
                            {
                                foreach (var item in fwuReportData.vg1)
                                {
                                    if (item.firmwareId != null)
                                    {
                                        firmwareTargets.Add(new FirmwareTarget(firmwareTargets.Count, item.firmwareId));
                                    }
                                }
                            }
                            ApplicationModel.FirmwareUpdateModel.FirmwareTargets = firmwareTargets;
                            ret = CommandExecutionResult.OK;
                        }
                    }
                    else
                    {
                        ret = CommandExecutionResult.Failed;
                        Logger.Log("OTA Firmware Update is not supported");
                    }
                    ApplicationModel.Invoke(() => ApplicationModel.FirmwareUpdateModel.SelectedFirmwareTarget = ApplicationModel.FirmwareUpdateModel.FirmwareTargets.FirstOrDefault());
                }
            }
            return ret;
        }

        public CommandExecutionResult DeleteReturnRoute(NodeTag srcNode)
        {
            ActionToken token = null;
            return DeleteReturnRoute(srcNode, false, out token);
        }

        public CommandExecutionResult DeleteReturnRoute(NodeTag srcNode, bool isSUCReturnRoute, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Remove Return Route started.");
            Logger.Log(ApplicationModel.BusyMessage);
            ActionResult result;
            if (isSUCReturnRoute)
            {
                result = (_device as Controller).DeleteSucReturnRoute(srcNode, out token);
            }
            else
            {
                result = (_device as Controller).DeleteReturnRoute(srcNode, out token);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                Logger.Log($"Return Route for Node {srcNode} deleted successfully ");
            }
            else
            {
                Logger.LogFail($"Return Route for Node {srcNode} delete failed");
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
                    var result = (_device as Controller).AssignReturnRoute(srcNode, destNode, out token);
                    if (result)
                    {
                        ret = CommandExecutionResult.OK;
                        Logger.Log($"Return Route from Node {srcNode.Id} to Node {destNode.Id} assigned successfully");
                    }
                    else
                    {
                        Logger.LogFail($"Return Route from Node {srcNode.Id} to Node {destNode.Id} assign failed");
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssignSUCReturnRoute(NodeTag srcNode, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Assign SUC Return Route started.");
            Logger.Log(ApplicationModel.BusyMessage);
            var result = (_device as Controller).AssignSucReturnRoute(srcNode, out token);
            if (result)
            {
                ret = CommandExecutionResult.OK;
                Logger.Log($"SUC Return Route from SUC Node to Node {srcNode} assigned successfully");
            }
            else
            {
                Logger.LogFail($"Return Route from SUC Node to Node {srcNode} assign failed");
            }
            return ret;
        }

        public CommandExecutionResult AssignPriorityReturnRoute(NodeTag srcNode, NodeTag destNode, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Assign Priority Return Route started.");
            Logger.Log(ApplicationModel.BusyMessage);
            var result = (_device as Controller).AssignPriorityReturnRoute(srcNode, destNode, repeater0, repeater1, repeater2, repeater3, routeSpeed, out token);
            if (result)
            {
                ret = CommandExecutionResult.OK;
                Logger.Log($"Priority Return Route from Node {srcNode} to Node {destNode} assigned successfully");
            }
            else
            {
                Logger.LogFail($"Priority Return Route from Node {srcNode} to Node {destNode} assign failed");
            }
            return ret;
        }

        public CommandExecutionResult AssignPrioritySUCReturnRoute(NodeTag srcNode, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            ApplicationModel.SetBusyMessage("Assign Priority SUC Return Route started.");
            Logger.Log(ApplicationModel.BusyMessage);
            var result = (_device as Controller).AssignPrioritySucReturnRoute(srcNode, repeater0, repeater1, repeater2, repeater3, routeSpeed, out token);
            if (result)
            {
                ret = CommandExecutionResult.OK;
                Logger.Log($"Priority Return Route from SUC Node to Node {srcNode} assigned successfully");
            }
            else
            {
                Logger.LogFail($"Priority Return Route from SUC Node to Node {srcNode} assign failed");
            }
            return ret;
        }

        public CommandExecutionResult AssociationGroupNameGet(NodeTag associativeDevice, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;
            RequestDataResult result;
            if (associativeDevice.EndPointId == 0)
            {
                COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_GET cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_GET();
                cmd.groupingIdentifier = groupId;
                result = _device.RequestData(associativeDevice,
                    cmd, ControllerSessionsContainer.Config.TxOptions,
                    new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_REPORT(),
                    _device.Network.RequestTimeoutMs);
            }
            else
            {
                COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_GET cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_GET();
                cmd.groupingIdentifier = groupId;
                result = _device.RequestData(associativeDevice,
                    EncapData(cmd, associativeDevice.EndPointId), ControllerSessionsContainer.Config.TxOptions,
                    new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP(),
                    _device.Network.RequestTimeoutMs);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_NAME_REPORT rpt;
                if (associativeDevice.EndPointId == 0)
                {
                    rpt = result.Command;
                }
                else
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                    var rep = new List<byte>();
                    rep.Add(encaped.commandClass);
                    rep.Add(encaped.command);
                    rep.AddRange(encaped.parameter);
                    rpt = rep.ToArray();
                }
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
            RequestDataResult result;
            COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_GET cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_GET();
            COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_REPORT rpt = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_INFO_REPORT();
            cmd.groupingIdentifier = groupId;
            if (associativeDevice.EndPointId == 0)
            {
                result = _device.RequestData(associativeDevice,
                    cmd, ControllerSessionsContainer.Config.TxOptions,
                    rpt,
                    _device.Network.RequestTimeoutMs);
            }
            else
            {
                cmd.groupingIdentifier = groupId;
                result = _device.RequestData(associativeDevice,
                    EncapData(cmd, associativeDevice.EndPointId), ControllerSessionsContainer.Config.TxOptions,
                    new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP(),
                    _device.Network.RequestTimeoutMs);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                if (associativeDevice.EndPointId == 0)
                {
                    rpt = result.Command;
                }
                else
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                    var rep = new List<byte>();
                    rep.Add(encaped.commandClass);
                    rep.Add(encaped.command);
                    rep.AddRange(encaped.parameter);
                    rpt = rep.ToArray();
                }
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

            COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_GET cmd = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_GET();
            COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_REPORT rpt = new COMMAND_CLASS_ASSOCIATION_GRP_INFO.ASSOCIATION_GROUP_COMMAND_LIST_REPORT();
            cmd.groupingIdentifier = groupId;
            RequestDataResult result;
            if (associativeDevice.EndPointId == 0)
            {
                result = _device.RequestData(associativeDevice, cmd, ControllerSessionsContainer.Config.TxOptions, rpt, _device.Network.RequestTimeoutMs);
            }
            else
            {
                cmd.groupingIdentifier = groupId;
                result = _device.RequestData(associativeDevice,
                    EncapData(cmd, associativeDevice.EndPointId), ControllerSessionsContainer.Config.TxOptions,
                    new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP(),
                    _device.Network.RequestTimeoutMs);
            }
            if (result)
            {
                ret = CommandExecutionResult.OK;
                if (associativeDevice.EndPointId == 0)
                {
                    rpt = result.Command;
                }
                else
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                    var rep = new List<byte>();
                    rep.Add(encaped.commandClass);
                    rep.Add(encaped.command);
                    rep.AddRange(encaped.parameter);
                    rpt = rep.ToArray();
                }
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

        public CommandExecutionResult AssociationRemove(NodeTag associativeDevice, byte groupId, IEnumerable<NodeTag> nodeIds)
        {
            var ret = CommandExecutionResult.Failed;
            if (_device.Network.HasCommandClass(associativeDevice, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
            {
                COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = nodeIds.Where(x => x.EndPointId == 0).Select(x => (byte)x.Id).ToList();
                foreach (var item in nodeIds.Where(x => x.EndPointId > 0))
                {
                    var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG.Tproperties1();
                    prop.endPoint = item.EndPointId;
                    var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REMOVE.TVG();
                    tvg.multiChannelNodeId = (byte)item.Id;
                    tvg.properties1 = prop;
                    cmd.vg.Add(tvg);
                }
                byte[] data = associativeDevice.EndPointId == 0 ? (byte[])cmd : EncapData(cmd, associativeDevice.EndPointId);
                if (_device.SendData(associativeDevice, data, ControllerSessionsContainer.Config.TxOptions))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            else
            {
                COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REMOVE cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REMOVE();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = nodeIds.Select(x => (byte)x.Id).Distinct().ToList();
                if (_device.SendData(associativeDevice, cmd, ControllerSessionsContainer.Config.TxOptions))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationCreate(NodeTag associativeDevice, byte groupId, IEnumerable<NodeTag> nodeIds)
        {
            var ret = CommandExecutionResult.Failed;
            if (_device.Network.HasCommandClass(associativeDevice, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
            {
                COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = nodeIds.Where(x => x.EndPointId == 0).Select(x => (byte)x.Id).ToList();
                foreach (var item in nodeIds.Where(x => x.EndPointId > 0))
                {
                    var prop = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG.Tproperties1();
                    prop.endPoint = item.EndPointId;
                    var tvg = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_SET.TVG();
                    tvg.multiChannelNodeId = (byte)item.Id;
                    tvg.properties1 = prop;
                    cmd.vg.Add(tvg);
                }
                byte[] data = associativeDevice.EndPointId == 0 ? (byte[])cmd : EncapData(cmd, associativeDevice.EndPointId);
                if (_device.SendData(associativeDevice, data, ControllerSessionsContainer.Config.TxOptions))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            else
            {
                COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET cmd = new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_SET();
                cmd.groupingIdentifier = groupId;
                cmd.nodeId = nodeIds.Select(x => (byte)x.Id).Distinct().ToList();
                if (_device.SendData(associativeDevice, cmd, ControllerSessionsContainer.Config.TxOptions))
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGroupingsGet(NodeTag associativeDevice)
        {
            var ret = CommandExecutionResult.Failed;
            if (_device.Network.HasCommandClass(associativeDevice, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
            {
                byte[] groupGet = associativeDevice.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_GET() :
                    EncapData(new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_GET(), associativeDevice.EndPointId);
                byte[] groupReport = associativeDevice.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_REPORT() :
                    (byte[])new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                RequestDataResult result = _device.RequestData(associativeDevice, groupGet, ControllerSessionsContainer.Config.TxOptions, groupReport, _device.Network.RequestTimeoutMs);
                if (result)
                {
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GROUPINGS_REPORT rpt;
                    if (associativeDevice.EndPointId == 0)
                    {
                        rpt = result.Command;
                    }
                    else
                    {
                        COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                        var rep = new List<byte>();
                        rep.Add(encaped.commandClass);
                        rep.Add(encaped.command);
                        rep.AddRange(encaped.parameter);
                        rpt = rep.ToArray();
                    }

                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                        Devices.FirstOrDefault(x => x.Device == associativeDevice);
                    if (ad != null)
                    {
                        ad.SetGroups(rpt.supportedGroupings);
                        if (ad.Groups != null && ad.Groups.Count > 0)
                            foreach (var item in ad.Groups)
                                ret = AssociationGet(ad.Device, item.Id);
                    }
                }
            }
            else
            {
                byte[] groupGet = associativeDevice.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_GET() :
                    EncapData(new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_GET(), associativeDevice.EndPointId);
                byte[] groupReport = associativeDevice.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_REPORT() :
                    (byte[])new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();

                RequestDataResult result = _device.RequestData(associativeDevice, groupGet, ControllerSessionsContainer.Config.TxOptions, groupReport, _device.Network.RequestTimeoutMs);
                if (result)
                {
                    COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GROUPINGS_REPORT rpt;
                    if (associativeDevice.EndPointId == 0)
                    {
                        rpt = result.Command;
                    }
                    else
                    {
                        COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                        var rep = new List<byte>();
                        rep.Add(encaped.commandClass);
                        rep.Add(encaped.command);
                        rep.AddRange(encaped.parameter);
                        rpt = rep.ToArray();
                    }
                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == associativeDevice.Id).
                        Devices.FirstOrDefault(x => x.Device == associativeDevice);
                    if (ad != null)
                    {
                        ad.SetGroups(rpt.supportedGroupings);
                        if (ad.Groups != null && ad.Groups.Count > 0)
                            foreach (var item in ad.Groups)
                                ret = AssociationGet(ad.Device, item.Id);
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult AssociationGet(NodeTag node, byte groupId)
        {
            var ret = CommandExecutionResult.Failed;
            if (_device.Network.HasCommandClass(node, COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
            {
                RequestDataResult result;
                if (node.EndPointId == 0)
                {
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET();
                    cmd.groupingIdentifier = groupId;
                    result = _device.RequestData(node,
                        cmd, ControllerSessionsContainer.Config.TxOptions,
                        new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT(),
                        _device.Network.RequestTimeoutMs);
                }
                else
                {
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT rpt = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT();
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET cmd = new COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_GET();
                    cmd.groupingIdentifier = groupId;
                    result = _device.RequestData(node,
                        EncapData(cmd, node.EndPointId), ControllerSessionsContainer.Config.TxOptions,
                        new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP(),
                        _device.Network.RequestTimeoutMs);
                }
                if (result)
                {
                    ret = CommandExecutionResult.OK;
                    COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.MULTI_CHANNEL_ASSOCIATION_REPORT rpt;
                    if (node.EndPointId == 0)
                    {
                        rpt = result.Command;
                    }
                    else
                    {
                        COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                        var rep = new List<byte>();
                        rep.Add(encaped.commandClass);
                        rep.Add(encaped.command);
                        rep.AddRange(encaped.parameter);
                        rpt = rep.ToArray();
                    }

                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == node.Id).
                        Devices.FirstOrDefault(x => x.Device == node);
                    if (ad != null)
                    {
                        IList<NodeTag> nodeIds = rpt.nodeId.Select(x => new NodeTag(x, 0)).ToList();
                        nodeIds.AddRange(rpt.vg.Select(x => new NodeTag(x.multiChannelNodeId, x.properties1.endPoint)).ToArray());
                        ad.UpdateGroup(rpt.groupingIdentifier, rpt.maxNodesSupported, nodeIds);
                    }
                }
            }
            else
            {
                byte[] groupGet = node.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GET { groupingIdentifier = groupId } :
                    EncapData(new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_GET { groupingIdentifier = groupId }, node.EndPointId);
                byte[] groupReport = node.EndPointId == 0 ?
                    (byte[])new COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REPORT() :
                    (byte[])new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                RequestDataResult result = _device.RequestData(node, groupGet, ControllerSessionsContainer.Config.TxOptions, groupReport, _device.Network.RequestTimeoutMs);
                if (result)
                {
                    COMMAND_CLASS_ASSOCIATION.ASSOCIATION_REPORT rpt;
                    if (node.EndPointId == 0)
                    {
                        rpt = result.Command;
                    }
                    else
                    {
                        COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = result.Command;
                        var rep = new List<byte>();
                        rep.Add(encaped.commandClass);
                        rep.Add(encaped.command);
                        rep.AddRange(encaped.parameter);
                        rpt = rep.ToArray();
                    }
                    ret = CommandExecutionResult.OK;

                    var ad = ApplicationModel.AssociationsModel.
                        AssociativeApplications.First(x => x.RootDevice.Id == node.Id).
                        Devices.FirstOrDefault(x => x.Device == node);
                    if (ad != null)
                    {
                        ad.UpdateGroup(rpt.groupingIdentifier, rpt.maxNodesSupported, rpt.nodeid.Select(x => new NodeTag(x, 0)).ToList());
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

            token = (_device as Controller).RequestNodeNeighborUpdate(node, GetTimeoutValue(false), null);
            var result = (RequestNodeNeighborUpdateResult)token.WaitCompletedSignal();
            if (result.IsStateCompleted)
            {
                Logger.LogOk("Request Neighbors Update Done");
                ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeNeighborUpdateCommand), Message = "Neighbor Node Updated" }));
                ret = CommandExecutionResult.OK;
            }
            else
            {
                Logger.LogFail($"Request Neighbor Update Failed with state {result.State}");
            }
            return ret;
        }

        public CommandExecutionResult StartPowerLevelTest(NodeTag node, NodeTag destinationNode, byte powerLevel, ActionToken token)
        {
            var ret = CommandExecutionResult.Inconclusive;
            COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_SET powerLevelCmd = new COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_SET();
            powerLevelCmd.powerLevel = powerLevel;
            powerLevelCmd.testFrameCount = new byte[] { 0x00, 0x0A }; //10 (NOPs)
            powerLevelCmd.testNodeid = (byte)destinationNode.Id;
            COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT powerLevelReport = new COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT();

            RequestDataResult requestDataResult = _device.RequestData(node, powerLevelCmd, ControllerSessionsContainer.Config.TxOptions, powerLevelReport, 10000, out token); //10 Seconds
            if (requestDataResult && requestDataResult.Command != null)
            {
                powerLevelReport = requestDataResult.Command;
                if (powerLevelReport.statusOfOperation == 0x01)
                {
                    Logger.Log($"LWRdB {powerLevel}dB nodes: {node} - {destinationNode} Passed");
                    ret = CommandExecutionResult.OK;
                }
                else
                {
                    Logger.Log($"LWRdB {powerLevel}dB nodes: {node} - {destinationNode} FAILED");
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
                    Logger.Log($"LWRdB {powerLevel}dB nodes: {node} - {destinationNode} FAILED. Report Missing");
            }

            return ret;
        }

        public CommandExecutionResult GetRoutingInfo(NodeTag node, out NodeTag[] routingNodes)
        {
            var ret = CommandExecutionResult.Failed;
            routingNodes = null;
            var res = (_device as Controller).GetRoutingInfo(node, 0, 0);
            if (res)
            {
                routingNodes = res.RoutingNodes;
                Logger.Log(string.Format("Node {0} neighbors: {1}", node, Tools.GetNodeIds((res.RoutingNodes ?? new NodeTag[0]).Select(x => (byte)x.Id).ToArray(), ", ")));
                ret = CommandExecutionResult.OK;
            }
            return ret;
        }

        public CommandExecutionResult AreNeighbors(NodeTag srcNode, NodeTag destNode)
        {
            ApplicationModel.SetBusyMessage("Are Neighbours in progress");
            var ret = CommandExecutionResult.Failed;
            bool areNeigbhbors = false;
            if (srcNode.Id != 0x00 && destNode.Id != 0x00)
            {
                Logger.Log(string.Format("Checking if Node {0} and Node {1} are neighbors", srcNode.Id, destNode.Id));

                //source node
                var srcRepsRoutingInfoRes = (_device as Controller).GetRoutingInfo(srcNode, 0, 0);
                if (srcRepsRoutingInfoRes)
                {
                    Logger.Log(string.Format("Node {0} - 'All' {1} NBs [{2}]", srcNode,
                        srcRepsRoutingInfoRes.RoutingNodes.Length,
                        string.Join(",", srcRepsRoutingInfoRes.RoutingNodes.Select(x => x.ToString()).ToArray())));
                    ret = CommandExecutionResult.OK;
                }
                //destination node
                var destRepsRoutingInfoRes = (_device as Controller).GetRoutingInfo(destNode, 0, 0);
                if (destRepsRoutingInfoRes)
                {
                    Logger.Log(string.Format("Node {0} - 'All' {1} NBs [{2}]", destNode,
                        destRepsRoutingInfoRes.RoutingNodes.Length,
                        string.Join(",", destRepsRoutingInfoRes.RoutingNodes.Select(x => x.ToString()).ToArray())));
                    ret = CommandExecutionResult.OK;
                }
                //comparison
                areNeigbhbors = destRepsRoutingInfoRes.RoutingNodes.Any(x => x == srcNode)
                                && srcRepsRoutingInfoRes.RoutingNodes.Any(x => x == destNode);

                Logger.Log(string.Format("Node {0} and Node {1} are{2} neighbors", srcNode, destNode, areNeigbhbors ? string.Empty : " NOT"));
            }
            return ret;
        }

        public ActionResult SetLearnMode(LearnModes mode, out ActionToken token)
        {
            StopSmartListener();
            var caption = "Controller is working in learning mode";
            var learnMode = mode;
            if (learnMode == LearnModes.LearnModeSmartStart)
            {
                learnMode = LearnModes.NetworkMask | learnMode;
            }
            ControllerSessionsContainer.Config.DeleteConfiguration(_device);
            var endDevice = _device as EndDevice;
            if (endDevice != null)
            {
                var commandClasses = _device.Network.GetCommandClasses(_device.Network.NodeTag);
                endDevice.ApplicationNodeInformationCmdClasses(
                    commandClasses,
                    commandClasses,
                    commandClasses.Where(x => x != COMMAND_CLASS_SECURITY.ID && x != COMMAND_CLASS_SECURITY_2.ID).ToArray());
            }
            token = _device.SetLearnMode(learnMode, 65000, null);
            using (var logAction = ReportAction(caption, token))
            {
                logAction.Caption = "Learn mode";
                var ret = (SetLearnModeResult)token.WaitCompletedSignal();
                ApplicationModel.DskPinDialog.Close();
                Update(false);
                List<byte> cmdClasses = new List<byte>();
                var cc = _device.Network.GetCommandClasses(_device.Network.NodeTag);
                if (cc != null)
                {
                    cmdClasses.AddRange(cc.Where(x => x != COMMAND_CLASS_SECURITY.ID && x != COMMAND_CLASS_SECURITY_2.ID).ToArray());
                    if (_device.Network.IsEnabledS0)
                    {
                        cmdClasses.Add(COMMAND_CLASS_SECURITY.ID);
                    }
                    // CC:009F.01.00.21.008 : A supporting node MUST always advertise the Security 2 Command Class in its NIF, 
                    // regardless of the inclusion status and security bootstrapping outcome.
                    if (_device.Network.IsEnabledS2_UNAUTHENTICATED || _device.Network.IsEnabledS2_AUTHENTICATED || _device.Network.IsEnabledS2_ACCESS)
                    {
                        cmdClasses.Add(COMMAND_CLASS_SECURITY_2.ID);
                    }
                    //
                    _device.Network.SetCommandClasses(cmdClasses.ToArray());
                }

                SetNodeInformation(out token);
                if (ret != null && (ret || ret.LearnModeStatus != LearnModeStatuses.None))
                {
                    logAction.Caption += " " + ret.LearnModeStatus;
                    if (SecurityManager != null)
                    {
                        var _securitySettings = ApplicationModel.ConfigurationItem.SecuritySettings;
                        foreach (var scheme in SecuritySchemeSet.ALL)
                        {
                            SecurityManager.SecurityManagerInfo.SetTestNetworkKey(null, scheme, false);
                        }
                        foreach (var scheme in SecuritySchemeSet.ALLS2_LR)
                        {
                            SecurityManager.SecurityManagerInfo.SetTestNetworkKey(null, scheme, true);
                        }
                    }
                    if (ret.LearnModeStatus != LearnModeStatuses.Replicated)
                    {
                        ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Clear());
                        ApplicationModel.SmartStartModel.ResetFields();
                    }
                    if (ret.SubstituteStatus == SubstituteStatuses.Failed)
                    {
                        if (ret.LearnModeStatus != LearnModeStatuses.Removed)
                        {
                            SecurityManager.SecurityManagerInfo.Network.ResetSecuritySchemes();
                        }
                        logAction.Caption += " (Security Failed)";
                    }
                }
                if (ret.LearnModeStatus == LearnModeStatuses.Removed)
                {
                    SaveSecurityKeys(true);
                }
                else if (ret.LearnModeStatus == LearnModeStatuses.Added)
                {
                    SaveSecurityKeys(false);
                }
                if (SecurityManager != null)
                {
                    SecurityManager.GenerateNewSecretKeyS2();
                }
                Task.Delay(1000);
                StartSmartListener();
                return ret;
            }
        }

        public CommandExecutionResult SetVirtualDeviceLearnMode(VirtualDeviceLearnModes mode, NodeTag endDeviceNode, out ActionToken token)
        {
            var ret = CommandExecutionResult.Failed;
            StopSmartListener();
            token = null;
            string msgLearnMode = "Set Virtual Device Learn Mode is executed.";
            ApplicationModel.SetBusyMessage(msgLearnMode);
            Logger.Log(msgLearnMode);
            if (IsBridgeControllerLibrary)
            {
                var bridgeController = ((BridgeController)_device);
                List<byte> virtualDeviceSupportedCC = SECURED_COMMAND_CLASSES_VIRTUAL.ToList();
                if (bridgeController.Network.HasSecurityScheme(SecuritySchemes.S0))
                {
                    virtualDeviceSupportedCC.Add(COMMAND_CLASS_SECURITY.ID);
                }
                if (bridgeController.Network.HasSecurityScheme(SecuritySchemeSet.ALLS2))
                {
                    virtualDeviceSupportedCC.Add(COMMAND_CLASS_SECURITY_2.ID);
                }
                bridgeController.ApplicationVirtualDeviceNodeInformation(true, 0x10, 0x00, virtualDeviceSupportedCC.ToArray());

                _device.MemoryGetId();
                token = bridgeController.SetVirtualDeviceLearnMode(endDeviceNode, mode, 65000, null);
                SetLearnModeResult result = (SetLearnModeResult)token.WaitCompletedSignal();
                if (result != null && (result && result.LearnModeStatus != LearnModeStatuses.None))
                {
                    if (result.LearnModeStatus != LearnModeStatuses.Removed)
                    {
                        if (result.SubstituteStatus == SubstituteStatuses.Failed)
                        {
                            Logger.LogWarning("Learn mode completed (Security Failed)");
                        }
                        else
                        {
                            Logger.LogOk("Learn mode completed");
                        }
                        var protocolResult = (_device as Controller).GetProtocolInfo(result.Node);
                        _device.Network.SetNodeInfo(result.Node, protocolResult.NodeInfo);
                        _device.Network.SetVirtual(result.Node, true);
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.AddOrUpdateNode(result.Node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        });
                        ret = CommandExecutionResult.OK;
                    }
                    else
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.RemoveNode(endDeviceNode);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        });
                        ret = CommandExecutionResult.OK;
                        Logger.Log("Remove Virtual Node completed");
                    }
                }
                else
                {
                    if (result.State == ActionStates.Cancelled)
                    {
                        Logger.LogFail("Learn Mode Operation Cancelled");
                        ret = CommandExecutionResult.Canceled;
                    }
                    else
                        Logger.LogFail(string.Format("Learn mode returned result: {0}", result.State));
                }
            }
            StartSmartListener();
            return ret;
        }

        public ActionResult SetDefault(bool isDefaultCommandClasses, out ActionToken token)
        {
            ActionResult ret = null;
            var caption = "Reset";
            token = _device.SetDefault(null);
            using (ReportAction(caption, token))
            {
                ret = token.WaitCompletedSignal();
                if (ret)
                {
                    _device.Network.DelayResponseMs = 0;
                    StopSmartListener();
                    _device.Cancel(_wakeupNoMoreToken);
                    _device.Cancel(_wakeupSetIntervalToken);
                    var securitySettings = ApplicationModel.ConfigurationItem.SecuritySettings;
                    ControllerSessionsContainer.Config.DeleteConfiguration(_device);
                    if (ApplicationModel.TraceCapture.TraceCaptureSettingsModel.IsWatchdogEnabled)
                    {
                        _device.WatchDogStart();
                        Logger.LogWarning("Watchdog started.");
                    }
                    Update(isDefaultCommandClasses);
                    List<byte> cmdClasses = new List<byte>();
                    var cc = _device.Network.GetCommandClasses(_device.Network.NodeTag);
                    if (cc != null)
                    {
                        cmdClasses.AddRange(cc.Where(x => x != COMMAND_CLASS_SECURITY.ID && x != COMMAND_CLASS_SECURITY_2.ID).ToArray());
                        if (_device.Network.HasSecurityScheme(SecuritySchemes.S0))
                        {
                            cmdClasses.Add(COMMAND_CLASS_SECURITY.ID);
                        }
                        if (_device.Network.HasSecurityScheme(SecuritySchemeSet.ALLS2))
                        {
                            cmdClasses.Add(COMMAND_CLASS_SECURITY_2.ID);
                        }
                        _device.Network.SetCommandClasses(cmdClasses.ToArray());
                    }
                    SetNodeInformation(out token);
                    SaveSecurityKeys(true);
                    RestartSupportTasks();
                    StartSmartListener();
                    ApplicationModel.ConfigurationItem.SecuritySettings = securitySettings;
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Clear();
                        //ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, "Switched to Default");
                    });
                    ApplicationModel.SmartStartModel.ResetFields();
                }
            }
            return ret;
        }

        private void RestartSupportTasks()
        {
            StopSupportTasks();
            StartSupportTasks();
        }

        public void SaveSecurityKeys(bool isClearFile)
        {
            if (SecurityManager != null)
            {
                var controllerConfiguration = ControllerSessionsContainer.Config.ControllerConfiguration;
                if (controllerConfiguration.IsSaveKeys && !string.IsNullOrEmpty(controllerConfiguration.KeysStorageFolder))
                {
                    if (!Directory.Exists(controllerConfiguration.KeysStorageFolder))
                    {
                        //LogError("Invalid Key Storage Folder Path");
                    }
                    else
                    {
                        if (!Directory.Exists(controllerConfiguration.KeysStorageFolder))
                            Directory.CreateDirectory(controllerConfiguration.KeysStorageFolder);
                        string extension = ".txt";
                        char separator = ';';
                        int counter = 1;
                        string fileName = string.Concat(Path.Combine(controllerConfiguration.KeysStorageFolder,
                                                        string.Concat(_device.HomeId.Select(x => x.ToString("X2").ToUpper()).ToArray())),
                                                        extension);
                        var storedKeys = new List<StoredKey>();
                        var lines = new List<string>();
                        if (File.Exists(fileName) && !isClearFile)
                        {
                            lines = File.ReadAllLines(fileName).ToList();
                            foreach (var line in lines)
                            {
                                var values = line.Split(separator);
                                storedKeys.Add(new StoredKey() { SecurityId = values[0], Key = values[1] });
                            }
                        }
                        var securitySchemes = _device.Network.GetSecuritySchemes();
                        if (securitySchemes != null)
                        {
                            for (int i = 0; i < securitySchemes.Length; i++)
                            {
                                byte[] actualNetworkKey = SecurityManager.SecurityManagerInfo.GetActualNetworkKey(securitySchemes[i], false);
                                if (actualNetworkKey != null || _device.Network.HasSecurityScheme(securitySchemes[i]))
                                {
                                    var netKey = string.Concat(actualNetworkKey.Select(x => x.ToString("x2").ToUpper()).ToArray());
                                    string securityId = COMMAND_CLASS_SECURITY_2.ID.ToString("X2");
                                    if (securitySchemes[i] == SecuritySchemes.S0)
                                    {
                                        securityId = COMMAND_CLASS_SECURITY.ID.ToString("X2");
                                    }
                                    if (!(storedKeys.Contains(new StoredKey() { SecurityId = securityId, Key = netKey })))
                                    {
                                        lines.Add(string.Concat(securityId, separator, netKey, separator, counter));
                                    }

                                    if (securitySchemes[i] == SecuritySchemes.S2_ACCESS || securitySchemes[i] == SecuritySchemes.S2_AUTHENTICATED)
                                    {
                                        byte[] actualNetworkKeyLR = SecurityManager.SecurityManagerInfo.GetActualNetworkKey(securitySchemes[i], true);
                                        if (actualNetworkKeyLR != null)
                                        {
                                            netKey = string.Concat(actualNetworkKeyLR.Select(x => x.ToString("x2").ToUpper()).ToArray());
                                            if (!storedKeys.Contains(new StoredKey() { SecurityId = securityId, Key = netKey }))
                                            {
                                                lines.Add(string.Concat(securityId, separator, netKey, separator, counter));
                                            }
                                        }
                                    }
                                }
                            }
                            File.WriteAllLines(fileName, lines.ToArray());
                            new DirectoryInfo(controllerConfiguration.KeysStorageFolder).GetFiles().
                                OrderByDescending(x => x.LastWriteTime.ToFileTime()).Skip(100).ToList().
                                ForEach(x => x.Delete());
                        }
                    }
                }
            }
        }

        public ActionResult ControllerChange(out ActionToken token)
        {
            var ctrlDevice = _device as Controller;
            if (ctrlDevice != null)
            {
                StopSmartListener();
                var caption = "Shift the primary role to another controller in the network";
                var busyText = "Shifting the primary role to another controller in the network. Set the receiving controller in the learning mode.";
                ControllerSessionsContainer.Config.DeleteConfiguration(_device);
                token = ctrlDevice.ControllerChange(ControllerChangeModes.Start, 65000, null);
                using (ReportAction(caption, busyText, token))
                {
                    var ret = (AddRemoveNodeResult)token.WaitCompletedSignal();
                    if (ret)
                    {
                        RequestNodeInfo(ret.Node, token);
                        if (ret.CommandClasses != null && ret.CommandClasses.Contains(COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID))
                        {
                            GetNodeRoleType(ret.Node);
                        }
                        var protocolRes = ctrlDevice.GetProtocolInfo(ret.Node);
                        Update(false);
                        ctrlDevice.Network.SetNodeInfo(ret.Node, protocolRes.NodeInfo);
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.AddOrUpdateNode(ret.Node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList | NotifyProperty.ControllerInfo);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ShiftControllerCommand), Message = "Controller Shift" });
                        });
                    }
                    StartSmartListener();
                    return ret;
                }
            }
            else
            {
                token = null;
                return null;
            }
        }

        public ActionResult RequestNetworkUpdate(out ActionToken token)
        {
            var caption = "Request network update";
            token = _device.RequestNetworkUpdate(null);
            using (ReportAction(caption, token))
            {
                var result = (RequestNetworkUpdateResult)token.WaitCompletedSignal();
                if (result.IsStateCompleted)
                {
                    Update(false);
                    Logger.Log("Request network update completed");
                    ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNetworkUpdateCommand), Message = "Network Updated" }));
                }
                return result;
            }
        }

        public ActionResult SetSucNode(NodeTag node, out ActionToken mToken)
        {
            mToken = null;
            var caption = "Set SIS";
            StopSmartListener();
            var GetSucResult = (_device as Controller).GetSucNodeId();
            if (GetSucResult.SucNode.Id == 0)
            {
                using (var logAction = ReportAction(caption, null))
                {
                    bool isLowPower = (ControllerSessionsContainer.Config.TxOptions & TransmitOptions.TransmitOptionLowPower) == TransmitOptions.TransmitOptionLowPower;
                    var result = (_device as Controller).SetSucNodeID(node, true, isLowPower, 0x01);
                    logAction.State = result.State;
                    Update(false);
                    StartSmartListener();
                    return result;
                }
            }
            else
            {
                LogError($"In the network can be only one SUC/SIS, Node Id = {GetSucResult.SucNode}");
                return GetSucResult;
            }
        }

        public void Cancel(NodeTag cancelNode, ActionToken token)
        {
            _device.Cancel(token);
        }

        public void SendNop(NodeTag node, out ActionToken token)
        {
            token = _device.SendData(node, new byte[1], ControllerSessionsContainer.Config.TxOptions, null, null);
            using (ReportAction("Send NOP", token))
            {
                token.WaitCompletedSignal();
            }
        }

        public void RequestNodeInfo(NodeTag node, ActionToken token)
        {
            if (ApplicationModel.SelectedNode.Item.EndPointId == 0)
            {
                var caption = "Request Node information from the network";
                token = _device.NodeInfo(node, null);
                using (var logAction = ReportAction(caption, token))
                {
                    var nodeInfoResult = (NodeInfoResult)token.WaitCompletedSignal();
                    var ret = nodeInfoResult.RequestNodeInfo;
                    if (ret && ret.Generic != 0)
                    {
                        //TODO: Review, Maybe needed update NIF from protocol - will fix props1-3 (Case: Slave in the Cotroller secition after update smt.)
                        //var protocolResult = (_device as Controller).GetProtocolInfo(node);
                        //_device.Network.SetNodeInfo(node, protocolResult.NodeInfo);
                        _device.Network.SetNodeInfo(node, ret.Basic, ret.Generic, ret.Specific);
                        _device.Network.SetCommandClasses(node, ret.CommandClasses?.ToArray());
                        _device.Network.SetSecureCommandClasses(node, ret.SecureCommandClasses?.ToArray());
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeInfoCommand), Message = "Node Info Received" });
                        });
                        ApplicationModel.AssociationsModel.UpdateAssociativeDevice(node);
                        if (nodeInfoResult.RequestEndPointCapabilities != null)
                        {
                            AddEndPointsForMultiChannelNode(node, nodeInfoResult.RequestEndPointCapabilities);
                        }
                    }
                    else
                    {
                        logAction.State = ActionStates.Failed;
                    }
                }
            }
            else
            {
                RequestEndPointCapability(node, token);
            }
        }

        private void RequestEndPointCapability(NodeTag node, ActionToken token)
        {
            token = null;
            ApplicationModel.SetBusyMessage("Request Node information from the network.");
            Logger.Log("Request Node information from the network Started.");

            var capability = GetEndPointCapability(node, token);
            byte[] secureSupportedCC = null;
            if (_device.Network.HasSecurityScheme(node, SecuritySchemeSet.ALLS2))
            {
                COMMAND_CLASS_SECURITY_2.SECURITY_2_COMMANDS_SUPPORTED_GET cmd = new COMMAND_CLASS_SECURITY_2.SECURITY_2_COMMANDS_SUPPORTED_GET();
                byte[] rptData = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                var result = RequestData(node, cmd, ref rptData, _device.Network.RequestTimeoutMs, token);
                if (result)
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = rptData;
                    secureSupportedCC = encaped.parameter.ToArray();
                }

            }
            else if (_device.Network.HasSecurityScheme(node, SecuritySchemes.S0))
            {
                COMMAND_CLASS_SECURITY.SECURITY_COMMANDS_SUPPORTED_GET cmd = new COMMAND_CLASS_SECURITY.SECURITY_COMMANDS_SUPPORTED_GET();
                byte[] rptData = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
                var result = RequestData(node, cmd, ref rptData, _device.Network.RequestTimeoutMs, token);
                if (result)
                {
                    COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP encaped = rptData;
                    secureSupportedCC = encaped.parameter.Skip(1).TakeWhile(x => x != 0xEF).ToArray();
                }
            }

            _device.Network.SetNodeInfo(node, capability.genericDeviceClass, capability.specificDeviceClass);
            _device.Network.SetCommandClasses(node, capability.commandClass?.ToArray());
            _device.Network.SetSecureCommandClasses(node, secureSupportedCC);
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeInfoCommand), Message = "Node Info Received" });
            });
            Logger.LogOk("Request Node information from the network completed.");
        }

        public ActionResult ReplaceFailedNode(NodeTag node, ActionToken token)
        {
            var caption = "Replace Failed Node";
            var busyText = "Replacing the non-responding node. Press shortly the pushbutton on the replacement node to be used instead of the failed one.";
            StopSmartListener();
            token = (_device as Controller).ReplaceFailedNode(node, null);
            using (var logAction = ReportAction(caption, busyText, token))
            {
                var ret = ((InclusionResult)token.WaitCompletedSignal());
                if (ret.AddRemoveNode != null && ((ret.AddRemoveNode && ret.AddRemoveNode.Id > 0) ||
                    (ret.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.None)))
                {
                    if (ret.AddRemoveNode.SubstituteStatus == SubstituteStatuses.Failed)
                    {
                        _device.Network.ResetSecuritySchemes(ret.AddRemoveNode.Node);
                        _device.Network.SetSecuritySchemesSpecified(ret.AddRemoveNode.Node);
                        logAction.Caption = caption + " (Security Failed)";
                    }
                    _device.Network.SetFailed(node, false);
                    AddNode(ret);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ReplaceFailedCommand), Message = "Failed Replaced" });
                    });
                }
                StartSmartListener();
                return ret;
            }
        }

        public ActionResult RemoveFailedNode(NodeTag node, out ActionToken token)
        {
            var caption = "Remove Failed Node";
            var busyText = "Removing the non-responding node.";
            StopSmartListener();
            token = (_device as Controller).RemoveFailedNodeId(node, null);
            using (ReportAction(caption, busyText, token))
            {
                var ret = token.WaitCompletedSignal();
                if (ret)
                {
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.RemoveNode(node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveFailedCommand), Message = "Failed Removed" });
                    });
                }
                StartSmartListener();
                return ret;
            }
        }

        public ActionResult IsFailedNode(NodeTag node, out ActionToken requestToken)
        {
            var caption = "Is Failed Node Check";
            var busyText = "Requesting Node status.";
            requestToken = (_device as Controller).IsFailedNode(node, null);
            using (ReportAction(caption, busyText, requestToken))
            {
                var ret = (IsFailedNodeResult)requestToken.WaitCompletedSignal();
                if (ret.IsStateCompleted)
                {
                    _device.Network.SetFailed(node, ret.RetValue);
                    Logger.Log(string.Format("Node {0} status - is{1}failed", node.Id, ret.RetValue ? " " : " not "));
                }
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(IsFailedNodeCommand), Message = ret.RetValue ? "Node Marked as Failed" : "Node is Active" });
                });
                return ret;
            }
        }

        public void RemoveNodeNWE(out ActionToken token)
        {
            var caption = "Remove Node";
            var busyText = "Network Wide Exclusion. Controller is waiting for the node information. Press 'Abort' button when the NWE operation is completed.";
            StopSmartListener();
            token = null;
            bool isCanceled = false;
            while (!isCanceled)
            {
                token = (_device as Controller).ExcludeNode(Modes.NodeAny | Modes.NodeOptionNormalPower | Modes.NodeOptionNetworkWide, 65000, null);
                using (var logAction = ReportAction(caption, busyText, token))
                {
                    AddRemoveNodeResult result = ((ExclusionResult)token.WaitCompletedSignal()).AddRemoveNode;
                    Thread.Sleep(WAIT_AFTER_ADD_REMOVE_NODE_TIMEOUT); // Wait a little for child action (remove node) to be completed.
                    if (result)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.RemoveNode(result.Node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveNodeCommand), Message = "Node Removed" });
                        });
                    }
                    else if (result.State == ActionStates.Cancelled)
                    {
                        isCanceled = true;
                    }
                    else
                    {
                        logAction.State = result.State;
                    }
                }
            }
            StartSmartListener();
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveNodeFromNetworkWideCommand), Message = "Node from Network Wide Removed" });
        }

        public void AddNodeNWI(out ActionToken token)
        {
            var caption = "Add Node";
            var busyText = "Network Wide Inclusion. Controller is waiting for the node information. Press 'Abort' button when the NWI operation is completed.";
            StopSmartListener();
            token = null;
            bool isCanceled = false;
            while (!isCanceled)
            {
                caption = "Add Node";
                //DSKneededCallback and KEXSetConfirmationCallback
                token = (_device as Controller).IncludeNode(Modes.NodeAny | Modes.NodeOptionNormalPower | Modes.NodeOptionNetworkWide, GetTimeoutValue(true), null);
                using (var logAction = ReportAction(caption, busyText, token))
                {
                    var result = (InclusionResult)token.WaitCompletedSignal();
                    Thread.Sleep(WAIT_AFTER_ADD_REMOVE_NODE_TIMEOUT); //Wait a little for child action (add node) to be completed
                    if (result.AddRemoveNode != null && ((result.AddRemoveNode && result.AddRemoveNode.Id > 0) ||
                    (result.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.None)))
                    {
                        if (result.AddRemoveNode.SubstituteStatus == SubstituteStatuses.Failed)
                        {
                            SecurityManager.SecurityManagerInfo.Network.ResetSecuritySchemes(result.AddRemoveNode.Node);
                            SecurityManager.SecurityManagerInfo.Network.SetSecuritySchemesSpecified(result.AddRemoveNode.Node);
                            logAction.Caption = caption + " (Security Failed)";
                        }
                        ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddNodeCommand), Message = "Node Added to NWI Network" }));
                        AddNode(result);
                        //Update();
                        Thread.Sleep(500); //Wait a little before starting next inclusion to allow all nodes to complete requests.
                    }
                    else
                    {
                        if (result.AddRemoveNode.State == ActionStates.Cancelled)
                        {
                            isCanceled = true;
                        }
                        else if (result.AddRemoveNode.State == ActionStates.Failed)
                        {
                            logAction.State = ActionStates.Failed;
                        }
                    }
                }
                SecurityManager.GenerateNewSecretKeyS2();
                //_csaPinDialog.Close();
            }
            StartSmartListener();
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddNodeToNetworkNWICommand), Message = "NWI Command" });
            });
        }

        public ActionResult AddNode(out ActionToken token)
        {
            return AddNodeWithCustomSettings(SetupNodeLifelineSettings.Default, out token);
        }

        private void AddEndPointsForMultiChannelNode(NodeTag parentNode, List<RequestDataResult> requestEPCapabilities)
        {
            for (byte i = 0; i < requestEPCapabilities.Count; i++)
            {
                byte endPoint = (byte)(i + 1);
                var node = new NodeTag(parentNode.Id, endPoint);
                COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_REPORT rpt = requestEPCapabilities[i].Command;
                var nInfo = _device.Network.GetNodeInfo(parentNode);
                nInfo.Generic = rpt.genericDeviceClass;
                nInfo.Specific = rpt.specificDeviceClass;
                _device.Network.SetNodeInfo(node, nInfo);
                _device.Network.SetCommandClasses(node, rpt.commandClass?.ToArray());
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RequestNodeInfoCommand), Message = "Node Info Received" });
                });
            }
        }

        private COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_REPORT GetEndPointCapability(NodeTag node, ActionToken token)
        {
            COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_REPORT ret = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_REPORT();
            COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_GET cmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_GET();
            cmd.properties1.res = 0x00;
            cmd.properties1.endPoint = node.EndPointId;
            byte[] rptData = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CAPABILITY_REPORT();
            var result = RequestData(node.Parent, cmd, ref rptData, _device.Network.RequestTimeoutMs, token);
            if (result)
            {
                ret = rptData;
            }
            return ret;
        }

        public ActionResult RemoveNode(NodeTag node, out ActionToken token)
        {
            var caption = "Remove Node";
            var busyText = "Controller is waiting for the node information. Press shortly the pushbutton on the node to be excluded from the network.";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                StopSmartListener();
                AddRemoveNodeResult ret;
                if (node.Id != 0)
                {
                    token = (_device as Controller).RemoveNodeIdFromNetwork(Modes.NodeAny, node, 65000, null);
                    ret = (AddRemoveNodeResult)token.WaitCompletedSignal();
                }
                else
                {
                    token = (_device as Controller).ExcludeNode(Modes.NodeAny | Modes.NodeOptionNormalPower, 65000, null);
                    ret = ((ExclusionResult)token.WaitCompletedSignal()).AddRemoveNode;
                }
                logAction.State = ret.State;
                Thread.Sleep(WAIT_AFTER_ADD_REMOVE_NODE_TIMEOUT); // Wait a little for child action (remove node) to be completed
                if (ret)
                {
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.RemoveNode(ret.Node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(RemoveNodeCommand), Message = "Node Removed" });
                    });
                    WakeUpNodesService.WakeUpNodeHealthStatuses.TryRemove(ret.Node, out var container);
                }
                StartSmartListener();
                return ret;
            }
        }

        public ActionResult SendNodeInformation(out ActionToken token)
        {
            var caption = "Send Node Information";
            var busyText = "Sending node information.";
            token = _device.SendNodeInformation(new NodeTag(255), ControllerSessionsContainer.Config.TxOptions, null);
            using (var logAction = ReportAction(caption, busyText, token))
            {
                return token.WaitCompletedSignal();
            }
        }

        private void BusySmartStartCallback(bool isBusy, byte[] dsk, ActionResult res)
        {
            var caption = "Add Node";
            var busyText = "Controller is adding node with Smart Start";
            ApplicationModel.SetBusy(isBusy);
            using (var logAction = ReportAction(caption, busyText, null))
            {
                logAction.State = ActionStates.Running;
                if (res as InclusionResult != null)
                {
                    var result = res as InclusionResult;
                    if (result.AddRemoveNode != null && ((result.AddRemoveNode && result.AddRemoveNode.Id > 0) ||
                    (result.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.None)))
                    {
                        if (result.AddRemoveNode.SubstituteStatus == SubstituteStatuses.Failed)
                        {
                            SecurityManager.SecurityManagerInfo.Network.ResetSecuritySchemes(result.AddRemoveNode.Node);
                            SecurityManager.SecurityManagerInfo.Network.SetSecuritySchemesSpecified(result.AddRemoveNode.Node);
                            logAction.Caption = caption + " (Security Failed)";
                        }
                        else
                        {
                            ApplicationModel.Invoke(() =>
                                ApplicationModel.ConfigurationItem.PreKitting.UpdateProvisioningItem(dsk, result.AddRemoveNode.Node, PreKittingState.Included));
                        }

                        if (result.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.None)
                        {
                            AddNode(result);
                        }
                        Update(false);
                    }
                    logAction.State = result.State;
                    ApplicationModel.CsaPinDialog.Close();
                }
            }
        }

        private NodeProvision? LookupForDsk(byte updateStateNodeInfo, byte[] homeId)
        {
            NodeProvision? ret = null;
            if (ApplicationModel.ConfigurationItem.PreKitting != null && ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList != null)
            {
                foreach (var item in ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList)
                {
                    if (item.Dsk != null && item.Dsk.Length >= 12)
                    {
                        if (item.State == PreKittingState.Pending && ((item.Dsk[8] | 0xC0) == homeId[0] &&
                            item.Dsk[9] == homeId[1] &&
                            item.Dsk[10] == homeId[2] &&
                            (item.Dsk[11] & 0xFE) == homeId[3]))
                        {
                            if (updateStateNodeInfo == 0x85 && (item.NodeOptions & (byte)Modes.NodeOptionLongRange) == 0)
                            {
                                ret = new NodeProvision(new NodeTag((byte)item.NodeId), (Modes)item.NodeOptions, item.Dsk, (NetworkKeyS2Flags)item.GrantSchemes);
                                break;
                            }
                            else if (updateStateNodeInfo == 0x87 && (item.NodeOptions & (byte)Modes.NodeOptionLongRange) > 0)
                            {
                                ret = new NodeProvision(new NodeTag((byte)item.NodeId), (Modes)item.NodeOptions, item.Dsk, (NetworkKeyS2Flags)item.GrantSchemes);
                                break;
                            }

                        }
                    }
                }
            }
            return ret;
        }

        public void StartSmartListener()
        {
            if (ApplicationModel.ConfigurationItem.PreKitting != null && ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList != null &&
                ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Count > 0)
            {
                (_device as Controller)?.StartSmartListener(LookupForDsk, BusySmartStartCallback, 0, GetTimeoutValue(true));
            }
        }

        /// <summary>
        /// Stops smart start only if it was running. Delay of SMART_START_STATE_CHANGED_TIMEOUT is added after Stop
        /// </summary>
        public void StopSmartListener()
        {
            // Wait a little for device change it state (Otherwise it can corrupts Transfer Presentation frame).
            (_device as Controller)?.StopSmartListener(SMART_START_STATE_CHANGED_TIMEOUT);
        }

        private void CommandTransmitCallback(byte srcNodeId, byte[] cmd, bool isOutcome, SubstituteIncomingFlags flags)
        {

        }

        public void Update(bool isDefaultCommandClasses)
        {
            ApplicationModel.IsNeedFirstReloadTopology = true;
            _device.SerialApiGetInitData();
            _device.SerialApiGetLRNodes();
            _device.MemoryGetId();
            if (_device is Controller controller)
            {
                controller.GetControllerCapabilities();
                controller.GetSucNodeId();
            }
            else
            {
                ApplicationModel.ConfigurationItem.Node.Clear();
            }
            if (SecurityManager != null)
            {
                ApplicationModel.Invoke(() =>
                {
                    ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys = SecurityManager.SecurityManagerInfo.NetworkKeys;
                    ApplicationModel.ConfigurationItem.AddOrUpdateNode(new NodeTag(_device.Id));
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo);
                });
            }
            InitNodes(isDefaultCommandClasses);
        }
        #endregion

        private ActionToken _wakeupNoMoreToken = null;
        private void SendWakeUpNoMore(NodeTag node, SecuritySchemes scheme)
        {
            if (_device.Network.DelayWakeUpNoMoreInformationMs > 0 && _device.Network.DelayWakeUpNoMoreInformationMs <= 60000)
            {
                Thread.Sleep(_device.Network.DelayWakeUpNoMoreInformationMs);
            }
            _wakeupNoMoreToken = _device.SendDataEx(
                node,
                new COMMAND_CLASS_WAKE_UP.WAKE_UP_NO_MORE_INFORMATION(),
                ControllerSessionsContainer.Config.NonListeningTxOptions,
                scheme,
                null);
        }

        private ActionToken _wakeupSetIntervalToken = null;
        private void SendWakeupSetInterval(NodeTag node, SecuritySchemes scheme)
        {
            _wakeupSetIntervalToken = _device.SendDataEx(
                node,
                new COMMAND_CLASS_WAKE_UP.WAKE_UP_INTERVAL_SET()
                {
                    nodeid = (byte)_device.Id,
                    seconds = Tools.GetBytes(5 * 60).Skip(1).ToArray()
                },
                ControllerSessionsContainer.Config.NonListeningTxOptions,
                scheme,
                null);
        }

        public void StopSupportTasks()
        {
            ApplicationModel.ULMonitorViewModel.StopULMonitor(null);
            if (_basicSupportToken != null)
                _device.Cancel(_basicSupportToken);
            if (_serialApiStartedToken != null)
                _device.Cancel(_serialApiStartedToken);
            if (_powerLevelSupportToken != null)
                _device.Cancel(_powerLevelSupportToken);
            if (_versionGetResponseToken != null)
                _device.Cancel(_versionGetResponseToken);
            if (_versionCapabilitiesGetResponseToken != null)
                _device.Cancel(_versionCapabilitiesGetResponseToken);
            if (_versionCCGetResponseToken != null)
                _device.Cancel(_versionCCGetResponseToken);
            if (_manufacturerSpecificResponseToken != null)
                _device.Cancel(_manufacturerSpecificResponseToken);
            if (_wakeUpResponseToken != null)
                _device.Cancel(_wakeUpResponseToken);
            if (_removeFailedToken != null)
                _device.Cancel(_removeFailedToken);
            if (_deviceResetLocallyResponseToken != null)
                _device.Cancel(_deviceResetLocallyResponseToken);
            if (_configurationSupportToken != null)
                _device.Cancel(_configurationSupportToken);
            if (_ZWavePlusInfoGetResponseToken != null)
                _device.Cancel(_ZWavePlusInfoGetResponseToken);
            if (_associationSupportToken != null)
                _device.Cancel(_associationSupportToken);
            if (_associationGroupInfoSupportToken != null)
                _device.Cancel(_associationGroupInfoSupportToken);
            if (_firmwareUpdateMdSupportToken != null)
                _device.Cancel(_firmwareUpdateMdSupportToken);
            if (_multiChannelSupportToken != null)
                _device.Cancel(_multiChannelSupportToken);
            if (_multiChannelAssociationSupportToken != null)
                _device.Cancel(_multiChannelAssociationSupportToken);
            if (_wakeupSupportToken != null)
                _device.Cancel(_wakeupSupportToken);
            if (_timeSupportToken != null)
                _device.Cancel(_timeSupportToken);
            if (_timeParametersSupportToken != null)
                _device.Cancel(_timeParametersSupportToken);
            WakeUpNodesService.Stop();
        }

        /// <summary>
        /// Z-Wave API Started Command handler (0x0A)
        /// </summary>
        /// <param name="data"></param>
        private void OnSerialApiRestarted(byte[] data)
        {
            var supportedLRtxt = string.Empty;
            if (data != null)
            {
                var idx = 0;
                var wakeupReason = (byte)(data.Length > idx ? data[idx++] : 0); // Wake Up Reason
                var watchdogStarted = (byte)(data.Length > idx ? data[idx++] : 0); // Watchdog Started enum
                var deviceOptionMask = (byte)(data.Length > idx ? data[idx++] : 0); // Device Option Mask
                var genericDeviceType = (byte)(data.Length > idx ? data[idx++] : 0); // Generic Device Type
                var specificDeviceType = (byte)(data.Length > idx ? data[idx++] : 0); // Specific Device Type
                var ccsListLenght = (byte)(data.Length > idx ? data[idx++] : 0); // Command Class List Length
                var ccsList = new byte[ccsListLenght];// Command Class List 1 .. N 
                Array.Copy(data, idx, ccsList, 0, ccsListLenght); // We can allow updating NIF in Set Controller NIF View?
                idx += ccsListLenght;
                var supportedProtocols = (byte)(data.Length > idx ? data[idx++] : 0); // Supported Protocols

                supportedLRtxt = (supportedProtocols & 0x01) == 1 ? "Supports Long Range" : "Doesn't support Long Range";
            }
            Logger.LogWarning($"Serial API restarted. {supportedLRtxt}");
            Task.Run(() =>
            {
                _device?.SerialApiSetNodeIdBaseType(2);
                if (_device is Controller ctrl)
                {
                    var protocolResult = ctrl.GetProtocolInfo(ctrl.Network.NodeTag);
                    _device.Network.SetNodeInfo(ctrl.Network.NodeTag, protocolResult.NodeInfo);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.AddOrUpdateNode(ctrl.Network.NodeTag);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList | NotifyProperty.ControllerInfo);
                    });
                }
                if (ApplicationModel.TraceCapture.TraceCaptureSettingsModel.IsWatchdogEnabled)
                {
                    _device?.WatchDogStart();
                    Logger.LogWarning("Watchdog started.");
                }
                StartSmartListener();
            });
        }

        public void StartSupportTasks()
        {
            ApplicationModel.ULMonitorViewModel.StartULMonitor(null);
            _basicSupportToken = _device.SessionClient.ExecuteAsync(new BasicSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            _serialApiStartedToken = _device.Listen(new ByteIndex[] { 0x00, 0x0A }, OnSerialApiRestarted);
            var controllerDevice = _device;
            if (_device.Network.HasCommandClass(COMMAND_CLASS_MANUFACTURER_SPECIFIC_V2.ID))
            {
                _manufacturerSpecificResponseToken = _device.SessionClient.ExecuteAsync(
                    new ManufacturerSpecificSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions,
                    controllerDevice.ManufacturerId, controllerDevice.ManufacturerProductId, controllerDevice.ManufacturerProductType));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_POWERLEVEL.ID))
            {
                _powerLevelSupportToken = _device.SessionClient.ExecuteAsync(new PowerLevelSupport(_device.Network));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_ASSOCIATION.ID))
            {
                _associationSupportToken = _device.SessionClient.ExecuteAsync(new AssociationSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID))
            {
                _associationGroupInfoSupportToken = _device.SessionClient.ExecuteAsync(new AssociationGroupInfoSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_FIRMWARE_UPDATE_MD.ID))
            {
                FirmwareUpdateMdSupport firmwareUpdateMdSupportTask = ChipTypeSupported.XModem(_device.ChipType) ?
                    (FirmwareUpdateMdSupport)new FirmwareUpdateMdSupportXModem(_device.Network, ControllerSessionsContainer.Config.TxOptions, SetNewImageCompletedCallback, FirmwareUpdateXModemCallback) :
                    new FirmwareUpdateMdSupportBasic(_device.Network, ControllerSessionsContainer.Config.TxOptions, SetNewImageCompletedCallback);
                _firmwareUpdateMdSupportToken = _device.SessionClient.ExecuteAsync(firmwareUpdateMdSupportTask);
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_VERSION.ID))
            {
                _versionGetResponseToken = _device.ResponseDataEx((a, b, c, x) =>
                {
                    ReceiveStatuses receiveStatus = (ReceiveStatuses)a;
                    if (receiveStatus.HasFlag(ReceiveStatuses.TypeBroad) || receiveStatus.HasFlag(ReceiveStatuses.TypeMulti))
                    {
                        return null;
                    }
                    COMMAND_CLASS_VERSION_V2.VERSION_REPORT rt = new COMMAND_CLASS_VERSION_V2.VERSION_REPORT();
                    if (x != null && x.Length >= 2)
                    {
                        rt.zWaveLibraryType = (byte)_device.Library;
                        string version = _device.Version.Replace("Z-Wave ", string.Empty);
                        rt.zWaveProtocolVersion = Convert.ToByte(version.Split('.')[0]);
                        rt.zWaveProtocolSubVersion = Convert.ToByte(version.Split('.')[1]);
                        rt.firmware0Version = controllerDevice.SerialApplicationVersion;
                        rt.firmware0SubVersion = controllerDevice.SerialApplicationRevision;
                        rt.hardwareVersion = controllerDevice.HardwareVersion;
                        rt.numberOfFirmwareTargets = 0;
                    }
                    return rt;
                }, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_VERSION_V2.VERSION_GET());
                _versionCCGetResponseToken = _device.ResponseDataEx((a, b, c, x) =>
                {
                    COMMAND_CLASS_VERSION_V2.VERSION_COMMAND_CLASS_REPORT rt = new COMMAND_CLASS_VERSION_V2.VERSION_COMMAND_CLASS_REPORT();
                    if (x != null && x.Length > 2)
                    {
                        var ccId = x[2];
                        rt.requestedCommandClass = ccId;
                        if (_device.Network.HasCommandClass(ccId) || ccId == COMMAND_CLASS_BASIC_V2.ID)
                        {
                            switch (ccId)
                            {
                                case (COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_ASSOCIATION_GRP_INFO_V3.VERSION;
                                    break;
                                case (COMMAND_CLASS_ASSOCIATION.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_ASSOCIATION_V2.VERSION;
                                    break;
                                case (COMMAND_CLASS_FIRMWARE_UPDATE_MD.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.VERSION;
                                    break;
                                case (COMMAND_CLASS_INCLUSION_CONTROLLER.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_INCLUSION_CONTROLLER.VERSION;
                                    break;
                                case (COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.VERSION;
                                    break;
                                case (COMMAND_CLASS_MULTI_CHANNEL_V4.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_MULTI_CHANNEL_V4.VERSION;
                                    break;
                                case (COMMAND_CLASS_POWERLEVEL.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_POWERLEVEL.VERSION;
                                    break;
                                case (COMMAND_CLASS_VERSION_V2.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_VERSION_V2.VERSION;
                                    break;
                                case (COMMAND_CLASS_WAKE_UP_V2.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_WAKE_UP_V2.VERSION;
                                    break;
                                case (COMMAND_CLASS_TIME_V2.ID):
                                    rt.commandClassVersion = COMMAND_CLASS_TIME_V2.VERSION;
                                    break;
                                default:
                                    rt.commandClassVersion = ApplicationModel.ZWaveDefinition.FindCommandClasses(ccId).Max(cv => cv.Version);
                                    break;
                            }
                        }
                    }
                    return rt;
                }, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_VERSION_V2.VERSION_COMMAND_CLASS_GET());

                _versionCapabilitiesGetResponseToken = _device.ResponseDataEx((a, b, c, x) =>
                {
                    ReceiveStatuses receiveStatus = (ReceiveStatuses)a;
                    if (receiveStatus.HasFlag(ReceiveStatuses.TypeBroad) || receiveStatus.HasFlag(ReceiveStatuses.TypeMulti))
                    {
                        return null;
                    }
                    COMMAND_CLASS_VERSION_V3.VERSION_CAPABILITIES_REPORT rt = new COMMAND_CLASS_VERSION_V3.VERSION_CAPABILITIES_REPORT();
                    if (x != null && x.Length >= 2)
                    {
                        rt.properties1 = new COMMAND_CLASS_VERSION_V3.VERSION_CAPABILITIES_REPORT.Tproperties1
                        {
                            commandClass = 1,
                            version = 1,
                            zWaveSoftware = 0
                        };
                    }
                    return rt;
                }, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_VERSION_V3.VERSION_CAPABILITIES_GET());
            }

            if (_device.Network.HasCommandClass(COMMAND_CLASS_DEVICE_RESET_LOCALLY.ID))
            {
                _deviceResetLocallyResponseToken = _device.ResponseDataEx((a, b, node, x) =>
                {
                    ThreadPool.QueueUserWorkItem((y) =>
                    {
                        Thread.Sleep(1200); //Device is not fast enough to reset itself (S2 behaviour) 600 + Broadcast frame 600
                        var item = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault(i => i.NodeId == node.Id);
                        _device.SendData(node, new byte[1], ControllerSessionsContainer.Config.TxOptions);
                        if (!_device.IsEndDeviceApi)
                        {
                            var removeFailedRes = RemoveFailedNode(node, out _removeFailedToken);
                            if (removeFailedRes && item != null && ApplicationModel.SmartStartModel.IsRemoveDSK)
                            {
                                ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Remove(item));
                            }
                        }
                    });
                    return null;
                }, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_DEVICE_RESET_LOCALLY.DEVICE_RESET_LOCALLY_NOTIFICATION());
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_CONFIGURATION_V4.ID))
            {
                _configurationSupportToken = _device.SessionClient.ExecuteAsync(new ConfigurationSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }

            if (_device.Network.HasCommandClass(COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID))
            {
                _ZWavePlusInfoGetResponseToken = _device.ResponseDataEx((a, b, c, x) =>
                {
                    COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_REPORT rt = new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_REPORT();
                    rt.zWaveVersion = 2;
                    rt.roleType = (byte)_device.Network.GetRoleType();
                    rt.nodeType = (byte)_device.Network.GetNodeType();
                    return rt;
                }, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_GET());
            }

            if (_device.Network.HasCommandClass(COMMAND_CLASS_MULTI_CHANNEL_V4.ID))
            {
                _multiChannelSupportToken = _device.SessionClient.ExecuteAsync(new MultiChannelSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID))
            {
                _multiChannelAssociationSupportToken = _device.SessionClient.ExecuteAsync(new MultiChannelAssociationSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_WAKE_UP_V2.ID))
            {
                _wakeupSupportToken = _device.SessionClient.
                    ExecuteAsync(new WakeupSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_TIME_V2.ID))
            {
                _timeSupportToken = _device.SessionClient.
                    ExecuteAsync(new TimeSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            if (_device.Network.HasCommandClass(COMMAND_CLASS_TIME_PARAMETERS.ID))
            {
                _timeParametersSupportToken = _device.SessionClient.
                    ExecuteAsync(new TimeParametersSupport(_device.Network, ControllerSessionsContainer.Config.TxOptions));
            }
            _wakeUpResponseToken = _device.ResponseData((a) => { WakeUpNodesService.WakeUp(a.SrcNode, a.SecurityScheme, a.Options); return null; },
                        ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_WAKE_UP.WAKE_UP_NOTIFICATION());
            WakeUpNodesService.Start();
        }

        public void SetNodeInformation(out ActionToken token)
        {
            var nodeInfo = _device.Network.GetNodeInfo();
            var commandClasses = _device.Network.GetNetworkAwareCommandClasses();
            if (commandClasses != null && commandClasses.Length > 0)
            {
                _device.ApplicationNodeInformation(nodeInfo.DeviceOptions, nodeInfo.Generic, nodeInfo.Specific, commandClasses);
                var endDevice = _device as EndDevice;
                if (endDevice != null)
                {
                    var commandClassesSecure = _device.Network.GetSecureCommandClasses();
                    endDevice.ApplicationNodeInformationCmdClasses(
                        commandClasses,
                        commandClasses,
                        commandClassesSecure);
                }
            }
            token = null;
            //Invoke
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.AddOrUpdateNode(new NodeTag(_device.Id));
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
            });
        }

        public void SetNodeInformation(DeviceOptions deviceOptions, byte generic, byte specific, byte[] commandClasses, out ActionToken token)
        {
            _device.ApplicationNodeInformation(deviceOptions, generic, specific, commandClasses);
            if (ChipTypeSupported.SoftResetAfterSetNodeInformation(_device.ChipType))
            {
                var expectToken = _device.Expect(new ByteIndex[] { 0x00, 0x0A }, 5000, null);
                _device.SoftReset();
                expectToken.WaitCompletedSignal();
            }
            var ctrlDevice = _device as Controller;
            var endDevice = _device as EndDevice;
            token = null;
            if (ctrlDevice != null)
            {
                _device.Network.SetApplicationNodeInformation(deviceOptions, generic, specific, commandClasses);
                if (ctrlDevice.IncludedNodes != null && ctrlDevice.IncludedNodes.Length == 1)
                {
                    SetDefault(false, out token);
                }
                var res = ctrlDevice.GetProtocolInfo(_device.Network.NodeTag);
                _device.Network.SetNodeInfo(res.NodeInfo);
                _device.Network.SetCommandClasses(commandClasses);
            }
            else if (endDevice != null)
            {
                if (endDevice.Id == 0)
                {
                    ControllerSessionsContainer.Config.DeleteConfiguration(_device);
                    endDevice.SetDefault();
                    endDevice.MemoryGetId();
                }
                _device.Network.SetApplicationNodeInformation(deviceOptions, generic, specific, commandClasses);
            }
            RestartSupportTasks();
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.AddOrUpdateNode(new NodeTag(_device.Id));
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
            });
        }

        private ActionToken _controllerUpdateActionToken;

        public void ControllerUpdateCallback(ApplicationControllerUpdateResult result)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var receivedVal = result.Data != null ? result.Data.GetHex() : string.Empty;
                Logger.Log($"Controller Update {result.Status} from node {result.NodeId} recieved: {receivedVal}");
                var node = new NodeTag(result.NodeId);

                switch (result.Status)
                {
                    case ControllerUpdateStatuses.NewIdAssigned:
                        int lenIndex = _device.Network.IsNodeIdBaseTypeLR ? 3 : 2;
                        if (result.Data != null && result.Data.Length > lenIndex && result.Data.Length > lenIndex + result.Data[lenIndex])
                        {
                            try
                            {
                                NodeInfo nInfo = new NodeInfo();
                                nInfo.Basic = 0;
                                nInfo.Generic = 0;
                                nInfo.Specific = 0;
                                byte[] commandClassesInfo = null;
                                if (result.Data[lenIndex] > 0)
                                {
                                    nInfo.Basic = result.Data[lenIndex + 1];
                                }
                                if (result.Data[lenIndex] > 1)
                                {
                                    nInfo.Generic = result.Data[lenIndex + 2];
                                }
                                if (result.Data[lenIndex] > 2)
                                {
                                    nInfo.Specific = result.Data[lenIndex + 3];
                                }

                                if (nInfo.IsEmpty)
                                {
                                    var protocolResult = (_device as Controller).GetProtocolInfo(node);
                                    _device.Network.SetNodeInfo(node, protocolResult.NodeInfo);
                                }
                                else
                                {
                                    _device.Network.SetNodeInfo(node, nInfo);
                                }
                                if (result.Data[lenIndex] > 3)
                                {
                                    commandClassesInfo = new byte[result.Data[lenIndex] - 3];
                                    Array.Copy(result.Data, lenIndex + 4, commandClassesInfo, 0, result.Data[lenIndex] - 3);
                                    _device.Network.SetCommandClasses(node, commandClassesInfo);
                                }
                                ApplicationModel.Invoke(() =>
                                {
                                    ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                                    ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                                });
                            }
                            catch
                            { }
                        }
                        break;
                    case ControllerUpdateStatuses.DeleteDone:
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.RemoveNode(node);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                        });
                        break;
                    case ControllerUpdateStatuses.SucId:
                        {
                            var sucNodeId = result.NodeId;
                            if (sucNodeId == _device.Id)
                            {
                                _device.SucNodeId = sucNodeId;
                                (_device as Controller).GetControllerCapabilities();
                                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                                StopSmartListener();
                                StartSmartListener();
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// Here should start listener that will listen all log messages from controller 
        /// (to show in log window the results of execution commands)
        /// TODO: must be completed
        /// </summary>
        public void StartListener()
        {
            _listenDataToken = _device.ListenData((x) =>
            {
                LogCommand(x.SrcNode, x.Command, x.SecurityScheme, true);
                if (WakeUpNodesService != null)
                {
                    if (x.Command != null && x.Command.Length > 1 &&
                        (x.Command[0] == COMMAND_CLASS_WAKE_UP.ID && x.Command[1] == COMMAND_CLASS_WAKE_UP.WAKE_UP_NOTIFICATION.ID ||
                        x.Command[0] == COMMAND_CLASS_SECURITY.ID && x.Command[1] == COMMAND_CLASS_SECURITY.SECURITY_NONCE_REPORT.ID ||
                        x.Command[0] == COMMAND_CLASS_SECURITY.ID && x.Command[1] == COMMAND_CLASS_SECURITY.SECURITY_NONCE_GET.ID ||
                        x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_REPORT.ID ||
                        x.Command[0] == COMMAND_CLASS_SECURITY_2.ID && x.Command[1] == COMMAND_CLASS_SECURITY_2.SECURITY_2_NONCE_GET.ID))
                    {
                        //ignore
                    }
                    else
                    {
                        WakeUpNodesService.RxSet(x.SrcNode);
                    }
                }
            });
        }

        public void StopListener()
        {
            if (_listenDataToken != null)
                _device.Cancel(_listenDataToken);
        }

        public void EnqueueCommand(NodeTag node, CommandBase command)
        {
            var ctrl = _device as Controller;
            if (ctrl != null)
            {
                if (ctrl.Network.GetWakeupInterval(node) ||
                    ctrl.NetworkRole.HasFlag(ControllerRoles.SIS) ||
                    (ctrl.NetworkRole.HasFlag(ControllerRoles.RealPrimary) && ctrl.Network.SucNodeId == 0))
                {
                    WakeUpNodesService.Enqueue(node, command, 200);
                }
                else
                {
                    LogError("Wakeup interval for Current node is not assigned");
                }
            }
            else
            {
                LogError("Command queue supported only by controller library");
            }
        }

        public void SetWakeupDelayed()
        {
            WakeUpNodesService.IsWakeupDelayed = true;
        }

        public void UnSetWakeupDelayed()
        {
            WakeUpNodesService.IsWakeupDelayed = false;
        }

        public void ResponseDelay()
        {
            if (_device.Network.DelayResponseMs > 0)
                Task.Delay(_device.Network.DelayResponseMs).Wait();
        }

        private void AddNode(InclusionResult result)
        {
            var addResult = result.AddRemoveNode;
            var nodeInfoResult = result.NodeInfo;

            var protocolResult = (_device as Controller).GetProtocolInfo(addResult.Node);
            var node = new NodeTag(addResult.Id);
            _device.Network.SetNodeInfo(node, protocolResult.NodeInfo);
            _device.Network.SetCommandClasses(node, nodeInfoResult.RequestNodeInfo.CommandClasses?.ToArray());
            _device.Network.SetSecureCommandClasses(node, nodeInfoResult.RequestNodeInfo.SecureCommandClasses?.ToArray());
            _device.Network.SetRoleType(node, (RoleTypes)addResult.RoleType);
            _device.Network.SetNodeType(node, (NodeTypes)addResult.NodeType);

            if (result.SetWakeUpInterval)
            {
                _device.Network.SetWakeupInterval(node, true);
                var wakeUpIntervalValueInSeconds = result.SetupLifelineResult.WakeUpIntervalValueSeconds;
                _device.Network.SetWakeupIntervalValue(node, wakeUpIntervalValueInSeconds);
                WakeUpNodesService.WakeUpNodeHealthStatuses.AddOrUpdate(node, new WakeUpMonitorContainer { WakeUpInterval = wakeUpIntervalValueInSeconds },
                    (nodeKey, container) =>
                    {
                        container.WakeUpInterval = wakeUpIntervalValueInSeconds;
                        container.LastReceivedTimestamp = DateTime.Now;
                        container.IsAlive = true;
                        return container;
                    });
            }

            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.AddOrUpdateNode(node);
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
            });
            if (nodeInfoResult.RequestEndPointCapabilities != null)
            {
                AddEndPointsForMultiChannelNode(node, nodeInfoResult.RequestEndPointCapabilities);
            }

            //_applicationModel.NotifyPropertiesChanged("SelectedNodes", "SelectedNode", "Controller", "IsEndDevice");
        }

        private RequestDataResult GetNodeRoleTypeInternal(NodeTag node)
        {
            RequestDataResult result = _device.RequestData(
                node,
                new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_GET(),
                ControllerSessionsContainer.Config.TxOptions,
                new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_REPORT(),
                _device.Network.RequestTimeoutMs);
            return result;
        }

        public RoleTypes GetNodeRoleType(NodeTag node)
        {
            RoleTypes ret = RoleTypes.None;
            COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_REPORT zwavePlusInfoReportData = new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_REPORT();
            var res = GetNodeRoleTypeInternal(node);
            if (!res)
            {
                Thread.Sleep(500);
                res = GetNodeRoleTypeInternal(node);
            }
            if (res && res.Command != null)
            {
                zwavePlusInfoReportData = res.Command;
                ret = (RoleTypes)zwavePlusInfoReportData.roleType.Value;
                _device.Network.SetRoleType(node, ret);
                _device.Network.SetNodeType(node, (NodeTypes)zwavePlusInfoReportData.nodeType.Value);
            }

            return ret;
        }

        public int GetTimeoutValue(bool IsInclusion)
        {
            var nodeIds = ApplicationModel.ConfigurationItem.Nodes.Where(x => x.Item.EndPointId == 0).Select(x => x.Item).ToArray();
            return _device.Network.GetTimeoutValue(nodeIds, IsInclusion);
        }

        public byte[] EncapData(byte[] data, byte destinationEndPoint)
        {
            if (destinationEndPoint > 0)
            {
                COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP multiChannelCmd = new COMMAND_CLASS_MULTI_CHANNEL_V4.MULTI_CHANNEL_CMD_ENCAP();
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

        public void CancelFirmwareUpdateOTADownload()
        {
            ApplicationModel.SetBusyMessage("OTA Firmware Download cancelling.");
            ApplicationModel.FirmwareUpdateModel.DownloadFirmwareData = null;
            _isDownloadCancelled = true;
        }

        byte SERIAL_API_SETUP_CMD_NETWORK_MANAGEMENT = 1 << 4;
        byte CMD_NETWORK_MANAGEMENT_CMD_INIF_SET_USER_INPUT = 1;
        byte CMD_NETWORK_MANAGEMENT_CMD_INIF_GET_USER_INPUT = 2;
        public CommandExecutionResult SetInifUserInput(byte[] uiid, byte[] uilr)
        {
            var ret = CommandExecutionResult.Failed;
            if (uiid != null && uiid.Length > 1 && uilr != null && uilr.Length > 1)
            {
                var res = _device.SerialApiSetup(
                    SERIAL_API_SETUP_CMD_NETWORK_MANAGEMENT,
                    CMD_NETWORK_MANAGEMENT_CMD_INIF_SET_USER_INPUT,
                    uiid[0], uiid[1], uilr[0], uilr[1]);

                if (res)
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult GetInifUserInput(out byte[] uiid, out byte[] uilr)
        {
            var ret = CommandExecutionResult.Failed;
            uiid = null;
            uilr = null;
            var res = _device.SerialApiSetup(
                SERIAL_API_SETUP_CMD_NETWORK_MANAGEMENT,
                CMD_NETWORK_MANAGEMENT_CMD_INIF_GET_USER_INPUT);
            if (res && res.ByteArray != null && res.ByteArray.Length > 5)
            {
                uiid = new byte[] { res.ByteArray[2], res.ByteArray[3] };
                uilr = new byte[] { res.ByteArray[4], res.ByteArray[5] };
                ret = CommandExecutionResult.OK;
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListSet(byte[] DSK, byte grantSchemesMask, byte nodeOptions, ProvisioningListItemData[] itemMetaData, ActionToken token)
        {
            token = null;
            var ret = CommandExecutionResult.Failed;
            if (DSK != null)
            {
                StopSmartListener();
                ProvisioningItem newItem = new ProvisioningItem();
                newItem.Dsk = DSK;
                newItem.GrantSchemes = grantSchemesMask;
                newItem.NodeOptions = nodeOptions;
                newItem.State = PreKittingState.Pending;
                newItem.Metadata = new ObservableCollection<ProvisioningItemExtension>();

                foreach (var metaData in itemMetaData)
                {
                    var ext = new ProvisioningItemExtension()
                    {
                        IsElective = false,
                        Type = (ProvisioningItemExtensionTypes)metaData.Type,
                        Text = metaData.ToStringFromValue(),
                        IsCritical = metaData.IsCritical
                    };
                    newItem.Metadata.Add(ext);
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
                        //exi.State = newItem.State; - MUST stays Pending
                        //exi.Comment = newItem.Comment; - doesn't changes
                    }
                    else
                    {
                        ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Add(newItem);
                    }
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ProvisioningListSetCommand), Message = $"DSK {updStateTxt} to Provisioning List" });
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.RefreshProvisioningList);
                });
                StartSmartListener();
                ret = CommandExecutionResult.OK;
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListDelete(byte[] DSK, ActionToken token)
        {
            token = null;
            var ret = CommandExecutionResult.Failed;
            if (DSK != null)
            {
                var selectedItem = ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.FirstOrDefault(x => x.Dsk != null && x.Dsk.SequenceEqual(DSK));
                if (selectedItem != null)
                {
                    var nid = (byte)selectedItem.NodeId;
                    if (nid > 0)
                    {
                        IDialog dialog = ApplicationModel.CreateEmptyUserInputDialog("Provisioning list item delete",
                            $"The removed node {nid} will stay in the network until reset or manually excluded");
                        var state = ((IUserInputDialog)dialog).State;
                        state.IsCancelButtonVisible = true;
                        if (dialog.ShowDialog())
                        {
                            ApplicationModel.Invoke(() => ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Remove(selectedItem));
                            ret = CommandExecutionResult.OK;
                        }
                        else
                        {
                            ret = CommandExecutionResult.Canceled;
                        }
                    }
                    else
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Remove(selectedItem);
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ProvisioningListDeleteCommand), Message = "DSK Removed from Provisioning List" });
                            ApplicationModel.NotifyControllerChanged(NotifyProperty.RefreshProvisioningList);
                        });
                        ret = CommandExecutionResult.OK;
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult ProvisioningListClear(ActionToken token)
        {
            token = null;
            var ret = CommandExecutionResult.Failed;
            if (ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Any(x => x.NodeId > 0))
            {
                IDialog dialog = ApplicationModel.CreateEmptyUserInputDialog("Provisioning list clear", "The removed nodes will stay in the network until reset or manually excluded");
                dialog.ShowDialog();
            }
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Clear();
                ApplicationModel.SmartStartModel.ResetFields();
                ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(ProvisioningListClearCommand), Message = "Provisioning List Cleared" });
                ApplicationModel.NotifyControllerChanged(NotifyProperty.RefreshProvisioningList);
            });
            ret = CommandExecutionResult.OK;
            return ret;
        }

        public CommandExecutionResult ProvisioningListGet()
        {
            //useless code!
            //var ret = CommandExecutionResult.Failed;
            //if (ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Count > 0)
            //{
            //    foreach (var item in ApplicationModel.ConfigurationItem.PreKitting.ProvisioningList.Select(x => x.Dsk))
            //    {
            //        ret = ProvisioningListGet(item);
            //    }
            //}
            //return ret;
            return CommandExecutionResult.OK;
        }

        public CommandExecutionResult ProvisioningListGet(byte[] DSK)
        {
            return CommandExecutionResult.OK;
        }

        public void SetNewImageCompletedCallback(bool isUpdated)
        {
#if NETCOREAPP
            if (isUpdated && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                ApplicationModel.NotifyControllerChanged(NotifyProperty.RedirectToHome, "Redirecting to Home Page...");
                ControllerSessionsContainer.Remove(DataSource.SourceId);
                return;
            }
#endif
            ApplicationModel.SetBusy(true);
            ApplicationModel.SetBusyMessage("SoftReset...");
            ApplicationModel.IsErrorMessagePopupDisabled = true;
            _device.SoftReset();
            Update(true);
            ApplicationModel.SetBusyMessage("Disconnecting...");
            Disconnect();
            Task.Delay(10000).Wait();
            for (int i = 0; i < 5; i++)
            {
                var busyMsg = i == 0 ? "Connecting..." : $"Reconnecting attempt {i + 1}/5...";
                ApplicationModel.SetBusyMessage(busyMsg);
                Connect(DataSource);
                Task.Delay(1500).Wait();
                if (_device != null)
                {
                    Logger.LogOk($"Connected to {_device.DataSource.SourceName} after Firmware Update to {_device.Version}");
                    break;
                }
                Task.Delay(1500).Wait();
            }
            ApplicationModel.IsErrorMessagePopupDisabled = false;
            ApplicationModel.SetBusy(false);
        }

        public FirmwareUpdateXModemResult FirmwareUpdateXModemCallback(byte[] hexData)
        {
            var res = new FirmwareUpdateXModemResult();
            _device.Disconnect();
            ApplicationModel.SetBusy(true);
            var xmodemAppLayer = ControllerSessionsContainer.ControllerSessionCreator.CreateXModemApplicationLayer(_sessionLayer, _device.DataSource);
            xmodemAppLayer.TransportLayer.DataTransmitted += ApplicationModel.TraceCapture.SaveTransmittedData;
            using (var xModemDevice = xmodemAppLayer.CreateXModem())
            {
                bool xmodemSessionReady = false;
                if (xModemDevice.Connect(DataSource) == CommunicationStatuses.Done)
                {
                    xModemDevice.TransportClient.WriteData(new byte[] { 0x01, 0x03, 0x00, 0x27, 0xdb }); // Send firmware update init frame.
                    if (xModemDevice.WaitUpdateSessionReady(5000))
                    {
                        xmodemSessionReady = true;
                    }
                }
                if (xmodemSessionReady)
                {
                    List<byte[]> firmwareData = Tools.ArraySplit(hexData, XModemDataFrame.PayloadSize);
                    int packetsCount = 0;
                    foreach (var packetData in firmwareData)
                    {
                        if (xModemDevice.Send(packetData))
                        {
                            packetsCount++;
                            ApplicationModel.SetBusyMessage("Please wait until the Local Firmware Update completed.\n" +
                                $"packet# {packetsCount} of {firmwareData.Count} written.", packetsCount == 0 || packetsCount % 100 == 0 ? (int)(Math.Round((double)packetsCount / firmwareData.Count, 2) * 100) : -1);
                        }
                        else
                            break;
                    }
                    res.UpdateStatus = xModemDevice.ConfirmUpdate(5000);
                    if (res.UpdateStatus)
                    {
                        Task.Delay(1000).Wait(); // Wait for Serial API app run.
                        Logger.Log("Firmware Update Locally succeeded.");
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(FirmwareUpdateLocalCommand), Message = "Firmware Update Locally succeeded." });
                    }
                    else
                    {
                        res.IsKeyValidationFailed = xModemDevice.IsKeyValidationFailed;
                        LogError(xModemDevice.ErrorMessage);
                    }
                }
                else
                {
                    LogError("Firmware update failed: can't open XModem session.");
                }
            }
            xmodemAppLayer.TransportLayer.DataTransmitted -= ApplicationModel.TraceCapture.SaveTransmittedData;
            ApplicationModel.SetBusy(false);
            _device.Connect();
            return res;
        }

        public CommandExecutionResult SetRFReceiveMode(bool isEnabled)
        {
            var caption = "Set RF Receiver Mode";
            var busyText = "The controller sets RF Receiver mode";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                byte val = (byte)(isEnabled ? 1 : 0);
                logAction.State = _device.SetRFReceiveMode(val).State;
            }
            return CommandExecutionResult.OK;
        }

        public CommandExecutionResult SetSleepMode(SleepModes mode, byte intEnable)
        {
            var caption = "Set Sleep Mode";
            var busyText = "The controller sets Sleep mode";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                logAction.State = _device.SetSleepMode(mode, intEnable).State;
            }
            return CommandExecutionResult.OK;
        }

        public CommandExecutionResult GetDefaultTxPowerLevel(out short normalTxPower, out short measured0dBmPower)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            normalTxPower = 0;
            measured0dBmPower = 0;
            if (ChipTypeSupported.TransmitSettings(_device.ChipType))
            {
                var res = _device.ExtendedSetupSupportedSubCommands.Contains(0x013) ?
                    _device.GetDefaultTxPowerLevel16Bit() :
                    _device.GetDefaultTxPowerLevel();
                if (res)
                {
                    normalTxPower = res.NormalTxPower;
                    measured0dBmPower = res.Measured0dBmPower;
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult SetDefaultTxPowerLevel(short normalTxPower, short measured0dBmPower)
        {
            var ret = CommandExecutionResult.Failed;
            if (ChipTypeSupported.TransmitSettings(_device.ChipType))
            {
                var res = _device.ExtendedSetupSupportedSubCommands.Contains(0x012) ?
                    _device.SetDefaultTxPowerLevel16Bit(normalTxPower, measured0dBmPower) :
                    _device.SetDefaultTxPowerLevel((sbyte)normalTxPower, (sbyte)measured0dBmPower);
                if (res)
                {
                    _device.SoftReset();
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult GetRfRegion(out RfRegions rfRegion)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            rfRegion = RfRegions.Undefined;
            var res = _device.GetRfRegion();
            if (res)
            {
                rfRegion = res.RfRegion;
                ret = CommandExecutionResult.OK;
            }
            return ret;
        }

        public CommandExecutionResult SetRfRegion(RfRegions rfRegion)
        {
            if (_device.SetRfRegion(rfRegion))
            {
                var expectToken = _device.Expect(new ByteIndex[] { 0x00, 0x0A }, 5000, null);
                var caption = "Set Rf Region";
                var busyText = "Set Rf Region: " + rfRegion;
                using (var logAction = ReportAction(caption, busyText, expectToken))
                {
                    _device.SoftReset();
                    expectToken.WaitCompletedSignal();
                }
                return CommandExecutionResult.OK;
            }
            return CommandExecutionResult.Failed;
        }

        public CommandExecutionResult GetMaxPayloadSize()
        {
            var ret = CommandExecutionResult.Failed;

            if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSerialApiSetup))
            {
                var maxPayloadSizeRes = _device.GetMaxPayloadSize();
                if (maxPayloadSizeRes && maxPayloadSizeRes.MaxPayloadSize > 0)
                {
                    ret = CommandExecutionResult.OK;
                }
                Logger.Log($"Max Payload Size updated = {_device.Network.TransportServiceMaxSegmentSize}");
                if (ApplicationModel.TransmitSettingsModel.IsRfRegionLR)
                {
                    var maxLRPayloadSizeRes = _device.GetMaxLRPayloadSize();
                    if (maxLRPayloadSizeRes && maxLRPayloadSizeRes.MaxPayloadSize > 0)
                    {
                        ret = CommandExecutionResult.OK;
                    }
                }
                Logger.Log($"Max LR Payload Size updated = {_device.Network.TransportServiceMaxLRSegmentSize}");
            }
            return ret;
        }

        public CommandExecutionResult SetDcdcMode(DcdcModes dcdcMode)
        {
            var ret = CommandExecutionResult.Failed;
            var caption = $"Set DCDC Mode: {dcdcMode}";
            var busyText = $"Trying {caption}...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSetDcdcMode))
                {
                    var res = _device.SetDcdcMode(dcdcMode);
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                    LogError(logAction.Caption);
                }
            }
            return ret;
        }

        public CommandExecutionResult GetDcdcMode()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Get DCDC Mode";
            var busyText = $"Trying {caption}...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetDcdcMode))
                {
                    var res = _device.GetDcdcMode();
                    ApplicationModel.TransmitSettingsModel.DcdcMode = res.DcdcMode;
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = $"{caption} unsupported";
                    ApplicationModel.TransmitSettingsModel.DcdcMode = DcdcModes.Auto;
                }
            }
            return ret;
        }

        public CommandExecutionResult SetMaxLrTxPower(short value)
        {
            var ret = CommandExecutionResult.Failed;
            var caption = $"Set Max LR Tx Power: {value}";
            var busyText = $"Trying {caption}...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (ChipTypeSupported.TransmitSettings(_device.ChipType))
                {
                    var res = _device.SetMaxLrTxPower(value);
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = $"{caption} unsupported";
                    ApplicationModel.TransmitSettingsModel.MaxLrTxPowerMode = 0;
                }
            }
            return ret;
        }

        public CommandExecutionResult GetMaxLrTxPower(out short value)
        {
            var ret = CommandExecutionResult.Failed;
            value = (short)MaxLrTxPowerModes.Undefined;
            if (ChipTypeSupported.TransmitSettings(_device.ChipType))
            {
                var res = _device.GetMaxLrTxPower();
                if (res)
                {
                    value = res.Value;
                    ret = CommandExecutionResult.OK;
                }
            }
            return ret;
        }

        public CommandExecutionResult GetLRChannel()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Get LR Channel";
            var busyText = "Trying Get LR Channel...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetLRChannel))
                {
                    var res = _device.GetLRChannel();
                    ApplicationModel.TransmitSettingsModel.LRChannel = res.Channel;
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = $"{caption} unsupported";
                }
            }
            return ret;
        }

        public CommandExecutionResult SetLRChannel(LongRangeChannels channel)
        {
            var ret = CommandExecutionResult.Failed;
            var caption = $"Set LR Channel: {channel}";
            var busyText = $"Tying to {caption}...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdSetLRChannel))
                {
                    var res = _device.SetLRChannel(channel);
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                    LogError(logAction.Caption);
                }
            }
            return ret;
        }

        public CommandExecutionResult IsRadioPTI()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Get Is Radio PTI enabled";
            var busyText = $"Trying Get {caption}...";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetRadioPTI))
                {
                    var res = _device.IsRadioPTI();
                    ApplicationModel.TransmitSettingsModel.IsRadioPTIEnabled = res && res.IsEnabled;
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = $"{caption} unsupported";
                }
            }
            return ret;
        }


        public CommandExecutionResult EnableRadioPTI(bool isEnabled)
        {
            var ret = CommandExecutionResult.Failed;
            var caption = $"{(isEnabled ? "Enable" : "Disable")} Radio PTI";
            var busyText = $"Tying to {caption}...";
            var expectToken = _device.Expect(new ByteIndex[] { 0x00, 0x0A }, 5000, null);
            using (var logAction = ReportAction(caption, busyText, expectToken))
            {
                if (_device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdEnableRadioPTI))
                {
                    var res = _device.EnableRadioPTI(isEnabled);
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;

                    _device.SoftReset();
                    expectToken.WaitCompletedSignal();
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                    LogError(logAction.Caption);
                }
            }
            return ret;
        }

        public CommandExecutionResult ClearNetworkStats()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Clear Network Stats";
            var busyText = "Trying Clear Network Stats..";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (ChipTypeSupported.NetworkStatistics(_device.ChipType) &&
                    _device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdClearNetworkStats))
                {
                    var res = _device.ClearNetworkStats();
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.NetworkStatisticsModel.RFTxFrames = 0;
                        ApplicationModel.NetworkStatisticsModel.RFRxFrames = 0;
                        ApplicationModel.NetworkStatisticsModel.RFTxLBTBackOffs = 0;
                        ApplicationModel.NetworkStatisticsModel.RFRxLRCErrors = 0;
                        ApplicationModel.NetworkStatisticsModel.RFRxCRC16Errors = 0;
                        ApplicationModel.NetworkStatisticsModel.RFRxForeignHomeID = 0;
                    });
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                }
            }
            return ret;
        }

        public CommandExecutionResult GetNetworkStats()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Get Network Stats";
            var busyText = "Trying Get Network Stats..";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (ChipTypeSupported.NetworkStatistics(_device.ChipType) &&
                    _device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetNetworkStats))
                {
                    var res = _device.GetNetworkStats();
                    if (res)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.NetworkStatisticsModel.RFTxFrames = res.RFTxFrames;
                            ApplicationModel.NetworkStatisticsModel.RFRxFrames = res.RFRxFrames;
                            ApplicationModel.NetworkStatisticsModel.RFTxLBTBackOffs = res.RFTxLBTBackOffs;
                            ApplicationModel.NetworkStatisticsModel.RFRxLRCErrors = res.RFRxLRCErrors;
                            ApplicationModel.NetworkStatisticsModel.RFRxCRC16Errors = res.RFRxCRC16Errors;
                            ApplicationModel.NetworkStatisticsModel.RFRxForeignHomeID = res.RFRxForeignHomeID;
                        });
                    }
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                }
            }
            return ret;
        }

        public CommandExecutionResult ClearTxTimer()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Clear Tx Timer";
            var busyText = "Trying Clear Tx Timer..";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (ChipTypeSupported.NetworkStatistics(_device.ChipType) &&
                    _device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdClearTxTimer))
                {
                    var res = _device.ClearTxTimers();
                    if (res)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel0 = 0;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel1 = 0;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel2 = 0;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel3 = 0;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel4 = 0;
                        });
                    }
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                }
            }
            return ret;
        }

        public CommandExecutionResult GetTxTimer()
        {
            var ret = CommandExecutionResult.Failed;
            var caption = "Get Tx Timer";
            var busyText = "Trying Get Tx Timer..";
            using (var logAction = ReportAction(caption, busyText, null))
            {
                if (ChipTypeSupported.NetworkStatistics(_device.ChipType) &&
                    _device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetTxTimer))
                {
                    var res = _device.GetTxTimer();
                    if (res)
                    {
                        ApplicationModel.Invoke(() =>
                        {
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel0 = res.TxTimeChannel0;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel1 = res.TxTimeChannel1;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel2 = res.TxTimeChannel2;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel3 = res.TxTimeChannel3;
                            ApplicationModel.NetworkStatisticsModel.TxTimeChannel4 = res.TxTimeChannel4;
                        });
                    }
                    ret = res ? CommandExecutionResult.OK : ret;
                    logAction.State = res.State;
                }
                else
                {
                    logAction.Caption = caption + " unsupported";
                }
            }
            return ret;
        }

        public void SendSerialApi(byte[] data, out ActionToken mToken)
        {
            _device.WriteZW(data);
            mToken = null;
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

        private void InitSubstituteManagers()
        {
            if (_device != null && (!IsEndDeviceLibrary || _device.Library == Libraries.EndDeviceSysTestLib))
            {
                if (_device.Library == Libraries.EndDeviceSysTestLib)
                {
                    var token = _device.SessionClient.ExecuteAsync(new SetSecurityBypassOperation(0x01));
                    var rest = token.WaitCompletedSignal();
                }

                var crc16EncapManager = new Crc16EncapManager(_device.Network);
                _device.SessionClient.AddSubstituteManager(crc16EncapManager);
                var supervisionManager = new SupervisionManager(_device.Network, (node, data) =>
                {
                    if (node.Id > 0x00 && node.Id != 0xFF)
                    {
                        if (_device.Network.HasCommandClass(node, COMMAND_CLASS_SUPERVISION.ID))
                        {
                            bool isRequestCmd = false;
                            var command = ApplicationModel.ZWaveDefinition.ParseApplicationStringName(data);
                            if (!string.IsNullOrEmpty(command))
                            {
                                isRequestCmd = command.EndsWith("_GET", StringComparison.InvariantCultureIgnoreCase);
                            }
                            return true && !isRequestCmd;
                        }
                    }
                    return false;
                });
                supervisionManager.SetWakeupDelayed = SetWakeupDelayed;
                supervisionManager.UnSetWakeupDelayed = UnSetWakeupDelayed;
                supervisionManager.ResponseDelay = ResponseDelay;
                _device.SessionClient.AddSubstituteManager(supervisionManager, new SupervisionReportTask(Controller.Network,
                    ControllerSessionsContainer.Config.TxOptions));
                SupervisionManager = supervisionManager;

                InitSecurityManager();

                var transportServiceManagerInfo = new TransportServiceManagerInfo(ControllerSessionsContainer.Config.TxOptions,
                    node =>
                    {
                        return node.Id > 0x00 && _device.Network.HasCommandClass(node, COMMAND_CLASS_TRANSPORT_SERVICE_V2.ID);
                    });

                var transportServiceManager = new TransportServiceManager(_device.Network, transportServiceManagerInfo);
                _device.SessionClient.AddSubstituteManager(transportServiceManager);
                TransportServiceManager = transportServiceManager;
            }
        }

        private void LoadConfigurationItem()
        {
            var securitySettingsModel = ApplicationModel.SecuritySettingsModel;
            var configurationItem = ConfigurationItem.Load(_device.Network, ApplicationModel.CreateSelectableItem);
            if (configurationItem.SecuritySettings.NetworkKey != null)
            {
                foreach (var item in configurationItem.SecuritySettings.NetworkKey)
                {
                    configurationItem.SecuritySettings.NetworkKeys[item.Class] = item;
                }
            }

            securitySettingsModel.TestS0Settings = XmlUtility.XmlStr2Obj<TestS0Settings>(XmlUtility.Obj2XmlStr(configurationItem.SecuritySettings.TestS0Settings));
            var settM = securitySettingsModel.TestS0Settings;
            var settC = configurationItem.SecuritySettings.TestS0Settings;

            securitySettingsModel.IsClientSideAuthS2Enabled = _device.Network.IsCsaEnabled = configurationItem.SecuritySettings.IsCsaEnabled;
            securitySettingsModel.IsEnabledSecurityS0 = _device.Network.IsEnabledS0 = configurationItem.SecuritySettings.IsEnabledS0;
            securitySettingsModel.IsEnabledSecurityS2_UNAUTHENTICATED = _device.Network.IsEnabledS2_UNAUTHENTICATED = configurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED;
            securitySettingsModel.IsEnabledSecurityS2_AUTHENTICATED = _device.Network.IsEnabledS2_AUTHENTICATED = configurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED;
            securitySettingsModel.IsEnabledSecurityS2_ACCESS = _device.Network.IsEnabledS2_ACCESS = configurationItem.SecuritySettings.IsEnabledS2_ACCESS;

            securitySettingsModel.TestNetworkKeys[7].IsSet = configurationItem.SecuritySettings.NetworkKeys[7].TestValue != null;
            securitySettingsModel.TestNetworkKeys[7].Value = configurationItem.SecuritySettings.NetworkKeys[7].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[7].TestValue : new byte[16];
            securitySettingsModel.TestNetworkKeys[0].IsSet = configurationItem.SecuritySettings.NetworkKeys[0].TestValue != null;
            securitySettingsModel.TestNetworkKeys[0].Value = configurationItem.SecuritySettings.NetworkKeys[0].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[0].TestValue : new byte[16];
            securitySettingsModel.TestNetworkKeys[1].IsSet = configurationItem.SecuritySettings.NetworkKeys[1].TestValue != null;
            securitySettingsModel.TestNetworkKeys[1].Value = configurationItem.SecuritySettings.NetworkKeys[1].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[1].TestValue : new byte[16];
            securitySettingsModel.TestNetworkKeys[2].IsSet = configurationItem.SecuritySettings.NetworkKeys[2].TestValue != null;
            securitySettingsModel.TestNetworkKeys[2].Value = configurationItem.SecuritySettings.NetworkKeys[2].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[2].TestValue : new byte[16];

            securitySettingsModel.TestNetworkKeys[3].IsSet = configurationItem.SecuritySettings.NetworkKeys[3].TestValue != null;
            securitySettingsModel.TestNetworkKeys[3].Value = configurationItem.SecuritySettings.NetworkKeys[3].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[3].TestValue : new byte[16];
            securitySettingsModel.TestNetworkKeys[4].IsSet = configurationItem.SecuritySettings.NetworkKeys[4].TestValue != null;
            securitySettingsModel.TestNetworkKeys[4].Value = configurationItem.SecuritySettings.NetworkKeys[4].TestValue != null ? configurationItem.SecuritySettings.NetworkKeys[4].TestValue : new byte[16];

            byte delay = 15;
            byte scheme = 20;

            settM.DelayNetworkKeySet = settC.DelayNetworkKeySetSpecified ? settC.DelayNetworkKeySet : delay;
            settM.DelayNetworkKeyVerify = settC.DelayNetworkKeyVerifySpecified ? settC.DelayNetworkKeyVerify : delay;
            settM.DelayNonceGet = settC.DelayNonceGetSpecified ? settC.DelayNonceGet : delay;
            settM.DelayNonceReport = settC.DelayNonceReportSpecified ? settC.DelayNonceReport : delay;
            settM.DelayNonceGetIN = settC.DelayNonceGetINSpecified ? settC.DelayNonceGetIN : delay;
            settM.DelayNonceReportIN = settC.DelayNonceReportINSpecified ? settC.DelayNonceReportIN : delay;
            settM.DelaySchemeGet = settC.DelaySchemeGetSpecified ? settC.DelaySchemeGet : delay;
            settM.DelaySchemeInherit = settC.DelaySchemeInheritSpecified ? settC.DelaySchemeInherit : delay;
            settM.DelaySchemeReport = settC.DelaySchemeReportSpecified ? settC.DelaySchemeReport : delay;
            settM.DelaySchemeInheritReport = settC.DelaySchemeInheritReportSpecified ? settC.DelaySchemeInheritReport : delay;

            settM.MACInNetworkKeySet = settC.MACInNetworkKeySetSpecified ? settC.MACInNetworkKeySet : new byte[8];
            settM.MACInNetworkKeyVerify = settC.MACInNetworkKeyVerifySpecified ? settC.MACInNetworkKeyVerify : new byte[8];
            settM.ValueInNetworkKeySet = settC.ValueInNetworkKeySetSpecified ? settC.ValueInNetworkKeySet : new byte[16];
            settM.ValueInNetworkKeyVerify = settC.ValueInNetworkKeyVerifySpecified ? settC.ValueInNetworkKeyVerify : new byte[16];
            settM.NonceInNetworkKeySet = settC.NonceInNetworkKeySetSpecified ? settC.NonceInNetworkKeySet : new byte[8];
            settM.NonceInNetworkKeyVerify = settC.NonceInNetworkKeyVerifySpecified ? settC.NonceInNetworkKeyVerify : new byte[8];

            settM.ValueInSchemeGet = settC.ValueInSchemeGetSpecified ? settC.ValueInSchemeGet : scheme;
            settM.ValueInSchemeReport = settC.ValueInSchemeReportSpecified ? settC.ValueInSchemeReport : scheme;
            settM.ValueInSchemeInherit = settC.ValueInSchemeInheritSpecified ? settC.ValueInSchemeInherit : scheme;

            settM.ReuseReceiverNonceInSecondFragment = settC.ReuseReceiverNonceInSecondFragment;

            ApplicationModel.ConfigurationItem = configurationItem;
        }

        private void InitNodes(bool isDefaultCommandClasses)
        {
            var configurationItem = ApplicationModel.ConfigurationItem;
            var ctrlDevice = _device as Controller;
            var brdgDevice = _device as BridgeController;
            var endDevice = _device as EndDevice;
            if (ctrlDevice != null)
            {
                ApplicationModel.Invoke(() => { configurationItem.FillNodes(ctrlDevice.IncludedNodes); });

                var addedNodes = configurationItem.Nodes.ToArray();
                foreach (var item in addedNodes)
                {
                    var node = item.Item;
                    if (_device.Network.GetNodeInfo(node).IsEmpty || _device.Id == node.Id)
                    {
                        var infoRes = ctrlDevice.GetProtocolInfo(node);
                        if (brdgDevice != null)
                        {
                            var virtRes = brdgDevice.IsVirtualNode(node);
                            _device.Network.SetVirtual(node, virtRes.RetValue);
                        }
                        _device.Network.SetNodeInfo(node, infoRes.NodeInfo);
                        var supportedCCs = _device.Network.GetCommandClasses();
                        if (_device.Id == node.Id && (isDefaultCommandClasses || supportedCCs == null))
                        {
                            _device.Network.SetCommandClasses(node, NodeInformationService.GetDefaultCommandClasses());
                        }
                        ApplicationModel.Invoke(() =>
                        {
                            configurationItem.AddOrUpdateNode(node);
                        });
                    }
                    if (_device.Network.GetWakeupInterval(node))
                    {
                        WakeUpNodesService.WakeUpNodeHealthStatuses.TryAdd(node, new WakeUpMonitorContainer
                        {
                            WakeUpInterval = _device.Network.GetWakeupIntervalValue(node) > -1 ? _device.Network.GetWakeupIntervalValue(node) : WakeUpMonitorContainer.DefaultWakeUpIntervalValue
                        });
                    }
                }
            }
            else if (endDevice != null)
            {
                var sdid = endDevice.Id;
                ApplicationModel.Invoke(
                    () => configurationItem.FillNodes(new ushort[]
                    {
                        sdid, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19
                    }.Distinct().Select(x => new NodeTag(x)).ToArray()));
                var addedNodes = configurationItem.Nodes.ToArray();
                foreach (var item in addedNodes)
                {
                    var node = item.Item;
                    if (_device.Network.GetNodeInfo(node).IsEmpty)
                    {
                        var nInfo = new NodeInfo
                        {
                            Capability = 0xD3,
                            Security = 0x1C,
                            Properties1 = 0x01,
                        };
                        _device.Network.SetNodeInfo(node, nInfo);
                        if (_device.Id == node.Id)
                        {
                            var generic = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p => p.Name == "GENERIC_TYPE_METER");
                            var specific = generic.SpecificDevice.FirstOrDefault(p => p.Name == "SPECIFIC_TYPE_NOT_USED");
                            _device.Network.SetNodeInfo(node, generic.KeyId, specific.KeyId);
                            if (_device is Controller && ((Controller)_device).SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdZWaveSendDataEx))
                            {
                                _device.Network.SetCommandClasses(NodeInformationService.GetDefaultCommandClasses().
                                    Where(x =>
                                        x != COMMAND_CLASS_INCLUSION_CONTROLLER.ID).ToArray());
                            }
                            else
                            {
                                _device.Network.SetCommandClasses(NodeInformationService.GetDefaultCommandClasses().
                                    Where(x =>
                                        x != COMMAND_CLASS_INCLUSION_CONTROLLER.ID &&
                                        x != COMMAND_CLASS_SECURITY.ID &&
                                        x != COMMAND_CLASS_SECURITY_2.ID &&
                                        x != COMMAND_CLASS_TRANSPORT_SERVICE_V2.ID).ToArray());
                            }
                            _device.Network.SetRoleType(node, RoleTypes.END_NODE_ALWAYS_ON);
                            _device.Network.SetNodeType(node, NodeTypes.ZWAVEPLUS_NODE);
                        }
                        ApplicationModel.Invoke(() =>
                        {
                            configurationItem.AddOrUpdateNode(node);
                        });
                    }
                }
            }
            configurationItem.RefreshNodes();

            var selfNode = configurationItem.Nodes.Where(x => x.Item.Id == _device.Id);
            if (selfNode.Any())
            {
                ApplicationModel.SelectedNode = selfNode.First();
            }
            ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
        }

        private void LoadIMAFullNetworkConfiguration()
        {
            var configurationItem = ApplicationModel.ConfigurationItem;
            var imaViewSettings = configurationItem.ViewSettings.IMAView;
            if (imaViewSettings != null)
            {
                ApplicationModel.IMAFullNetworkModel.SetLayout(NetworkCanvasLayout.FromString(imaViewSettings.NetworkLayoutString));

                ApplicationModel.IMAFullNetworkModel.UseBackgroundColor = imaViewSettings.UseNetworkBackgroundColor;
                string bgImgPath = imaViewSettings.NetworkBackgroundImagePath;
                if (!System.IO.File.Exists(bgImgPath))
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
        }

        private void InitSecurityManager()
        {
            var securitySettings = ApplicationModel.ConfigurationItem.SecuritySettings;

            _device.Network.SetCommandClassesSecureVirtual(SECURED_COMMAND_CLASSES_VIRTUAL.ToArray());
            SecurityManager sm = new SecurityManager(
                _device.Network,
                securitySettings.NetworkKeys,
                _device.PRK,
                (node, data) =>
                {
                    if (node.Id != 0xFF && data != null && data.Length > 0 && data[0] > 0)
                    {
                        byte commandClassId = data[0];
                        if (NetworkViewPoint.HighSecureCommandClasses.Contains(commandClassId) ||
                            _device.Network.HasSecureCommandClass(node, commandClassId) ||
                            (commandClassId == COMMAND_CLASS_MULTI_CHANNEL_V4.ID && _device.Network.HasSecureCommandClass(node.Parent, commandClassId)))
                        {
                            return true;
                        }
                    }
                    return false;
                },
                (srcNodeId, data, isSubstituted) => { });
            _device.SessionClient.AddSubstituteManager(sm, sm.CreateSecurityReportTask(), sm.CreateSecurityS2ReportTask(),
                sm.CreateInclusionControllerSupportTask(
                    InclusionControllerUpdateCallback,
                    InclusionControllerStatusUpdateCallback));
            SecurityManager = sm;
            SecurityManager.SecurityManagerInfo.CheckIfSupportSecurityCC = true;
            SetupSecurityManagerInfo(securitySettings);
            _device.DSK = sm.SecurityManagerInfo.GetJoinPublicKeyS2();

            // RemoveS2SenderSideCallbacks
            sm.SecurityManagerInfo.DSKNeededCallback = null;
            sm.SecurityManagerInfo.KEXSetConfirmCallback = null;

            if (ApplicationModel.CsaPinDialog is IDialog)
            {
                ApplicationModel.CsaPinDialog.Close();
            }
            sm.SecurityManagerInfo.CsaPinCallback = null;

            // RemoveS2ReceiverSideCallbacks
            sm.SecurityManagerInfo.DSKVerificationOnReceiverCallback = null;

            ApplyS2SenderSideCallbacks();
            ApplyS2ReceiverSideCallbacks();
        }

        private void ApplyS2SenderSideCallbacks()
        {
            var configurationItem = ApplicationModel.ConfigurationItem;
            SecurityManager.SecurityManagerInfo.DSKNeededCallback = (nodeId, dskFull) =>
            {
                byte[] ret = null;
                if (configurationItem.PreKitting.ProvisioningList != null && configurationItem.PreKitting.ProvisioningList.Count > 0)
                {
                    var matchedItem = configurationItem.PreKitting.ProvisioningList.FirstOrDefault(x => x.Dsk != null && x.Dsk.Skip(2).SequenceEqual(dskFull.Skip(2)));
                    if (matchedItem != null)
                    {
                        ret = matchedItem.Dsk.Take(2).ToArray();
                        configurationItem.PreKitting.UpdateProvisioningItem(matchedItem.Dsk, nodeId, PreKittingState.Included);
                    }
                }
                if (ret == null)
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
                    return ApplicationModel.DskNeededDialog.ShowDialog() ? ApplicationModel.DskNeededDialog.GetBytesFromInput() : null;
                }
                return ret;
            };

            SecurityManager.SecurityManagerInfo.KEXSetConfirmCallback = (requestedSchemes, isClientSideAuthRequested) =>
            {
                ApplicationModel.KEXSetConfirmDialog.SetInputParameters(requestedSchemes,
                    isClientSideAuthRequested,
                    _device.Network.HasSecurityScheme(SecuritySchemes.S0),
                    _device.Network.HasSecurityScheme(SecuritySchemes.S2_UNAUTHENTICATED),
                    _device.Network.HasSecurityScheme(SecuritySchemes.S2_AUTHENTICATED),
                    _device.Network.HasSecurityScheme(SecuritySchemes.S2_ACCESS));

                return ApplicationModel.KEXSetConfirmDialog.ShowDialog() ? ApplicationModel.KEXSetConfirmDialog.GetResult() : KEXSetConfirmResult.Default;
            };

            SecurityManager.SecurityManagerInfo.CsaPinCallback = () =>
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    var senderPublicKey = SecurityManager.SecurityManagerInfo.GetPublicKeyS2();
                    ApplicationModel.CsaPinDialog.State.AdditionalText =
                    Tools.GetInt32(senderPublicKey.Take(2).ToArray()).ToString("00000") + " "
                        + Tools.GetInt32(senderPublicKey.Skip(2).Take(2).ToArray()).ToString("00000");
                    ApplicationModel.CsaPinDialog.ShowDialog();
                });
            };
        }

        private void ApplyS2ReceiverSideCallbacks()
        {
            SecurityManager.SecurityManagerInfo.DSKVerificationOnReceiverCallback = () =>
            {
                return ApplicationModel.DskVerificationDialog.ShowDialog() ? ApplicationModel.DskVerificationDialog.GetBytesFromInput(4) : null;
            };
            SecurityManager.SecurityManagerInfo.DskPinCallback = () =>
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    var senderPublicKey = SecurityManager.SecurityManagerInfo.GetJoinPublicKeyS2();
                    ApplicationModel.DskPinDialog.State.AdditionalText = Tools.GetInt32(senderPublicKey.Take(2).ToArray()).ToString("00000");
                    ApplicationModel.DskPinDialog.ShowDialog();
                });
            };
        }

        private bool _isInclusionControllerCancelling;
        private void InclusionControllerStatusUpdateCallback(ActionToken token, bool isCompleted)
        {
            if (!isCompleted)
            {
                CommandBase command = new CommandBase(
                    (x) =>
                    {
                        _isInclusionControllerCancelling = false;
                        for (int i = 0; i < 240 * 5; i++)
                        {
                            if (_isInclusionControllerCancelling)
                            {
                                break;
                            }
                            Task.Delay(200);
                        }
                    },
                    (x) => true,
                    (x) =>
                    {
                        _device.Cancel(token);
                        _isInclusionControllerCancelling = true;
                    },
                    (x) => true);

                //command.MainViewModel = MainModel;
                CommandsFactory.CommandRunner.ExecuteAsync(command, null);
                ApplicationModel.SetBusyMessage("Node setup process is running.");
                ApplicationModel.SetBusy(true);
            }
            else
            {
                _isInclusionControllerCancelling = true;
                ApplicationModel.SetBusy(false);
            }

        }

        private void InclusionControllerUpdateCallback(ActionResult result)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                RequestNodeInfoResult requestResult = null;
                if (result is RequestNodeInfoResult)
                {
                    requestResult = (RequestNodeInfoResult)result;
                }
                else if (result is SetupNodeLifelineResult)
                {
                    var lifelineResult = (SetupNodeLifelineResult)result;
                    requestResult = ((SetupNodeLifelineResult)result).NodeInfo.RequestNodeInfo;
                    if (lifelineResult.SetWakeUpInterval)
                    {
                        _device.Network.SetWakeupInterval(requestResult.Node, true);
                        var wakeUpIntervalValueInSeconds = lifelineResult.WakeUpIntervalValueSeconds;
                        _device.Network.SetWakeupIntervalValue(requestResult.Node, wakeUpIntervalValueInSeconds);
                        WakeUpNodesService.WakeUpNodeHealthStatuses.AddOrUpdate(requestResult.Node, new WakeUpMonitorContainer { WakeUpInterval = wakeUpIntervalValueInSeconds },
                            (nodeKey, container) =>
                            {
                                container.WakeUpInterval = wakeUpIntervalValueInSeconds;
                                container.LastReceivedTimestamp = DateTime.Now;
                                container.IsAlive = true;
                                return container;
                            });
                    }
                    ApplicationModel.AssociationsModel.UpdateAssociativeDevice(requestResult.Node);
                    if (lifelineResult.NodeInfo.RequestEndPointCapabilities != null)
                    {
                        AddEndPointsForMultiChannelNode(requestResult.Node, lifelineResult.NodeInfo.RequestEndPointCapabilities);
                    }
                }
                var ctrlDevice = _device as Controller;
                if (ctrlDevice != null && requestResult != null && requestResult.Node.Id > 0)
                {
                    var protocolResult = ctrlDevice.GetProtocolInfo(requestResult.Node);
                    ctrlDevice.Network.SetNodeInfo(requestResult.Node, protocolResult.NodeInfo);
                    ApplicationModel.Invoke(() =>
                    {
                        ApplicationModel.ConfigurationItem.AddOrUpdateNode(requestResult.Node);
                        ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                    });
                }
            });
        }

        public void SetupSecurityManagerInfo(SecuritySettings securitySettings)
        {
            SecurityManagerInfo smi = SecurityManager.SecurityManagerInfo;
            ConfigurationItem configurationItem = ApplicationModel.ConfigurationItem;

            Logger.Log("Setup Security Manager Info");

            smi.ActivateNetworkKeyS0();
            smi.ClearDelaysS0();
            smi.ClearTestOverridesS0();

            ApplicationModel.HasActiveSecurityTestSettings =
                configurationItem.SecuritySettings.TestS0Settings.IsActive ||
                configurationItem.SecuritySettings.TestS2Settings.IsActive;

            if (configurationItem.SecuritySettings.TestS0Settings.IsActive)
            {
                #region Including Controller

                if (securitySettings.TestS0Settings.NonceInNetworkKeySetSpecified)
                {
                    smi.SetTestReceiverNonceS0(securitySettings.TestS0Settings.NonceInNetworkKeySet);
                }

                if (securitySettings.TestS0Settings.MACInNetworkKeySetSpecified)
                {
                    smi.SetTestMacS0(securitySettings.TestS0Settings.MACInNetworkKeySet);
                }

                if (securitySettings.TestS0Settings.ValueInNetworkKeySetSpecified)
                {
                    smi.SetTestNetworkKeyS0InSet(securitySettings.TestS0Settings.ValueInNetworkKeySet);
                }

                if (securitySettings.TestS0Settings.ValueInSchemeGetSpecified)
                {
                    smi.SetTestSecuritySchemeS0(securitySettings.TestS0Settings.ValueInSchemeGet);
                }

                if (securitySettings.TestS0Settings.ValueInSchemeInheritSpecified)
                {
                    smi.SetTestSecuritySchemeInInheritS0(securitySettings.TestS0Settings.ValueInSchemeInherit);
                }
                if (securitySettings.TestS0Settings.DelaySchemeGetSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.SchemeGet,
                        securitySettings.TestS0Settings.DelaySchemeGet * 1000);
                }
                if (securitySettings.TestS0Settings.DelayNonceGetSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NonceGet,
                        securitySettings.TestS0Settings.DelayNonceGet * 1000);
                }

                if (securitySettings.TestS0Settings.DelayNetworkKeySetSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NetworkKeySet,
                        securitySettings.TestS0Settings.DelayNetworkKeySet * 1000);
                }

                if (securitySettings.TestS0Settings.DelayNonceReportSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NonceReport,
                        securitySettings.TestS0Settings.DelayNonceReport * 1000);
                }
                if (securitySettings.TestS0Settings.DelaySchemeInheritSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.SchemeInherit,
                        securitySettings.TestS0Settings.DelaySchemeInherit * 1000);
                }
                #endregion

                #region Included Node

                if (securitySettings.TestS0Settings.NonceInNetworkKeyVerifySpecified)
                {
                    smi.SetTestReceiverNonceS0(securitySettings.TestS0Settings.NonceInNetworkKeyVerify);
                }

                if (securitySettings.TestS0Settings.MACInNetworkKeyVerifySpecified)
                {
                    smi.SetTestMacS0(securitySettings.TestS0Settings.MACInNetworkKeyVerify);
                }
                if (securitySettings.TestS0Settings.ValueInNetworkKeyVerifySpecified)
                {
                    smi.SetTestNetworkKey(securitySettings.TestS0Settings.ValueInNetworkKeyVerify, SecuritySchemes.S0, false);
                    smi.ActivateNetworkKeyS0();
                }
                if (securitySettings.TestS0Settings.ValueInSchemeReportSpecified)
                {
                    smi.SetTestSecuritySchemeS0(securitySettings.TestS0Settings.ValueInSchemeReport);
                }
                if (securitySettings.TestS0Settings.DelaySchemeReportSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.SchemeReport,
                        securitySettings.TestS0Settings.DelaySchemeReport * 1000);
                }
                if (securitySettings.TestS0Settings.DelayNonceReportINSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NonceReport,
                       securitySettings.TestS0Settings.DelayNonceReportIN * 1000);

                }
                if (securitySettings.TestS0Settings.DelayNonceGetINSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NonceGet,
                       securitySettings.TestS0Settings.DelayNonceGetIN * 1000);
                }
                if (securitySettings.TestS0Settings.DelayNetworkKeyVerifySpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.NetworkKeyVerify,
                        securitySettings.TestS0Settings.DelayNetworkKeyVerify * 1000);
                }
                if (securitySettings.TestS0Settings.DelaySchemeInheritReportSpecified)
                {
                    smi.SetDelayS0(SecurityS0Delays.SchemeReportEnc,
                        securitySettings.TestS0Settings.DelaySchemeInheritReport * 1000);
                }

                #endregion

                smi.SetTestReuseReceiverNonceS0InSecondFragment(securitySettings.TestS0Settings.ReuseReceiverNonceInSecondFragment);
            }

            smi.ClearTestOverridesS2();
            if (securitySettings.TestS2Settings.IsActive)
            {
                TestParametersS2Settings param;
                if (securitySettings.IsParameterS2Set(ParameterS2Type.Span, out param))
                {
                    smi.SetTestSpanS2(param.Value);
                }
                if (securitySettings.IsParameterS2Set(ParameterS2Type.Sender_EI, out param))
                {
                    smi.SetTestSenderEntropyInputS2(param.Value);
                }
                if (securitySettings.IsParameterS2Set(ParameterS2Type.SecretKey, out param))
                {
                    smi.SetTestSecretKeyS2(param.Value);
                }
                if (securitySettings.IsParameterS2Set(ParameterS2Type.SequenceNo, out param))
                {
                    smi.SetTestSequenceNumberS2(param.Value[0]);
                }
                if (securitySettings.IsParameterS2Set(ParameterS2Type.ReservedField, out param))
                {
                    smi.SetTestReservedFieldS2(param.Value[0]);
                }

                if (securitySettings.TestS2Settings.Frames.Count > 0)
                {
                    foreach (var item in configurationItem.SecuritySettings.TestS2Settings.Frames)
                    {
                        if (item.IsEnabled)
                        {
                            smi.SetTestFrameCommandS2(item.FrameTypeV, item.Command);
                            smi.SetTestFrameDelayS2(item.FrameTypeV, item.DelaySpecified ? item.Delay * 1000 : 0);
                            smi.SetTestFrameEncryptedS2(item.FrameTypeV, item.IsEncryptedSpecified ? item.IsEncrypted : (bool?)null);
                            smi.SetTestFrameMulticastS2(item.FrameTypeV, item.IsMulticastSpecified ? item.IsMulticast : (bool?)null);
                            smi.SetTestFrameBroadcastS2(item.FrameTypeV, item.IsBroadcastSpecified ? item.IsBroadcast : (bool?)null);
                            smi.SetTestFrameNetworkKeyS2(item.FrameTypeV, item.NetworkKey, item.IsTemp);
                        }
                    }
                }
                if (securitySettings.TestS2Settings.Extensions.Count > 0)
                {
                    foreach (var item in securitySettings.TestS2Settings.Extensions)
                    {
                        if (item.IsEnabled)
                        {
                            smi.AddTestExtensionS2(item);
                        }
                    }
                }

                if (ApplicationModel.SecuritySettingsModel.TestS2Settings.IsOverrideExistingExtensions)
                {
                    var msgTypes = new[] { MessageTypes.MulticastAll, MessageTypes.SinglecastAll };
                    var extTypes = new[] { ExtensionTypes.Mos, ExtensionTypes.Mpan, ExtensionTypes.MpanGrp, ExtensionTypes.Span };
                    foreach (var msgType in msgTypes)
                    {
                        foreach (var extType in extTypes)
                        {
                            smi.AddTestExtensionS2(new TestExtensionS2Settings(msgType, extType, ExtensionAppliedActions.Delete));
                        }
                    }
                }
            }
            smi.TestEnableClientSideAuthS2 = securitySettings.IsCsaEnabled;
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
                    schemeStr = string.Empty;
                }
                logMsg = string.Format("{0}{1} {2}{3}", isIncome ? "Rx" : "Tx", schemeStr, commandName, cmdInfo);
                ApplicationModel.Invoke(() => { Logger.Log(logMsg, new LogSettings() { Level = Utils.UI.Enums.LogLevels.Text }, new LogRawData() { RawData = data, SecuritySchemes = (byte)scheme, SourceId = srcNode.Id }); });
            }
        }

        public void LogError(Exception exception)
        {
            Logger.LogFail(exception.Message);
            if (!ApplicationModel.IsErrorMessagePopupDisabled)
            {
                ApplicationModel.ShowMessageDialog("Application run into error", exception.Message);
            }
        }

        public void LogError(string errorMessage, params object[] args)
        {
            LogError(new Exception(string.Format(errorMessage, args)));
        }

        public void LogWarning(string message, NodeTag node)
        {
            Controller.Network.SetFailed(node, true);
            Logger.LogWarning(message);
        }

        public void LogOk(string message, NodeTag node)
        {
            Controller.Network.SetFailed(node, false);
            Logger.LogOk(message);
        }

        public void OnDeviceInitFailed()
        {
            LogError("The required Z-Wave controller device was not found.");
        }

        public ActionResult AddNodeWithCustomSettings(SetupNodeLifelineSettings setupNodeLifelineSettings, out ActionToken token)
        {
            var caption = "Add Node";
            var busyText = "Controller is waiting for the node information. Press shortly the pushbutton on the node to be included in the network.";
            StopSmartListener();
            token = (Controller as Controller).IncludeNode(Modes.NodeAny | Modes.NodeOptionNormalPower, GetTimeoutValue(true), null, setupNodeLifelineSettings);
            using (var logAction = ReportAction(caption, busyText, token))
            {
                var ret = (InclusionResult)token.WaitCompletedSignal();
                Task.Delay(WAIT_AFTER_ADD_REMOVE_NODE_TIMEOUT).Wait(); // Wait a little for child action (add node) to be completed
                if (ret.AddRemoveNode != null && ((ret.AddRemoveNode && ret.AddRemoveNode.Id > 0) ||
                    (ret.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.None)))
                {
                    if (ret.AddRemoveNode.SubstituteStatus == SubstituteStatuses.Failed)
                    {
                        SecurityManager.SecurityManagerInfo.Network.ResetSecuritySchemes(ret.AddRemoveNode.Node);
                        SecurityManager.SecurityManagerInfo.Network.SetSecuritySchemesSpecified(ret.AddRemoveNode.Node);
                        logAction.Caption = caption + " (Security Failed)";
                    }
                    if (ret.AddRemoveNode.AddRemoveNodeStatus != AddRemoveNodeStatuses.Replicated)
                    {
                        AddNode(ret);
                    }
                    Update(false);
                    ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(AddNodeCommand), Message = "Node Added" });
                }
                SecurityManager.GenerateNewSecretKeyS2();
                ApplicationModel.CsaPinDialog.Close();
                //TODO: check smart start active after complited custom inclusion: - !doesn't work!
                Thread.Sleep(1000);
                StartSmartListener();
                ApplicationModel.NotifyControllerChanged(NotifyProperty.NodesList);
                return ret;
            }
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
                    mpanTableConfigurationModel.SelectedMpanItem = mpanTableConfigurationModel.FullMpanTableBind.First();
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
                COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_NAME_REPORT getNameRpt = rptData;
                name.Append(Encoding.UTF8.GetString(getNameRpt.name.ToArray()));
                for (int i = getNameRpt.reportsToFollow - 1; i >= 0; i--)
                {
                    var expRes = ExpectData(node, rptData, 500);
                    var res = (ExpectDataResult)expRes.WaitCompletedSignal();
                    if (res)
                    {
                        getNameRpt = res.Command;
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
                    var expRes = ExpectData(node, rptData, 500);
                    var res = (ExpectDataResult)expRes.WaitCompletedSignal();
                    if (res)
                    {
                        getInfoRpt = res.Command;
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
