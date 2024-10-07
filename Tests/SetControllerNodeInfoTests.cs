/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Linq;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveController.Models;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SetControllerNodeInfoTests : ControllerTestBase
    {
        [Test]
        public void SetNodeInfo_ClearSet_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SetNodeInformationViewModel)ApplicationPrimary.SetNodeInformationModel;
            if (!model.CommandClasses.Any(i => i.IsSelected))
                model.CommandClasses.FirstOrDefault().IsSelected = true;

            //Act.
            model.ClearActiveCommandClassesCommand.Execute(null);

            //Assert.
            Assert.IsTrue(!model.CommandClasses.Any(i => i.IsSelected));

            //Act.
            model.SelectDefaultCommandClassesCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.CommandClasses.Any(i => i.IsSelected));

            //Act.
            model.ClearActiveCommandClassesCommand.Execute(null);
            Assert.IsTrue(!model.CommandClasses.Any(i => i.IsSelected));
            model.CommandClasses.FirstOrDefault().IsSelected = true;
            model.SetNodeInformationCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.CommandClasses.Any(i => i.IsSelected));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.RefreshNodeInfoCommandClasses);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [Test]
        public void SetNodeInfo_SetInfoReport_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SetNodeInformationViewModel)ApplicationPrimary.SetNodeInformationModel;
            var role = model.RoleType == RoleTypes.END_NODE_PORTABLE ? RoleTypes.END_NODE_SLEEPING_REPORTING : RoleTypes.END_NODE_PORTABLE;
            var node = model.NodeType == NodeTypes.ZWAVEPLUS_FOR_IP_GATEWAY ? NodeTypes.ZWAVEPLUS_NODE : NodeTypes.ZWAVEPLUS_FOR_IP_GATEWAY;

            //Act.
            model.RoleType = role;
            model.NodeType = node;
            model.SetNodeInformationCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.RoleType == role);
            Assert.IsTrue(model.NodeType == node);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.RefreshNodeInfoCommandClasses);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [Test]
        public void SetNodeInfo_SetDeviceNodeInfo_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (SetNodeInformationViewModel)ApplicationPrimary.SetNodeInformationModel;
            var deviceOption = model.DeviceOption == DeviceOptionsView.FreqListeningMode1000ms ? DeviceOptionsView.FreqListeningMode250ms : DeviceOptionsView.FreqListeningMode1000ms;
            var generic = model.SelectedGenericDevice.KeyId == model.GenericDevices.FirstOrDefault().Item.KeyId ? model.GenericDevices.LastOrDefault().Item : model.GenericDevices.FirstOrDefault().Item;
            var specific = model.SelectedSpecificDevice.KeyId == model.SelectedGenericDevice.SpecificDevice.FirstOrDefault().KeyId ? model.SelectedGenericDevice.SpecificDevice.LastOrDefault() : model.SelectedGenericDevice.SpecificDevice.FirstOrDefault();

            //Act.
            model.DeviceOption = deviceOption;
            model.SelectedGenericDevice = generic;
            model.SelectedSpecificDevice = specific;
            model.SetNodeInformationCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.DeviceOption == deviceOption);
            Assert.IsTrue(model.SelectedGenericDevice.KeyId == generic.KeyId);
            Assert.IsTrue(model.SelectedSpecificDevice.KeyId == specific.KeyId);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.RefreshNodeInfoCommandClasses);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }
    }
}