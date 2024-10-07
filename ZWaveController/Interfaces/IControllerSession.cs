/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using Utils;
using Utils.UI.Interfaces;
using Utils.UI.Logging;
using ZWave;
using ZWave.BasicApplication;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.TransportService;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface IControllerSession
    {
        //ReadOnlyCollection<byte> SECURED_COMMAND_CLASSES_VIRTUAL { get; }
        IApplicationModel ApplicationModel { get; set; }
        IDataSource DataSource { get; }
        IDevice Controller { get; }
        ILogService Logger { get; set; }
        ISenderHistoryService SenderHistoryService { get; set; }
        IPredefinedPayloadsService PredefinedPayloadsService { get; set; }
        IPollingService PollingService { get; }
        IERTTService ERTTService { get; }
        INodeInformationService NodeInformationService { get; }
        ISerialPortMonitor SerialPortMonitor { get; }
        string UserSessionId { get; set; }        
        SecurityManager SecurityManager { get; }
        SupervisionManager SupervisionManager { get; }
        TransportServiceManager TransportServiceManager { get; }
        IMAFullNetwork IMAFullNetwork { get; set; }
        bool IsBridgeControllerLibrary { get; }
        bool IsEndDeviceLibrary { get; }
        CommunicationStatuses Connect(IDataSource dataSource);
        void Disconnect();
        void Update(bool isDefaultCommandClasses);
        void SaveSecurityKeys(bool isClearFile);
        void Cancel(NodeTag cancelNode, ActionToken token);
        ActionResult SendData(NodeTag node, byte[] txData, ActionToken token);
        ActionResult SendData(NodeTag name, byte[] txData, TransmitOptions txOptions, ActionToken token);
        ActionResult SendData(NodeTag node, byte[] data, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false);
        ActionResult SendData(NodeTag[] nodes, byte[] data, int maxBytesPerFrame, SubstituteFlags substituteFlag, ActionToken token, bool isMultiChanelProcessed = false);
        ActionResult RequestData(NodeTag name, byte[] txData, ref byte[] rxData, int timeoutMs, ActionToken token);
        ActionResult RequestData(NodeTag name, byte[] txData, ref byte[] rxData, int timeoutMs, Action SendDataSubstituteCallback, ActionToken token);
        void SendSerialApi(byte[] data, out ActionToken mToken);
        void SetNodeInformation(DeviceOptions deviceOptions, byte generic, byte specific, byte[] commandClasses, out ActionToken token);
        void SetNodeInformation(out ActionToken token);
        ActionResult SendNodeInformation(out ActionToken token);
        ActionResult AddNode(out ActionToken token);
        ActionResult AddNodeWithCustomSettings(SetupNodeLifelineSettings setupNodeLifelineSettings, out ActionToken token);
        ActionResult RemoveNode(NodeTag node, out ActionToken token);
        void AddNodeNWI(out ActionToken token);
        void RemoveNodeNWE(out ActionToken token);
        ActionResult IsFailedNode(NodeTag node, out ActionToken requestToken); //no zip support
        ActionResult RemoveFailedNode(NodeTag node, out ActionToken token);
        ActionResult ReplaceFailedNode(NodeTag node, ActionToken token);
        void RequestNodeInfo(NodeTag node, ActionToken requestToken);
        void SendNop(NodeTag node, out ActionToken requestToken);
        ActionResult SetSucNode(NodeTag node, out ActionToken mToken); // no zip support?
        ActionResult RequestNetworkUpdate(out ActionToken token);
        ActionResult ControllerChange(out ActionToken token);
        ActionResult SetDefault(bool isDefaultCommandClasses, out ActionToken token);
        ActionResult SetLearnMode(LearnModes mode, out ActionToken token);
        CommandExecutionResult SetVirtualDeviceLearnMode(VirtualDeviceLearnModes mode, NodeTag node, out ActionToken token);
        CommandExecutionResult AreNeighbors(NodeTag srcNodeId, NodeTag destNodeId); // no zip support
        CommandExecutionResult GetRoutingInfo(NodeTag node, out NodeTag[] routingNodes); // no zip support
        CommandExecutionResult StartPowerLevelTest(NodeTag node, NodeTag destinationNode, byte powerLevel, ActionToken token);
        CommandExecutionResult RequestNodeNeighborUpdate(NodeTag node, out ActionToken token);
        CommandExecutionResult AssignReturnRoute(NodeTag srcNodeId, NodeTag[] destNodeIds, out ActionToken token);
        CommandExecutionResult AssignSUCReturnRoute(NodeTag srcNodeId, out ActionToken token);
        CommandExecutionResult AssignPriorityReturnRoute(NodeTag srcNodeId, NodeTag destNodeId, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token);
        CommandExecutionResult AssignPrioritySUCReturnRoute(NodeTag srcNodeId, NodeTag repeater0, NodeTag repeater1, NodeTag repeater2, NodeTag repeater3, byte routeSpeed, out ActionToken token);
        CommandExecutionResult DeleteReturnRoute(NodeTag srcNodeId, bool isSUCReturnRoute, out ActionToken token);
        CommandExecutionResult DeleteReturnRoute(NodeTag scrNodeId);
        CommandExecutionResult AssociationGet(NodeTag node, byte groupId);
        CommandExecutionResult AssociationGroupingsGet(NodeTag node);
        CommandExecutionResult AssociationRemove(NodeTag node, byte groupId, IEnumerable<NodeTag> nodeIds);
        CommandExecutionResult AssociationCreate(NodeTag node, byte groupId, IEnumerable<NodeTag> nodeIds);
        CommandExecutionResult AssociationGroupNameGet(NodeTag node, byte groupId);
        CommandExecutionResult AssociationGetGroupInfo(NodeTag node, byte groupId);
        CommandExecutionResult AssociationGetCommandList(NodeTag node, byte groupId);
        CommandExecutionResult FirmwareUpdateOTAGet(NodeTag nodeId);
        void CancelFirmwareUpdateV1(NodeTag node, int waitTimeoutMs);
        void CancelFirmwareUpdateV2(NodeTag node, int waitTimeoutMs);
        void CancelFirmwareUpdateV3(NodeTag node, int waitTimeoutMs);
        void CancelFirmwareUpdateV4(NodeTag node, int waitTimeoutMs);
        void CancelFirmwareUpdateV5(NodeTag node, int waitTimeoutMs);
        void OnMDStatusReportAction(NodeTag node, Func<ReceiveStatuses, NodeTag, NodeTag, byte[], byte[]> responseCallback, ActionToken token);
        byte FirmwareUpdateV1(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, out ActionToken token);
        byte FirmwareUpdateV2(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, out ActionToken token);
        byte FirmwareUpdateV3(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, out ActionToken token);
        byte FirmwareUpdateV4(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, byte activation, out ActionToken token);
        byte FirmwareUpdateV5(NodeTag node, List<byte[]> fwData, byte[] fwChecksum, byte[] fwId, byte[] mfId, byte fwTarget, int fragmentSize, byte activation, byte hardwareVersion, out ActionToken token);
        CommandExecutionResult FirmwareUpdateOTAActivate(NodeTag node, ActionToken token);
        CommandExecutionResult FirmwareUpdateOTADownload(NodeTag node, out ActionToken mToken);
        void CancelFirmwareUpdateOTADownload();
        CommandExecutionResult SetInifUserInput(byte[] uiid, byte[] uilr);
        CommandExecutionResult GetInifUserInput(out byte[] uiid, out byte[] uilr);
        CommandExecutionResult ProvisioningListSet(byte[] DSK, byte GrantSchemesMask, byte nodeOptions, ProvisioningListItemData[] itemMetaData, ActionToken token);
        CommandExecutionResult ProvisioningListDelete(byte[] DSK, ActionToken token);
        CommandExecutionResult ProvisioningListClear(ActionToken token);
        CommandExecutionResult ProvisioningListGet();
        CommandExecutionResult ProvisioningListGet(byte[] DSK);
        CommandExecutionResult SetRFReceiveMode(bool isEnabled);
        CommandExecutionResult SetSleepMode(SleepModes mode, byte intEnable);
        void StopSmartListener();
        void StartSmartListener();
        void LogError(Exception exception);
        void LogError(string errorMessage, params object[] args);
        LogAction ReportAction(string caption, string busyText, ActionToken token, LogRawData logRawData = null);
        void SetupSecurityManagerInfo(SecuritySettings securitySettings);
        void LoadMpan();
        string ConfigurationNameGet(NodeTag node, byte[] parameterNumber, ActionToken _token);
        string ConfigurationInfoGet(NodeTag node, byte[] parameterNumber, ActionToken _token);
        PayloadItem GetPayloadItem(ICommandClassesModel model);
    }
}
