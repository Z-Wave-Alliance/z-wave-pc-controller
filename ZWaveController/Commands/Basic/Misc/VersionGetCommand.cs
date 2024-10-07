/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class VersionGetCommand : NetworkManagamentCommandBase
    {
        public VersionGetCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Get Version";
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteUIDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.ApplicationModel.SelectedNode != null &&
                   (ControllerSession.ApplicationModel.IsActiveSessionZip ? true:
                    ControllerSession.ApplicationModel.SelectedNode.Item.Id != SessionDevice.Id);
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        private NodeTag[] _selectedItems { get; set; }

        public override void PrepareData()
        {
            base.PrepareData();
            _selectedItems = NetworkManagementModel.SelectedNodeItems.ToArray();
        }

        protected override void ExecuteInner(object param)
        {
            var data = new COMMAND_CLASS_VERSION_V2.VERSION_GET();
            ControllerSession.SenderHistoryService.Add(data);
            if (_selectedItems.Length > 1)
            {
                ControllerSession.SendData(_selectedItems, data, 0, SubstituteFlags.None, _token);
            }
            else
            {
                ControllerSession.SendData(Device, data, 0, SubstituteFlags.None, _token);
            }
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(VersionGetCommand), Message = "Version Received" });
        }
    }
}
