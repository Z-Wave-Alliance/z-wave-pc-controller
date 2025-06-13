/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Threading;
using ZWaveController.Commands;
using System.Linq;
using ZWave.BasicApplication.Devices;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;
using ZWaveController;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;

namespace ZWaveControllerUI.Models
{
    public class MainMenuViewModel : VMBase
    {
        public CommandBase ShowNetworkManagementCommand =>
            CommandsFactory.CommandBaseGet<CommandBase>(ShowNetworkManagement, c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy);
        public CommandBase ShowEncryptDecryptCommand =>
            CommandsFactory.CommandBaseGet<CommandBase>(p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.EncryptDecryptModel);
        public CommandBase ShowCommandClassesCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowCommandClasses, c => !ApplicationModel.IsBusy, false);
        public CommandBase ShowSetupRouteCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.SetupRouteModel,
                c => ApplicationModel.Controller is IController && !ApplicationModel.IsActiveSessionZip && !ApplicationModel.IsBusy);
        public CommandBase ShowERTTCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.ERTTModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsActiveSessionZip && !ApplicationModel.IsBusy);
        public CommandBase ShowFirmwareUpdateCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.FirmwareUpdateModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy);
        public CommandBase ShowPollingCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.PollingModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy);
        public CommandBase ShowTransmitSettingsCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.TransmitSettingsModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy && ChipTypeSupported.TransmitSettings(ApplicationModel.Controller.ChipType));
        public CommandBase ShowTopologyMapCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.TopologyMapModel,
                c => ApplicationModel.Controller is IController && !ApplicationModel.IsActiveSessionZip);
        public CommandBase ShowIMAFullNetworkCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowIMAFullNetwork, c => ApplicationModel.Controller is IController && !ApplicationModel.IsBusy);
        public CommandBase ShowNetworkStatisticsCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.NetworkStatisticsModel,
                c => ApplicationModel.Controller is IController && !ApplicationModel.IsBusy && ChipTypeSupported.NetworkStatistics(ApplicationModel.Controller.ChipType));
        public CommandBase ShowNVMBackupRestoreCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.NVMBackupRestoreModel,
                c => ApplicationModel.Controller is IController && !ApplicationModel.IsActiveSessionZip && !ApplicationModel.IsBusy);
        public CommandBase ShowConfigurationCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.ConfigurationModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy);
        public CommandBase ShowAssociationsCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.AssociationsModel,
                c => ApplicationModel.Controller is IController && !ApplicationModel.IsBusy);
        public CommandBase ShowULMonitorCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                p => ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.ULMonitorViewModel,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy);
        public CommandBase ShowSmartStartCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowSmartStart, c => ApplicationModel.Controller is IController && !ApplicationModel.IsBusy);
        public CommandBase ShowNodeInformationCommand => CommandsFactory.CommandBaseGet<CommandBase>(
                ShowNodeInformation,
                c => ApplicationModel.Controller != null && !ApplicationModel.IsBusy && !ApplicationModel.IsActiveSessionZip);
        public FirmwareUpdateLocalCommand FirmwareUpdateLocalCommand => CommandsFactory.CommandControllerSessionGet<FirmwareUpdateLocalCommand>();

        private string _controllerInfo = $"Controller id: {"CONTROLLER ID"}";
        public string ControllerInfo
        {
            get { return _controllerInfo; }
            set
            {
                _controllerInfo = value;
                Notify("ControllerInfo");
            }
        }

        private bool _isIMAEnabled = false;
        public bool IsIMAEnabled
        {
            get { return _isIMAEnabled; }
            set
            {
                _isIMAEnabled = value;
                Notify("IsIMAEnabled");
            }
        }

        public MainMenuViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
        }

        private void ShowIMAFullNetwork(object obj)
        {
            var controller = ApplicationModel.Controller as Controller;
            if (controller != null && ApplicationModel.IsNeedFirstReloadTopology && !ApplicationModel.IsActiveSessionZip)
            {
                ThreadPool.QueueUserWorkItem((y) =>
                {
                    CommandsFactory.CommandRunner.Execute(((IMAFullNetworkViewModel)ApplicationModel.IMAFullNetworkModel).GetRoutingInfoCommand, null);
                    ApplicationModel.IsNeedFirstReloadTopology = false;
                });
            }
            ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.IMAFullNetworkModel;
        }

        public void ShowCommandClasses(object obj)
        {
            ApplicationModel.CurrentViewModel = this;
            ((VMBase)ApplicationModel.CommandClassesModel).Close();

            if (obj != null && (obj is ZWave.Xml.Application.Command))
            {
                ZWave.Xml.Application.Command Command = (ZWave.Xml.Application.Command)obj;
                if (Command != null)
                {
                    ((CommandClassesViewModel)ApplicationModel.CommandClassesModel).SelectedCommand = Command;
                }
            }
            if (((CommandClassesViewModel)ApplicationModel.CommandClassesModel).DialogSettings.IsFloating)
            {
                ((IDialog)ApplicationModel.CommandClassesModel).ShowDialog();
            }
            else
            {
                ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.CommandClassesModel;
            }
        }

        public void ShowSmartStart(object obj)
        {
            ApplicationModel.CurrentViewModel = this;
            ((SmartStartViewModel)ApplicationModel.SmartStartModel)?.Close();

            if (((SmartStartViewModel)ApplicationModel.SmartStartModel).DialogSettings.IsFloating)
            {
                ((IDialog)ApplicationModel.SmartStartModel).ShowDialog();
            }
            else
            {
                ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.SmartStartModel;
            }
        }

        private void ShowNodeInformation(object obj)
        {
            ApplicationModel.CurrentViewModel = this;
            //MainVM.SetNodeInformationVM.Close();

            var controllerNode = ApplicationModel.ConfigurationItem.Nodes.Where(x => x.Item.Id == ApplicationModel.Controller.Id).Select(y => y.Item).FirstOrDefault();
            var nInfo = ApplicationModel.Controller.Network.GetNodeInfo();
            ApplicationModel.SetNodeInformationModel.DeviceOption = (DeviceOptionsView)(nInfo.DeviceOptions & ~DeviceOptions.Listening);
            ApplicationModel.SetNodeInformationModel.IsListening = (nInfo.DeviceOptions & DeviceOptions.Listening) > 0;
            ApplicationModel.SetNodeInformationModel.SelectedGenericDevice = nInfo.Generic == 0 ?
                ApplicationModel.SetNodeInformationModel.GenericDevices.First(i => i.Item.Name == "GENERIC_TYPE_STATIC_CONTROLLER").Item:
                ApplicationModel.SetNodeInformationModel.GenericDevices.First(i => i.Item.KeyId == nInfo.Generic).Item;
            ApplicationModel.SetNodeInformationModel.SelectedSpecificDevice = nInfo.Specific == 0 ?
                ApplicationModel.SetNodeInformationModel.SelectedGenericDevice.SpecificDevice.FirstOrDefault(i => i.Name == "SPECIFIC_TYPE_PC_CONTROLLER") :
                ApplicationModel.SetNodeInformationModel.SelectedGenericDevice.SpecificDevice.FirstOrDefault(i => i.KeyId == nInfo.Specific);
            ApplicationModel.SetNodeInformationModel.CommandClasses.Where(v => v.IsSelected).ToList().ForEach(v => v.IsSelected = false);
            ApplicationModel.SetNodeInformationModel.RoleType = ApplicationModel.Controller.Network.GetRoleType();
            ApplicationModel.SetNodeInformationModel.NodeType = ApplicationModel.Controller.Network.GetNodeType();

            foreach (var ccId in ApplicationModel.Controller.Network.GetCommandClasses(ApplicationModel.Controller.Network.NodeTag) ?? new byte[0])
            {
                var cc = ApplicationModel.SetNodeInformationModel.CommandClasses.Where(i => i.Item.KeyId == ccId);
                if (cc.Any())
                {
                    foreach (var ccVm in cc)
                    {
                        ccVm.IsSelected = true;
                    }
                }
            }
            ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.SetNodeInformationModel;
        }

        public void ShowNetworkManagement(object obj)
        {
            ApplicationModel.CurrentViewModel = this;
            ((NetworkManagementViewModel)ApplicationModel.NetworkManagementModel)?.Close();

            if (((NetworkManagementViewModel)ApplicationModel.NetworkManagementModel)?.DialogSettings.IsFloating == true)
            {
                ((IDialog)ApplicationModel.NetworkManagementModel).ShowDialog();
            }
            else
            {
                ApplicationModel.CurrentViewModel = (VMBase)ApplicationModel.NetworkManagementModel;
            }
        }
    }
}