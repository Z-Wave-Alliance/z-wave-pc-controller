/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SetNodeInformationCommand : CommandBasicBase
    {
        private const byte DEFAULT_STATIC_CONTROLLER_KEYID = 0x02;
        private const byte DEFAULT_PC_CONTROLLER_KEYID = 0x01;

        public SetNodeInformationCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Set Controller Node Inforamtion";
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is BasicControllerSession &&
                ApplicationModel.SetNodeInformationModel.SelectedGenericDevice != null &&
                ApplicationModel.SetNodeInformationModel.SelectedSpecificDevice != null;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.Controller.Network.SetRoleType(ApplicationModel.SetNodeInformationModel.RoleType);
            ApplicationModel.Controller.Network.SetNodeType(ApplicationModel.SetNodeInformationModel.NodeType);
            var commandClasses = ApplicationModel.SetNodeInformationModel.CommandClasses.
                Where(v => v.IsSelected).
                Select(v => v.Item.KeyId).
                OrderBy(x => x != COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID).
                Distinct().
                ToArray();
            var deviceOption = (DeviceOptions)ApplicationModel.SetNodeInformationModel.DeviceOption;
            if (ApplicationModel.SetNodeInformationModel.IsListening)
            {
                deviceOption |= DeviceOptions.Listening;
            }
            byte genericKeyId = ApplicationModel.SetNodeInformationModel.SelectedGenericDevice == null ? DEFAULT_STATIC_CONTROLLER_KEYID : ApplicationModel.SetNodeInformationModel.SelectedGenericDevice.KeyId;
            byte specificKeyId = ApplicationModel.SetNodeInformationModel.SelectedSpecificDevice == null ? DEFAULT_PC_CONTROLLER_KEYID : ApplicationModel.SetNodeInformationModel.SelectedSpecificDevice.KeyId;

            ControllerSession.SetNodeInformation(deviceOption, genericKeyId, specificKeyId, commandClasses, out _token);
            ApplicationModel.NotifyControllerChanged(NotifyProperty.CommandSucceeded, new NotifyCommandData { CommandName = nameof(SetNodeInformationCommand), Message = "Node Information is Set" });
            ApplicationModel.NotifyControllerChanged(NotifyProperty.RefreshNodeInfoCommandClasses);
        }
    }
}
