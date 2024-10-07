/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using Utils.UI.Enums;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Security;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class NetworkManagementTests : ControllerTestBase
    {
        [TestCase]
        public void AddNode_SecureAll_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).AddNodeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef2 = new CommandReference();
            cmdRef2.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef2.Execute(null);
            Thread.Sleep(6000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_ACCESS));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_AUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_UNAUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S0));
        }

        [TestCase]
        public void AddNode_Replication_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).AddNodeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef2 = new CommandReference();
            cmdRef2.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef2.Execute(null);
            Thread.Sleep(6000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_ACCESS));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_AUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_UNAUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S0));


            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            cmdRef2.Execute(null);
            Thread.Sleep(6000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_ACCESS));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_AUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_UNAUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S0));
        }


        [TestCase]
        public void RemoveNode_NodesRemoved_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).RemoveNodeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(500);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);
            //Assert.

            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationSecondary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsFalse(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 1);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 1);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
        }

        [TestCase]
        public void NWI_NodesAdded_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).AddNodeToNetworkNwiCommand;
            cmdRef.Execute(null);
            Thread.Sleep(500);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef2 = new CommandReference();
            cmdRef2.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef2.Execute(null);
            Thread.Sleep(5000);

            //Assert.
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_ACCESS));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_AUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S2_UNAUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary.Controller.Id, SecuritySchemes.S0));

            // Act.
            ApplicationSecondary2 = StartApplicationOnFakeDevice(_ctrlThird);
            CommandsFactory.CurrentSourceId = ApplicationSecondary2.DataSource.SourceId;
            CommandReference cmdRef3 = new CommandReference();
            cmdRef3.Command = ((NetworkManagementViewModel)ApplicationSecondary2.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef3.Execute(null);
            Thread.Sleep(5000);

            //Assert.
            Assert.That(ApplicationSecondary2.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary2.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 3);
            Assert.IsTrue(ApplicationSecondary2.ConfigurationItem.Nodes.Count == 3);
            Assert.IsTrue((ApplicationSecondary2.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary2.Controller.Id, SecuritySchemes.S2_ACCESS));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary2.Controller.Id, SecuritySchemes.S2_AUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary2.Controller.Id, SecuritySchemes.S2_UNAUTHENTICATED));
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HasSecurityScheme(ApplicationSecondary2.Controller.Id, SecuritySchemes.S0));

            //Assert.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary2.NetworkManagementModel).ApplicationModel.CancelActiveCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void NWE_NodesRemoved_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationSecondary2 = StartApplicationOnFakeDevice(_ctrlThird);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary2);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).RemoveNodeFromNetworkWideCommand;
            cmdRef.Execute(null);
            Thread.Sleep(500);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationSecondary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsFalse(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 1);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationSecondary2.DataSource.SourceId;
            CommandReference cmdRef3 = new CommandReference();
            cmdRef3.Command = ((NetworkManagementViewModel)ApplicationSecondary2.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef3.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary2.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationSecondary2.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsFalse(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary2.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 1);
            Assert.IsTrue(ApplicationSecondary2.ConfigurationItem.Nodes.Count == 1);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsTrue((ApplicationSecondary2.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));

            //Assert.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary2.NetworkManagementModel).ApplicationModel.CancelActiveCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void IsFailed_FailedIsSet_Success()
        {
            //Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetDefaultCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).IsFailedNodeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Network.IsFailed(ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(i => i.Item.Id != ApplicationSecondary.Controller.Id).Item));
        }

        [TestCase]
        public void ReplaceFailed_FailedNodeReplaced_Success()
        {
            //Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetDefaultCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).IsFailedNodeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(3000);
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).ReplaceFailedCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(10000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationSecondary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsFalse(ApplicationPrimary.ConfigurationItem.Network.IsFailed(ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(i => i.Item.Id != ApplicationPrimary.Controller.Id).Item));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
        }

        [TestCase]
        public void RemoveFailed_FailedNodeRemoved_Success()
        {
            //Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetDefaultCommand;
            cmdRef.Execute(null);
            Thread.Sleep(100);
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).RemoveFailedCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 1);
        }

        [TestCase]
        public void BasicGet_ToggleBasicGet__Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).ToggleBasicGetCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            if (ApplicationPrimary.LogDialog != null)
            {
                Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
                Assert.That(ApplicationSecondary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(1).After(1000, 100));
                Assert.IsTrue(ApplicationPrimary.NetworkManagementModel.IsBasicTestStarted);
            }

            ApplicationPrimary.LogDialog.Clear();
            cmdRef.Execute(null);
            Thread.Sleep(1000);
            ApplicationSecondary.LogDialog.Clear();
            Thread.Sleep(1000);

            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
            Assert.IsFalse(ApplicationPrimary.NetworkManagementModel.IsBasicTestStarted);
        }

        [TestCase]
        public void NOP_SendNOP_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SendNopCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
            Assert.That(ApplicationSecondary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(1).After(1000, 100));
        }

        [TestCase]
        public void SetAsSIS_NoSiS_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_1.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SetSucCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
            Assert.AreEqual(NODE_ID_1.Id, ApplicationPrimary.Controller.SucNodeId);
        }

        [TestCase]
        public void SetAsSIS_SISPresent_Fail()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_1.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            _transport.SetSucNodeId(ApplicationPrimary.Controller.SessionId, NODE_ID_1);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SetSucCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(1).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Fail));
            Assert.IsTrue(ApplicationPrimary.Controller.SucNodeId == NODE_ID_1.Id);
        }

        [TestCase]
        public void Neighbors_Update_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_1.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).RequestNodeNeighborUpdateCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
        }

        [TestCase]
        public void AddCustom_AllDisabled_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            ApplicationPrimary.AddNodeWithCustomSettingsDialog = new AddNodeWithCustomSettingsViewModel(ApplicationPrimary)
            {
                IsDeleteReturnRoute = false,
                IsAssignReturnRoute = false,
                IsAssociationCreate = false,
                IsMultichannelAssociationCreate = false,
                IsWakeUpCapabilities = false,
                IsWakeUpInterval = false,
                IsSetAsSisAutomatically = false,
                IsBasedOnZwpRoleType = false
            };
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).AddNodeWithCustomSettings;
            cmdRef.Execute(null);
            Thread.Sleep(100);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef2 = new CommandReference();
            cmdRef2.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef2.Execute(null);
            Thread.Sleep(10000);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            Assert.That(ApplicationSecondary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.Controller.Network.HomeId.SequenceEqual(ApplicationSecondary.Controller.Network.HomeId));
            Assert.IsTrue(ApplicationPrimary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue(ApplicationSecondary.ConfigurationItem.Nodes.Count == 2);
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
            Assert.IsFalse((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.SIS));
            Assert.IsFalse((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Inclusion));
        }

        [TestCase]
        public void NodeInfo_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).RequestNodeInfoCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
        }

        [TestCase]
        public void GetVersion_Default_Success()
        {
            //Arrange.
            byte[] data = new COMMAND_CLASS_VERSION_V2.VERSION_GET();
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).VersionGetCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.IsBusy, Is.False.After(5000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok && x.LogRawData.RawData.SequenceEqual(data) && x.LogRawData.SourceId == NODE_ID_2.Id));
        }

        [TestCase]
        public void BasicSetOn_Default_Success()
        {
            //Arrange.
            byte[] data = new COMMAND_CLASS_BASIC.BASIC_SET { value = 0xFF };
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOnCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok && x.LogRawData.RawData.SequenceEqual(data)));
        }

        [TestCase]
        public void BasicSetOff_Default_Success()
        {
            //Arrange.
            byte[] data = new COMMAND_CLASS_BASIC.BASIC_SET { value = 0x00 };
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOffCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok && x.LogRawData.RawData.SequenceEqual(data)));
        }

        [TestCase]
        public void SwitchAllOn_Default_Success()
        {
            //Arrange.
            byte[] data = new COMMAND_CLASS_SWITCH_ALL.SWITCH_ALL_ON();
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SwitchAllOnCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok && x.LogRawData.RawData.SequenceEqual(data)));
        }

        [TestCase]
        public void SwitchAllOff_Default_Success()
        {
            //Arrange.
            byte[] data = new COMMAND_CLASS_SWITCH_ALL.SWITCH_ALL_OFF();
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SwitchAllOffCommand;
            cmdRef.Execute(null);
            Thread.Sleep(200);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok && x.LogRawData.RawData.SequenceEqual(data)));
        }

        [TestCase]
        public void ToggleBasicTest_StartStop_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();


            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).ToggleBasicGetCommand;
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => ApplicationPrimary.NetworkManagementModel.IsBasicTestStarted, Is.True.After(2000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleBasicTest, true);

            //Act.
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => ApplicationPrimary.NetworkManagementModel.IsBasicTestStarted, Is.False.After(2000, 100));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleBasicTest, false);
        }

        [TestCase]
        public void ResetSpan_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();
            var peerNodeId = new InvariantPeerNodeId(ApplicationPrimary.Controller.Network.NodeTag, ApplicationSecondary.Controller.Network.NodeTag);
            var sm = ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].SecurityManager;
            sm.SecurityManagerInfo.SpanTable.Add(peerNodeId,
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 00, 11, 88 }, 0x1, 0x1);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).ResetSPANCommand;
            cmdRef.Execute(null);
            Thread.Sleep(3000);

            //Assert.
            Assert.IsNull(sm.SecurityManagerInfo.SpanTable.GetContainer(peerNodeId));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void NextSpan_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            var peerNodeId = new ZWave.Security.InvariantPeerNodeId(ApplicationPrimary.Controller.Network.NodeTag, ApplicationSecondary.Controller.Network.NodeTag);
            var sm = ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].SecurityManager;
            sm.SecurityManagerInfo.SpanTable.Add(peerNodeId,
                new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 00, 11, 88 }, 0x1, 0x1);

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).NextSPANCommand;
            cmdRef.Execute(null);
            Thread.Sleep(2000);

            //Assert.
            Assert.IsTrue(sm.SecurityManagerInfo.SpanTable.GetContainer(peerNodeId).SpanState == SpanStates.Span);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void SecuritySchema_SetS2Unathenticated_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_2.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            var ss = ApplicationPrimary.SecuritySchema as SecuritySchemaDialogViewModel;
            ss.IsOk = true;
            ss.State.SelectedInputOption = SecuritySchemes.S2_UNAUTHENTICATED;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SetS2Command;
            cmdRef.Execute(null);
            Thread.Sleep(20000);

            //Assert.
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void SetDefault_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SetDefaultCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(1000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
        }

        [TestCase]
        public void SendNodeInfo_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_1.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SendNodeInformationCommand;
            cmdRef.Execute(null);
            Thread.Sleep(3000);

            //Assert.
            Assert.That(ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(3000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
        }

        [TestCase]
        public void RFRecieverSetOf_Default_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == NODE_ID_1.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).SetRFReceiveModeCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);

            //Assert.
            Assert.IsFalse(ApplicationPrimary.IsBusy);
            Assert.That(() => ApplicationPrimary.LogDialog.Queue.Count, Is.GreaterThanOrEqualTo(2).After(6000, 100));
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(x => x.LogLevel == LogLevels.Ok));
        }

        [TestCase]
        public void Shift_SwitchRole_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            ApplicationPrimary.SelectedNode = ApplicationPrimary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.SelectedNode.IsSelected = true;
            ApplicationPrimary.NetworkManagementModel.SelectedNodeItems = ApplicationPrimary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();

            //Act.
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).ShiftControllerCommand;
            cmdRef.Execute(null);

            Thread.Sleep(1000);

            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef2 = new CommandReference();
            cmdRef2.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SetClassicLearnModeCommand;
            cmdRef2.Execute(null);
            Thread.Sleep(15000);

            //Assert.
            Assert.That(() => ApplicationPrimary.IsBusy, Is.False.After(10000, 100));
            Assert.That(() => ApplicationSecondary.IsBusy, Is.False.After(10000, 100));
            ApplicationSecondary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
            Assert.IsTrue((ApplicationPrimary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.Secondary));
            Assert.IsTrue((ApplicationSecondary.Controller as IController).NetworkRole.HasFlag(ControllerRoles.RealPrimary));
        }

        [TestCase]
        public void Mpan_AddMpan_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            var mpanTable = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel;
            var state = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 55, 66, 77 };
            mpanTable.TestMpanState = state;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel.AddOrUpdateSelectedMpanItemCommand;
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => mpanTable.FullMpanTableBind.Count, Is.EqualTo(1).After(2000, 100));
            ApplicationSecondary.DidNotReceive().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
            Assert.IsTrue(mpanTable.FullMpanTableBind[0].MpanState.SequenceEqual(state));
        }

        [TestCase]
        public void Mpan_UpdateMpan_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var mpanTable = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel;
            var mpanItem = new MpanItem()
            {
                MpanState = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 55, 66, 77 }
            };
            mpanTable.FullMpanTableBind = new List<IMpanItem>() { mpanItem };
            var state = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 00, 11, 88 };
            mpanTable.TestMpanState = state;
            //Act.

            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel.AddOrUpdateSelectedMpanItemCommand;
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => mpanTable.FullMpanTableBind.Count, Is.EqualTo(1).After(2000, 100));
            ApplicationSecondary.DidNotReceive().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
            Assert.IsTrue(mpanTable.FullMpanTableBind[0].MpanState.SequenceEqual(state));
        }


        [TestCase]
        public void Mpan_Remove_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var mpanTable = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel;
            var mpanItem = new MpanItem()
            {
                MpanState = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 55, 66, 77 }
            };
            mpanTable.FullMpanTableBind = new List<IMpanItem>() { mpanItem };
            mpanTable.SelectedMpanItem = mpanTable.FullMpanTableBind.FirstOrDefault();
            //Act.

            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel.RemoveSelectedMpanItemCommand;
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => mpanTable.FullMpanTableBind.Count, Is.EqualTo(0).After(2000, 100));
            //Thread.Sleep(2000);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [TestCase]
        public void Mpan_Next_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;

            //Act.
            var mpanTable = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel;
            var state = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 22, 33, 44, 55, 66, 77 };
            var mpanItem = new MpanItem()
            {
                GroupId = 1,
                Owner = new NodeTag(1),
                MpanState = state
            };
            mpanTable.TestMpanState = mpanItem.MpanState;
            mpanTable.TestMpanGroupId = mpanItem.GroupId;
            mpanTable.TestMpanOwner = mpanItem.Owner;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel.AddOrUpdateSelectedMpanItemCommand;
            cmdRef.Execute(null);
            Thread.Sleep(1000);
            state[15] = (byte)(state[15] + 1);
            var securityInfo = ControllerSessionsContainer.ControllerSessions[ApplicationPrimary.DataSource.SourceName].SecurityManager.SecurityManagerInfo;
            securityInfo.ActivateNetworkKeyS2Multi(mpanItem.GroupId, mpanItem.Owner, SecuritySchemes.S2_ACCESS, false);
            mpanTable.SelectedMpanItem = mpanTable.FullMpanTableBind.FirstOrDefault();

            //Act.
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).MpanTableConfigurationViewModel.NextSelectedMpanItemCommand;
            cmdRef.Execute(null);

            //Assert.
            Assert.That(() => mpanTable.FullMpanTableBind.Count, Is.EqualTo(1).After(5000, 100));
            ApplicationSecondary.DidNotReceive().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
            Assert.IsTrue(mpanTable.SelectedMpanItem.MpanState.SequenceEqual(state));
            Assert.IsTrue(mpanTable.SelectedMpanItem.SequenceNumber == mpanItem.SequenceNumber + 1);
        }
    }
}