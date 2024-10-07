/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Utils.UI.Interfaces;
using ZWave.CommandClasses;
using ZWaveController;
using ZWaveController.Services;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class CommandsQueueTests : ControllerTestBase
    {
       [Test]
        public void CommandsQueueTests_AddReleaseCommandQueue_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            ApplicationSecondary.SetNodeInformationModel.IsListening = false;
            CommandsFactory.CommandRunner.Execute(((SetNodeInformationViewModel)ApplicationSecondary.SetNodeInformationModel).SetNodeInformationCommand, null);
            AddSupportCommandClass(ApplicationSecondary, COMMAND_CLASS_WAKE_UP_V2.ID);

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var node = ApplicationPrimary.ConfigurationItem.Node.FirstOrDefault(i => i.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.ConfigurationItem.Network.SetWakeupInterval(node.NodeTag, true);
            Assert.IsFalse(ApplicationPrimary.CommandQueueCollection.Any(), "Commands Queue not Empty");

            //Act.
            CommandsFactory.CommandRunner.ExecuteAsync(((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOnCommand, null);
            Thread.Sleep(100);
            CommandsFactory.CommandRunner.ExecuteAsync(((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOnCommand, null);
            Thread.Sleep(100);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == 2, "Commands Queue Empty");

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            SelectSecondaryNode(ApplicationSecondary, ApplicationPrimary);
            CommandsFactory.CommandRunner.ExecuteAsync(((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SendWakeUpNotificationCommand, null);
            Thread.Sleep(500);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == 0, "Commands Queue Empty");
        }

        [Test]
        public void CommandsQueueTests_AddReleaseDifferentCommands_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            ApplicationSecondary.SetNodeInformationModel.IsListening = false;
            CommandsFactory.CommandRunner.Execute(((SetNodeInformationViewModel)ApplicationSecondary.SetNodeInformationModel).SetNodeInformationCommand, null);
            AddSupportCommandClass(ApplicationSecondary, COMMAND_CLASS_WAKE_UP_V2.ID);

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var node = ApplicationPrimary.ConfigurationItem.Node.FirstOrDefault(i => i.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.ConfigurationItem.Network.SetWakeupInterval(node.NodeTag, true);
            Assert.IsFalse(ApplicationPrimary.CommandQueueCollection.Any(), "Commands Queue not Empty");

            //Act.
            var commandsList = new List<byte[]>();
            var ccm = (CommandClassesViewModel)ApplicationPrimary.CommandClassesModel;
            ccm.Payload = new COMMAND_CLASS_ASSOCIATION_V2.ASSOCIATION_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_VERSION_V3.VERSION_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ZWAVEPLUS_INFO_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ALARM_V2.ALARM_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ANTITHEFT_V2.ANTITHEFT_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ASSOCIATION_COMMAND_CONFIGURATION.COMMAND_CONFIGURATION_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ASSOCIATION_GRP_INFO_V3.ASSOCIATION_GROUP_INFO_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_ASSOCIATION_GRP_INFO_V3.ASSOCIATION_GROUP_NAME_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_BATTERY.BATTERY_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);

            ccm.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            commandsList.Add(ccm.Payload);
            CommandsFactory.CommandRunner.ExecuteAsync(ccm.SendCommand, null);
            Thread.Sleep(200);
           
            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == commandsList.Count, "Commands Queue Empty");

            //Act.
            ApplicationSecondary.LogDialog.Clear();
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            SelectSecondaryNode(ApplicationSecondary, ApplicationPrimary);
            CommandsFactory.CommandRunner.ExecuteAsync(((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SendWakeUpNotificationCommand, null);
            Thread.Sleep(commandsList.Count * 1000);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == 0, "Commands Queue Empty");
            var processed = ApplicationSecondary.LogDialog.Queue.Where(x => x.LogRawData != null);
            var count = commandsList.Where(i => processed.Any(j => j.LogRawData.RawData.SequenceEqual(i)));
            Assert.IsTrue(commandsList.All(i=> processed.Any(j=>j.LogRawData.RawData.SequenceEqual(i))), "Not All Commands Processed");
        }

        [Test]
        public void CommandsQueueTests_DeleteCommandFromQueue_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            ApplicationSecondary.SetNodeInformationModel.IsListening = false;
            CommandsFactory.CommandRunner.Execute(((SetNodeInformationViewModel)ApplicationSecondary.SetNodeInformationModel).SetNodeInformationCommand, null);
            AddSupportCommandClass(ApplicationSecondary, COMMAND_CLASS_WAKE_UP_V2.ID);

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var node = ApplicationPrimary.ConfigurationItem.Node.FirstOrDefault(i => i.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.ConfigurationItem.Network.SetWakeupInterval(node.NodeTag, true);
            Assert.IsFalse(ApplicationPrimary.CommandQueueCollection.Any(), "Commands Queue not Empty");
            var command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOnCommand;
            ApplicationPrimary.CommandQueueCollection = new ObservableCollection<ISelectableItem<QueueItem>> {
                new SelectableItem<QueueItem>(new QueueItem(ApplicationSecondary.Controller.Network.NodeTag,  command, 200)),
                    new SelectableItem<QueueItem>(new QueueItem(ApplicationSecondary.Controller.Network.NodeTag,  command, 200))
            };

            //Act.
            ApplicationPrimary.CommandQueueCollection.First().IsSelected = true;
            CommandsFactory.CommandRunner.Execute(((MainViewModel)ApplicationPrimary).CommandQueueViewModel.DeleteCommand, null);
            
            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == 1, "Commands Queue Empty");
        }

        [Test]
        public void CommandsQueueTests_ClearCommandQueue_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            ApplicationSecondary.SetNodeInformationModel.IsListening = false;
            CommandsFactory.CommandRunner.Execute(((SetNodeInformationViewModel)ApplicationSecondary.SetNodeInformationModel).SetNodeInformationCommand, null);            
            AddSupportCommandClass(ApplicationSecondary, COMMAND_CLASS_WAKE_UP_V2.ID);

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            SelectSecondaryNode(ApplicationPrimary, ApplicationSecondary);
            var node = ApplicationPrimary.ConfigurationItem.Node.FirstOrDefault(i => i.Id == ApplicationSecondary.Controller.Id);
            ApplicationPrimary.ConfigurationItem.Network.SetWakeupInterval(node.NodeTag, true);
            Assert.IsFalse(ApplicationPrimary.CommandQueueCollection.Any(), "Commands Queue not Empty");
            var command = ((NetworkManagementViewModel)ApplicationPrimary.NetworkManagementModel).BasicSetOnCommand;
            ApplicationPrimary.CommandQueueCollection = new ObservableCollection<ISelectableItem<QueueItem>> {
                new SelectableItem<QueueItem>(new QueueItem(ApplicationSecondary.Controller.Network.NodeTag,  command, 200)),
                    new SelectableItem<QueueItem>(new QueueItem(ApplicationSecondary.Controller.Network.NodeTag,  command, 200))
            };

            //Act.
            CommandsFactory.CommandRunner.Execute(((MainViewModel)ApplicationPrimary).CommandQueueViewModel.ClearCommand, null);

            //Assert.
            Assert.IsTrue(ApplicationPrimary.CommandQueueCollection.Count() == 0, "Commands Queue Empty");
        }
    }
}