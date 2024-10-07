/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveController.Commands;
using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Services;

namespace ZWaveController.Interfaces
{
    public interface IApplicationModel : IAppLogModel
    {
        IDataSource DataSource { get; set; }
        ZWaveDefinition ZWaveDefinition { get; set; }
        IDevice Controller { get; set; }
        ConfigurationItem ConfigurationItem { get; set; }
        IDispatch Dispatcher { get; set; }
        ObservableCollection<ISelectableItem<QueueItem>> CommandQueueCollection { get; set; }
        bool HasActiveSecurityTestSettings { get; set; }
        bool IsNeedFirstReloadTopology { get; set; }
        bool IsBusy { get; }
        string BusyMessage { get; }
        bool IsActiveSessionZip { get; }
        ISelectableItem<NodeTag> SelectedNode { get; set; }
        CommandBase ActiveCommand { get; set; }
        //ISubscribeCollectionFactory SubscribeCollectionFactory { get; set; }
        string ConnectionName { get; set; }
        CommandExecutionResult LastCommandExecutionResult { get; set; }
        List<GenericDeviceTypeColorItem> GenericDeviceTypeColorItems { get; set; }
        bool IsErrorMessageVisible { get; set; }
        bool IsErrorMessagePopupDisabled { get; set; }


        // Models
        ITraceCapture TraceCapture { get; }
        ICommandClassesModel CommandClassesModel { get; set; }
        ISecuritySettings SecuritySettingsModel { get; }
        IIMAFullNetworkModel IMAFullNetworkModel { get; }
        IEncryptDecryptModel EncryptDecryptModel { get; }
        IAppSettings AppSettings { get; set; }
        IERTTModel ERTTModel { get; set; }
        IULMonitorModel ULMonitorViewModel { get; set; }
        IFirmwareUpdateModel FirmwareUpdateModel { get; }
        IPollingModel PollingModel { get; }
        ISmartStartModel SmartStartModel { get; set; }
        ITransmitSettingsModel TransmitSettingsModel { get; set; }
        INVMBackupRestoreModel NVMBackupRestoreModel { get; }
        ISetupRouteModel SetupRouteModel { get; }
        ITopologyMapModel TopologyMapModel { get; }
        IConfigurationModel ConfigurationModel { get; }
        IAssociationsModel AssociationsModel { get; }
        ISetNodeInformationModel SetNodeInformationModel { get; set; }
        ISendDataSettingsModel SendDataSettingsModel { get; set; }
        ISenderHistoryModel SenderHistoryModel { get; }
        IPredefinedCommandsModel PredefinedCommandsModel { get; }
        INetworkStatisticsModel NetworkStatisticsModel { get; }

        // Dialogs
        IKEXSetConfirmDialog KEXSetConfirmDialog { get; set; }
        IScanDSKDialog DskNeededDialog { get; set; }
        IUserInputDialog DskPinDialog { get; set; }
        IUserInputDialog CsaPinDialog { get; set; }
        INetworkManagementModel NetworkManagementModel { get; }
        IOpenFileDialogModel OpenFileDialogModel { get; set; }
        ISaveFileDialogModel SaveFileDialogModel { get; set; }
        IAboutModel AboutDialog { get; set; }
        IAddSocketSourceModel AddSocketSourceDialog { get; set; }
        ISelectLocalIpAddressModel SelectLocalIpAddressDialog { get; set; }
        IScanDSKDialog DskVerificationDialog { get; set; }
        IMpanTableConfigurationModel MpanTableConfigurationDialog { get; set; }
        IDialog SetLearnModeDialog { get; set; }
        ISecuritySchemaDialog SecuritySchema { get; set; }
        IAddNodeWithCustomSettingsModel AddNodeWithCustomSettingsDialog { get; set; }
        IConfig Config { get; set; }

        // Methods
        void NotifyControllerChanged(NotifyProperty notifyProperty, object data = null);
        void SetBusy(bool value);
        void SetBusyMessage(string message, int? progress = null);
        void Invoke(Action action);
        IDialog CreateEmptyUserInputDialog(string title, string message);
        void ShowMessageDialog(string title, string message);
        void ClearCommandQueueCollection();
        void SaveDialogSettings();
        void InitControllerSessionCommands();
        ISelectableItem<T> CreateSelectableItem<T>(T item);
    }
}