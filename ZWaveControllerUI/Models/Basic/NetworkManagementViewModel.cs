/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.Devices;
using ZWaveController.Commands;
using ZWave;
using System.Net;
using ZWaveController.Interfaces;
using Utils.UI.Interfaces;
using ZWaveController.Enums;
using ZWaveController;
using ZWave.Enums;

namespace ZWaveControllerUI.Models
{
    public class NetworkManagementModel : DialogVMBase
    {
        private int _wakeupIntervalValue = 5;
        private byte _sendNopNodeId = 0;
        private byte _removeNodeId = 0;

        private string _basicTestCaption = "Start Test 'Basic Get'";
        public string BasicTestCaption
        {
            get { return _basicTestCaption; }
            set
            {
                _basicTestCaption = value;
                Notify("BasicTestCaption");
            }
        }

        private bool _isBasicTestStarted;
        public bool IsBasicTestStarted
        {
            get { return _isBasicTestStarted; }
            set
            {
                _isBasicTestStarted = value;
                if (value)
                {
                    BasicTestCaption = "Stop Test 'Basic Get'";
                }
                else
                {
                    BasicTestCaption = "Start Test 'Basic Get'";
                }
                Notify("IsBasicTestStarted");
            }
        }

        public byte SendNopNodeId
        {
            get { return _sendNopNodeId; }
            set
            {
                _sendNopNodeId = value;
                Notify("SendNopNodeId");
            }
        }

        public byte RemoveNodeId
        {
            get { return _removeNodeId; }
            set
            {
                _removeNodeId = value;
                Notify("RemoveNodeId");
            }
        }

        public NetworkManagementModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = true;
            DialogSettings.IsTopmost = false;
        }

        public int WakeupIntervalValue
        {
            get { return _wakeupIntervalValue; }
            set
            {
                _wakeupIntervalValue = value;
                Notify("WakeupIntervalValue");
            }
        }

        public override object Clone()
        {
            var ret = (NetworkManagementModel)base.Clone();
            return ret;
        }
    }

    public class NetworkManagementViewModel : NetworkManagementModel, INetworkManagementModel
    {
        public SelectLocalIpAddressViewModel SelectLocalIpAddressViewModel => (SelectLocalIpAddressViewModel)ApplicationModel.SelectLocalIpAddressDialog;
        public MpanTableConfigurationViewModel MpanTableConfigurationViewModel => (MpanTableConfigurationViewModel)ApplicationModel.MpanTableConfigurationDialog;
        public SetLearnModeViewModel SetLearnModeViewModel => (SetLearnModeViewModel)ApplicationModel.SetLearnModeDialog;

        #region Commands

        public AddNodeCommand AddNodeCommand => CommandsFactory.CommandControllerSessionGet<AddNodeCommand>();
        public RemoveNodeCommand RemoveNodeCommand => CommandsFactory.CommandControllerSessionGet<RemoveNodeCommand>();
        public AddVirtualNodeCommand AddVirtualNodeCommand => CommandsFactory.CommandControllerSessionGet<AddVirtualNodeCommand>();
        public RemoveVirtualNodeCommand RemoveVirtualNodeCommand => CommandsFactory.CommandControllerSessionGet<RemoveVirtualNodeCommand>();
        public RequestNodeInfoCommand RequestNodeInfoCommand => CommandsFactory.CommandControllerSessionGet<RequestNodeInfoCommand>();
        public IsFailedNodeCommand IsFailedNodeCommand => CommandsFactory.CommandControllerSessionGet<IsFailedNodeCommand>();
        public AddNodeToNetworkNWICommand AddNodeToNetworkNwiCommand => CommandsFactory.CommandControllerSessionGet<AddNodeToNetworkNWICommand>();
        public SetSUCCommand SetSucCommand => CommandsFactory.CommandControllerSessionGet<SetSUCCommand>();
        public ReplaceFailedCommand ReplaceFailedCommand => CommandsFactory.CommandControllerSessionGet<ReplaceFailedCommand>();
        public RemoveFailedCommand RemoveFailedCommand => CommandsFactory.CommandControllerSessionGet<RemoveFailedCommand>();
        public RequestNodeNeighborUpdateCommand RequestNodeNeighborUpdateCommand => CommandsFactory.CommandControllerSessionGet<RequestNodeNeighborUpdateCommand>();
        public AddNodeWithCustomSettingsCommand AddNodeWithCustomSettings => CommandsFactory.CommandControllerSessionGet<AddNodeWithCustomSettingsCommand>();
        public CommandBase AddNodeWithCustomSettingsCommand => CommandsFactory.CommandBaseGet<CommandBase>(
            param =>
            {
                if (((IDialog)ApplicationModel.AddNodeWithCustomSettingsDialog).ShowDialog())
                    AddNodeWithCustomSettings?.Execute(null);
            },
            AddNodeWithCustomSettings.CanExecute, AddNodeWithCustomSettings.Cancel, AddNodeWithCustomSettings.CanCancel);
        public VersionGetCommand VersionGetCommand => CommandsFactory.CommandControllerSessionGet<VersionGetCommand>();
        public IndicatorSetCommand IndicatorSetCommand => CommandsFactory.CommandControllerSessionGet<IndicatorSetCommand>();
        public BasicSetOnOffCommand BasicSetOnCommand => CommandsFactory.CommandControllerSessionGet<BasicSetOnOffCommand>(null, (byte)0xFF);
        public BasicSetOnOffCommand BasicSetOffCommand => CommandsFactory.CommandControllerSessionGet<BasicSetOnOffCommand>(null, (byte)0x00);
        public ToggleBasicGetCommand ToggleBasicGetCommand => CommandsFactory.CommandControllerSessionGet<ToggleBasicGetCommand>();
        public SwitchAllOnOffCommand SwitchAllOnCommand => CommandsFactory.CommandControllerSessionGet<SwitchAllOnOffCommand>(null, (byte)0xFF);
        public SwitchAllOnOffCommand SwitchAllOffCommand => CommandsFactory.CommandControllerSessionGet<SwitchAllOnOffCommand>(null, (byte)0x00);
        public SetWakeupIntervalCommand SetWakeupIntervalCommand => CommandsFactory.CommandControllerSessionGet<SetWakeupIntervalCommand>();
        public SendWakeUpNotificationCommand SendWakeUpNotificationCommand => CommandsFactory.CommandControllerSessionGet<SendWakeUpNotificationCommand>();
        public SendNOPCommand SendNopCommand => CommandsFactory.CommandControllerSessionGet<SendNOPCommand>();
        //public CommandBase AssignSucSisNoneCommand => CommandsFactory.CommandControllerSessionGet<AssignSucSisNoneCommand>();
        public SetS2Command SetS2Command => CommandsFactory.CommandControllerSessionGet<SetS2Command>();
        public ResetSPANCommand ResetSPANCommand => CommandsFactory.CommandControllerSessionGet<ResetSPANCommand>();
        public NextSPANCommand NextSPANCommand => CommandsFactory.CommandControllerSessionGet<NextSPANCommand>();
        public StartLearnModeCommand SetClassicLearnModeCommand => CommandsFactory.CommandControllerSessionGet<StartLearnModeCommand>(null, LearnModes.LearnModeClassic);
        public CommandBase ShowSetLearnModeCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowSetLearnModeDlg, obj => ApplicationModel.Controller != null && ApplicationModel.SelectedNode != null);
        public SetDefaultCommand SetDefaultCommand => CommandsFactory.CommandControllerSessionGet<SetDefaultCommand>();
        public RequestNetworkUpdateCommand RequestNetworkUpdateCommand => CommandsFactory.CommandControllerSessionGet<RequestNetworkUpdateCommand>();
        public ShiftControllerCommand ShiftControllerCommand => CommandsFactory.CommandControllerSessionGet<ShiftControllerCommand>();
        public SendNodeInformationCommand SendNodeInformationCommand => CommandsFactory.CommandControllerSessionGet<SendNodeInformationCommand>();
        public SetRFReceiveModeCommand SetRFReceiveModeCommand => CommandsFactory.CommandControllerSessionGet<SetRFReceiveModeCommand>();
        public SetSleepModeCommand SetSleepModeCommand => CommandsFactory.CommandControllerSessionGet<SetSleepModeCommand>();
        public CommandBase NodeSettingsCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowSetLearnModeDlg, obj => ApplicationModel.Controller != null && ApplicationModel.SelectedNode != null);
        public RemoveNodeFromNetworkWideCommand RemoveNodeFromNetworkWideCommand => CommandsFactory.CommandControllerSessionGet<RemoveNodeFromNetworkWideCommand>();
        public CommandBase ShowMpanTableConfigurationCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowMpanTableConfigurationCommandExecute, obj => ApplicationModel.Controller is IController && !ApplicationModel.IsActiveSessionZip);
        public CommandBase ShowZipUnsolicitedDestinationCommand => CommandsFactory.CommandBaseGet<CommandBase>(ShowZipUnsolicitedDestination, obj => ApplicationModel.IsActiveSessionZip);
        #endregion

        public NetworkManagementViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Network management";
            ApplicationModel.SelectedNodesChanged += new System.Action(ApplicationModel_SelectedNodesChanged);
        }

        private void ShowSetLearnModeDlg(object obj)
        {
            ((IDialog)SetLearnModeViewModel).ShowDialog();
        }

        private void ShowZipUnsolicitedDestination(object obj)
        {
            SelectLocalIpAddressViewModel.CurrentViewType = SelectLocalIpViews.UnsolicitedDestination;
            SelectLocalIpAddressViewModel.CustomPrimaryAddress = null;
            SelectLocalIpAddressViewModel.CustomPrimaryPort = 0;
            ((IDialog)SelectLocalIpAddressViewModel).ShowDialog();
        }

        private void ShowMpanTableConfigurationCommandExecute(object obj)
        {
            ((IDialog)MpanTableConfigurationViewModel).ShowDialog();
        }

        void ApplicationModel_SelectedNodesChanged()
        {
            NodeTag[] selected = null;
            if (ApplicationModel.ConfigurationItem.Nodes != null)
            {
                selected = ApplicationModel.ConfigurationItem.Nodes.Where(x => x.IsEnabled && x.IsSelected).Select(x => x.Item).ToArray();
            }
            SelectedNodeItems = selected;
        }

        private NodeTag[] _selectedNodeItems;
        public NodeTag[] SelectedNodeItems
        {
            get { return _selectedNodeItems ?? new NodeTag[0]; }
            set
            {
                _selectedNodeItems = value;
                Notify("SelectedNodeItems");
            }
        }

        public void NotifyApplicationModel()
        {
            Notify("ApplicationModel");
        }

        public ActionToken BasicTestToken { get; set; } = null;
        public void StopBasicTest()
        {
            if (ApplicationModel.Controller != null && BasicTestToken != null)
            {
                ApplicationModel.Controller.Cancel(BasicTestToken);
            }
            IsBasicTestStarted = false;
        }

        //public bool _isExtensionOpen = false;
        //public bool IsExtensionOpen
        //{
        //    get { return _isExtensionOpen; }
        //    set
        //    {
        //        _isExtensionOpen = value;
        //        Notify("IsExtensionOpen");
        //    }
        //}

        //public VMBase _extensionViewModel;
        //public VMBase ExtensionViewModel
        //{
        //    get { return _extensionViewModel; }
        //    set
        //    {
        //        _extensionViewModel = value;
        //        Notify("ExtensionViewModel");
        //    }
        //}
    }
}
