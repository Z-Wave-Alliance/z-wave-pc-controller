/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWaveController;
using ZWaveControllerUI.Bind;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class SendDataSettingsTests : ControllerTestBase
    {
        int DELAY_ON_UI_UPDATED_AFTER_BUTTON_PRESS = 400;

        [Test]
        public void DefaultValues_OnInit_AreSet()
        {
            //Arr.
            var expectedDelayWakeUpNoMoreInformationMs = 100;
            var expectedMaxBytesPerFrameSize = 26; // s0
            var expectedMaxSegmentSize = 0;  // from device payload - for Transport service
            var expectedRequestsTimeoutMs = 5000;
            var expectedDelayResponseMs = 0;
            var testedVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            //Act.
            var exDelayWakeUpNoMoreInformationMs = testedVM.DelayWakeUpNoMoreInformationMs;
            var exMaxBytesPerFrameSize = testedVM.S0MaxBytesPerFrameSize;
            var exMaxSegmentSize = testedVM.TransportServiceMaxSegmentSize;
            var exRequestsTimeoutMs = testedVM.RequestsTimeoutMs;
            var exDelayResponseMs = testedVM.DelayResponseMs;

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.CommandOk.Execute(null);

            //Assert:
            //on init
            Assert.AreEqual(expectedDelayWakeUpNoMoreInformationMs, exDelayWakeUpNoMoreInformationMs);
            Assert.AreEqual(expectedMaxBytesPerFrameSize, exMaxBytesPerFrameSize);
            Assert.AreEqual(expectedMaxSegmentSize, exMaxSegmentSize);
            Assert.AreEqual(expectedRequestsTimeoutMs, exRequestsTimeoutMs);
            Assert.AreEqual(expectedDelayResponseMs, exDelayResponseMs);
            //after applying
            Assert.AreEqual(exDelayWakeUpNoMoreInformationMs, testedVM.DelayWakeUpNoMoreInformationMs);
            Assert.AreEqual(exMaxBytesPerFrameSize, testedVM.S0MaxBytesPerFrameSize);
            Assert.AreEqual(exMaxSegmentSize, testedVM.TransportServiceMaxSegmentSize);
            Assert.AreEqual(exRequestsTimeoutMs, testedVM.RequestsTimeoutMs);
            Assert.AreEqual(exDelayResponseMs, testedVM.DelayResponseMs);
        }

        [Test]
        public void DelayWakeUpNoMoreInformationMs_NonListeningReceiver_ResponceDelayed()
        {
            //Arrange.
            var DELAY_MS = 1000;
            byte[] expectedData = new COMMAND_CLASS_WAKE_UP_V2.WAKE_UP_NO_MORE_INFORMATION();
            //Make sleeping device:
            AddWakeUpSupport(ApplicationSecondary);
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            //Apply settings:
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.DelayWakeUpNoMoreInformationMs = DELAY_MS;
            sendDataSettingsVM.CommandOk.Execute(null);
            //Act
            ApplicationSecondary.SelectedNode = ApplicationSecondary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationPrimary.Controller.Id);
            ApplicationSecondary.SelectedNode.IsSelected = true;
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).SendWakeUpNotificationCommand;
            cmdRef.Execute(null);
            var dt = DateTime.Now;
            Thread.Sleep(DELAY_MS + DELAY_ON_UI_UPDATED_AFTER_BUTTON_PRESS);

            //Assert
            var qr = ApplicationSecondary.LogDialog.Queue;
            Assert.That(qr.Count > 4, Is.True.After(DELAY_MS, 100));
            var matchItem = qr.FirstOrDefault(x => x.LogRawData != null && x.LogRawData.RawData.SequenceEqual(expectedData));
            Assert.IsNotNull(matchItem);
            var diff = matchItem.Timestamp - dt;
            Assert.GreaterOrEqual(diff.TotalMilliseconds, DELAY_MS);
        }

        [Test]
        public void DelayWakeUpNoMoreInformationMs_ListeningReceiver_ResponceNotDelayed()
        {
            var DELAY_MS = 1000;
            byte[] expectedData = new COMMAND_CLASS_VERSION.VERSION_REPORT();
            //Arrange.
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            //Apply settings:
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.DelayWakeUpNoMoreInformationMs = DELAY_MS;
            sendDataSettingsVM.CommandOk.Execute(null);
            //Act
            ApplicationSecondary.SelectedNode = ApplicationSecondary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationPrimary.Controller.Id);
            ApplicationSecondary.SelectedNode.IsSelected = true;
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).VersionGetCommand;
            var dt = DateTime.Now;
            cmdRef.Execute(null);
            Thread.Sleep(DELAY_ON_UI_UPDATED_AFTER_BUTTON_PRESS);

            //Assert
            var qr = ApplicationSecondary.LogDialog.Queue;
            Assert.That(qr.Count > 4, Is.True.After(DELAY_MS, 100));
            var matchItem = qr.FirstOrDefault(x => x.LogRawData != null && x.LogRawData.RawData.Take(2).SequenceEqual(expectedData.Take(2)));
            Assert.IsNotNull(matchItem);
            var diff = matchItem.Timestamp - dt;
            Assert.Less(diff.TotalMilliseconds, DELAY_MS);
        }

        [Test]
        public void DelayResponce_RequestFrom2nd_ResponceDelayed()
        {
            // Arrange.
            var DELAY_MS = 1000;
            byte[] expectedData = new COMMAND_CLASS_VERSION.VERSION_REPORT();
            SetupAsIncluded(ApplicationPrimary, ApplicationSecondary);
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.DelayResponseMs = DELAY_MS;
            sendDataSettingsVM.CommandOk.Execute(null);

            //Act
            ApplicationSecondary.SelectedNode = ApplicationSecondary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == ApplicationPrimary.Controller.Id);
            ApplicationSecondary.SelectedNode.IsSelected = true;
            CommandsFactory.CurrentSourceId = ApplicationSecondary.DataSource.SourceId;
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = ((NetworkManagementViewModel)ApplicationSecondary.NetworkManagementModel).VersionGetCommand;
            var dt = DateTime.Now;
            cmdRef.Execute(null);
            Thread.Sleep(DELAY_MS + DELAY_ON_UI_UPDATED_AFTER_BUTTON_PRESS);

            // Assert.
            var qr = ApplicationSecondary.LogDialog.Queue;
            Assert.That(qr.Count > 4, Is.True.After(DELAY_MS, 100));
            var matchItem = qr.FirstOrDefault(x => x.LogRawData != null && x.LogRawData.RawData.Take(2).SequenceEqual(expectedData.Take(2)));
            Assert.IsNotNull(matchItem);
            var diff = matchItem.Timestamp - dt;
            Assert.GreaterOrEqual(diff.TotalMilliseconds, DELAY_MS);
        }

        [Test]
        public void RequestsTimeoutMs_NewValue_SetInNetwork()
        {
            //Arrange.
            var expected = 654;
            var oldVal = ApplicationPrimary.Controller.Network.RequestTimeoutMs;
            //Act
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.RequestsTimeoutMs = expected;
            sendDataSettingsVM.CommandOk.Execute(null);

            // Assert.
            var newVal = ApplicationPrimary.Controller.Network.RequestTimeoutMs;
            Assert.AreEqual(expected, newVal);
            Assert.AreNotEqual(expected, oldVal);
        }

        [Test]
        public void MaxBytesPerFrameSize_NewValue_SetInNetwork()
        {
            //Arrange.
            var expected = 33;
            var oldVal = ApplicationPrimary.Controller.Network.S0MaxBytesPerFrameSize;
            //Act
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            sendDataSettingsVM.S0MaxBytesPerFrameSize = expected;
            sendDataSettingsVM.CommandOk.Execute(null);

            // Assert.
            var newVal = ApplicationPrimary.Controller.Network.S0MaxBytesPerFrameSize;
            Assert.AreEqual(expected, newVal);
            Assert.AreNotEqual(expected, oldVal);
        }

        [Test]
        public void SupervisionReportStatusResponse_NewValue_SetInNetwork()
        {
            //Arrange.
            var def = SupervisionReportStatuses.SUCCESS;
            var expected = SupervisionReportStatuses.NO_SUPPORT;
            var oldVal = ApplicationPrimary.Controller.Network.SupervisionReportStatusResponse;
            //Act
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var sendDataSettingsVM = (SendDataSettingsViewModel)ApplicationPrimary.SendDataSettingsModel;
            var prevVal = sendDataSettingsVM.SupervisionReportStatusResponse;
            sendDataSettingsVM.SupervisionReportStatusResponse = expected;
            sendDataSettingsVM.CommandOk.Execute(null);

            // Assert.
            var newVal = ApplicationPrimary.Controller.Network.SupervisionReportStatusResponse;
            Assert.AreEqual(expected, newVal);
            Assert.AreNotEqual(expected, oldVal);
            Assert.AreNotEqual(def, prevVal);
        }
    }
}