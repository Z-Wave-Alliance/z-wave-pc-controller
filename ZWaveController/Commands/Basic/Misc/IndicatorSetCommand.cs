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
    public class IndicatorSetCommand : NetworkManagamentCommandBase
    {
        private NodeTag[] _selectedNodeItems;
        public IndicatorSetCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Indicator";
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ApplicationModel.SelectedNode != null &&
                ControllerSession.Controller.Network.HasCommandClass(ApplicationModel.SelectedNode.Item, COMMAND_CLASS_INDICATOR_V3.ID);
        }

        public override bool CanExecuteUIDependent(object param)
        {
            return ApplicationModel.SelectedNode?.Item.Id != ControllerSession.Controller.Id;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveSendData; }
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override void PrepareData()
        {
            base.PrepareData();
            _selectedNodeItems = ApplicationModel.NetworkManagementModel.SelectedNodeItems.ToArray();
        }

        protected override void ExecuteInner(object parameter)
        {
            var data = new COMMAND_CLASS_INDICATOR_V3.INDICATOR_SET() { };
            data.properties1.indicatorObjectCount = 0x03;
            data.vg1.Add(new COMMAND_CLASS_INDICATOR_V3.INDICATOR_SET.TVG1()
            {
                indicatorId = 0x50,
                propertyId = 0x03,
                value = 0x08
            });
            data.vg1.Add(new COMMAND_CLASS_INDICATOR_V3.INDICATOR_SET.TVG1()
            {
                indicatorId = 0x50,
                propertyId = 0x04,
                value = 0x03
            });
            data.vg1.Add(new COMMAND_CLASS_INDICATOR_V3.INDICATOR_SET.TVG1()
            {
                indicatorId = 0x50,
                propertyId = 0x05,
                value = 0x06
            });
            ControllerSession.SenderHistoryService.Add(data);
            if (_selectedNodeItems.Length > 1)
            {
                ControllerSession.SendData(_selectedNodeItems, data, 0, SubstituteFlags.None, _token);
            }
            else
            {
                ControllerSession.SendData(Device, data, 0, SubstituteFlags.None, _token);
            }
            ApplicationModel.Invoke(() => ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(IndicatorSetCommand), Message = "Indicator Set" }));
        }
    }
}
