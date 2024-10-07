/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System.Linq;
using System.Threading;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveController.Models;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class AssociationsTests : ControllerTestBase
    {
        [Test]
        public void Associations_GetGroups_Success()
        {
            //Arrange.
            GetGroups();
          
            //Assert.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult != CommandExecutionResult.Failed);
            Assert.AreEqual(((AssociationViewModel)ApplicationPrimary.AssociationsModel).SelectedAssociativeDevice.Groups.Count, 1);
        }

        [Test]
        public void Associations_Create_Success()
        {
            CreateNode();

            //Arrange.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult != CommandExecutionResult.Failed);
            Assert.IsTrue(((AssociationViewModel)ApplicationPrimary.AssociationsModel).SelectedGroup.Nodes.NodeIds.Count > 0);
        }

        [Test]
        public void Associations_Remove_Success()
        {
            //Arrange.
            CreateNode();
            var model = (AssociationViewModel)ApplicationPrimary.AssociationsModel;
            model.SelectedGroup = model.SelectedAssociativeDevice.Groups.FirstOrDefault();
            model.SelectedAssociativeNode = model.SelectedGroup.Nodes.NodeIds.FirstOrDefault();

            //Act.
            CommandsFactory.CommandRunner.Execute(model.AssociationRemoveCommand, null);

            //Arrange.
            Assert.IsTrue(ApplicationPrimary.LastCommandExecutionResult != CommandExecutionResult.Failed);
            Assert.IsNull(model.SelectedGroup.Nodes);
        }

        private void GetGroups()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var newSupport = new byte[] { COMMAND_CLASS_ASSOCIATION_GRP_INFO_V3.ID, COMMAND_CLASS_ASSOCIATION_V2.ID, COMMAND_CLASS_SECURITY.ID, COMMAND_CLASS_SECURITY_2.ID, COMMAND_CLASS_SUPERVISION.ID, COMMAND_CLASS_VERSION_V3.ID };

            var controllerSession = (BasicControllerSession)ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceId];
            controllerSession.StopSupportTasks();
            ApplicationPrimary.Controller.Network.SetCommandClasses(newSupport);
            ApplicationPrimary.Controller.Network.SetCommandClasses(NODE_ID_2, newSupport);
            controllerSession.StartSupportTasks();

            controllerSession = (BasicControllerSession)ControllerSessionsContainer.ControllerSessions[ApplicationSecondary.DataSource.SourceId];
            controllerSession.StopSupportTasks();
            ApplicationSecondary.Controller.Network.SetCommandClasses(newSupport);
            ApplicationSecondary.Controller.Network.SetCommandClasses(NODE_ID_2, newSupport);
            controllerSession.StartSupportTasks();

            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);

            var model = (AssociationViewModel)ApplicationPrimary.AssociationsModel;
            model.SelectedNodeIds.Add(new NodeTag(ApplicationSecondary.Controller.Id));
            model.SelectedAssociativeDevice = model.AssociativeApplications.FirstOrDefault().Devices.FirstOrDefault();

            //Act.
            CommandsFactory.CommandRunner.Execute(model.AssociationGetGroupsCommand, null);
            Thread.Sleep(5000);
        }

        public void CreateNode()
        {
            //Arrange.
            GetGroups();
            var model = (AssociationViewModel)ApplicationPrimary.AssociationsModel;
            model.SelectedGroup = model.SelectedAssociativeDevice.Groups.FirstOrDefault();

            //Act.
            CommandsFactory.CommandRunner.Execute(model.AssociationCreateCommand, null);
        }
    }
}
