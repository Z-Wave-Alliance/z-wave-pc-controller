/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Devices;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class AssociationCommandBase : CommandBasicBase
    {
        protected byte GroupId { get; set; }
        public  IAssociationsModel AssociationModel
        {
            get; private set;
        }

        public AssociationCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            AssociationModel = controllerSession.ApplicationModel.AssociationsModel;
            UseBackgroundThread = true;
        }

        public override void UpdateTargetDevice()
        {
            if (AssociationModel.SelectedAssociativeDevice != null)
            {
                TargetDevice = AssociationModel.SelectedAssociativeDevice.Device;
                IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(AssociationModel.SelectedAssociativeDevice.Device);
            }
            else if (AssociationModel.SelectedGroup != null)
            {
                TargetDevice = AssociationModel.SelectedGroup.ParentDevice.Device;
                IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(AssociationModel.SelectedGroup.ParentDevice.Device);
            }
        }

        public override void PrepareData()
        {
            Device = (NodeTag)AssociationModel.SelectedGroup.ParentDevice.Device.Clone();
            GroupId = AssociationModel.SelectedGroup.Id;
        }
    }
}
