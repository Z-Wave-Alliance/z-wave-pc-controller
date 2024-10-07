/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using Utils;
using ZWave.BasicApplication;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.EmulatedLink;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Layers;
using ZWave.Layers.Session;
using ZWaveController;
using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;
using ZWaveControllerUI.Models;
using Arg = NSubstitute.Arg;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class ConnectTests : ControllerTestBase
    {
        protected override void ConnectToControllerSession(IApplicationModel applicationModel, Controller fakeDevice)
        {
            // Do not emulate connect command in this tests.
            return;
        }

        private SerialPortDataSource _selectedDataSource = new SerialPortDataSource("COM1");

        [TestCase]
        public void Connect_PassNullToCommand_LastCommandFailed()
        {
            //Arrange.
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            ApplicationPrimary.AppSettings.SourceOnStartup = null;

            //Act.
            settingsVM.ConnectCommand.Execute(null);

            //Assert.
            Assert.IsFalse(
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationPrimary.DataSource.SourceId));
            Assert.AreEqual(CommandExecutionResult.Failed, ApplicationPrimary.LastCommandExecutionResult);
            ApplicationPrimary.DidNotReceive().NotifyControllerChanged(NotifyProperty.ToggleSource, Arg.Any<object>());
        }

        [TestCase]
        public void Connect_HasStartupSource_ToggleSourceNotifyReceived()
        {
            //Arrange.
            var fakeControllerSession = Substitute.For<IControllerSession>();
            fakeControllerSession.Connect(Arg.Any<IDataSource>()).Returns(CommunicationStatuses.Done);

            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(fakeControllerSession);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            ApplicationPrimary.AppSettings.SourceOnStartup = _selectedDataSource;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ConnectCommand.Execute(null);

            //Assert.
            Assert.IsTrue(
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationPrimary.DataSource.SourceId));
            Assert.AreEqual(CommandExecutionResult.OK, ApplicationPrimary.LastCommandExecutionResult);
            ApplicationPrimary.AppSettings.Received().LastUsedDevice = _selectedDataSource.SourceName;
            Assert.IsNull(ApplicationPrimary.AppSettings.SourceOnStartup);
            ApplicationPrimary.AppSettings.Received().SaveSettings();
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleSource, Arg.Any<object>());
        }

        [TestCase]
        public void Connect_SaveLastUsedDeviceSecondary_SettingsSaved()
        {
            //Arrange.
            var fakeControllerSession = Substitute.For<IControllerSession>();
            fakeControllerSession.Connect(Arg.Any<IDataSource>()).Returns(CommunicationStatuses.Done);

            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(fakeControllerSession);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            ApplicationPrimary.AppSettings.SaveLastUsedDeviceSecondary = true;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            ApplicationPrimary.AppSettings.Received().LastUsedDeviceAlt = _selectedDataSource.SourceName;
            ApplicationPrimary.AppSettings.DidNotReceive().LastUsedDevice = _selectedDataSource.SourceName;
            ApplicationPrimary.AppSettings.Received().SaveSettings();
        }

        [TestCase]
        public void Connect_ConnectToDeviceDone_ToggleSourceNotifyReceived()
        {
            //Arrange.
            var fakeControllerSession = Substitute.For<IControllerSession>();
            fakeControllerSession.Connect(Arg.Any<IDataSource>()).Returns(CommunicationStatuses.Done);

            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(fakeControllerSession);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;

            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            var ConnectCommand = settingsVM.ConnectCommand;
            ConnectCommand.SenderHistoryService = Substitute.For<ISenderHistoryService>();
            ConnectCommand.PredefinedPayloadsService = Substitute.For<IPredefinedPayloadsService>();

            //Act.
            ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            Assert.IsTrue(
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationPrimary.DataSource.SourceId));
            Assert.AreEqual(CommandExecutionResult.OK, ApplicationPrimary.LastCommandExecutionResult);
            ApplicationPrimary.AppSettings.Received().LastUsedDevice = _selectedDataSource.SourceName;
            ApplicationPrimary.AppSettings.Received().SaveSettings();
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleSource, Arg.Any<object>());
        }

        [TestCase]
        public void Connect_ReconnectToDevice_DisconnectFirst()
        {
            //Arrange.
            Connect_ConnectToDeviceDone_ToggleSourceNotifyReceived();

            var fakeControllerSession = ControllerSessionsContainer.ControllerSessionCreator.CreateControllerSession(null, null);
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;
            var ConnectCommand = settingsVM.ConnectCommand;
            ConnectCommand.SenderHistoryService = Substitute.For<ISenderHistoryService>();
            ConnectCommand.PredefinedPayloadsService = Substitute.For<IPredefinedPayloadsService>();
            //Act.
            ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            fakeControllerSession.Received(1).Disconnect();
            Assert.AreEqual(CommandExecutionResult.OK, ApplicationPrimary.LastCommandExecutionResult);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleSource, Arg.Any<object>());
        }

        [TestCase(CommunicationStatuses.Busy)]
        [TestCase(CommunicationStatuses.Failed)]
        public void Connect_CantConnectToDevice_LastCommandFailed(CommunicationStatuses failStatus)
        {
            //Arrange.
            var fakeControllerSession = Substitute.For<IControllerSession>();
            fakeControllerSession.Connect(Arg.Any<IDataSource>()).Returns(failStatus);
            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(fakeControllerSession);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            Assert.IsFalse(
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationPrimary.DataSource.SourceId));
            Assert.AreEqual(CommandExecutionResult.Failed, ApplicationPrimary.LastCommandExecutionResult);
            fakeControllerSession.Received(1).Disconnect();
            ApplicationPrimary.DidNotReceive().NotifyControllerChanged(NotifyProperty.ToggleSource, Arg.Any<object>());
        }

        [TestCase]
        public void Connect_OnlyTransportIsFake_NotifiersTrigerred()
        {
            //Arrange.
            var realControllerSession = new BasicControllerSession(ApplicationPrimary);
            var fakeBasicApplicationLayer = new BasicApplicationLayer(new SessionLayer(), new BasicFrameLayer(), new BasicLinkTransportLayer());

            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(realControllerSession);
            fakeControllerSessionCreator.CreateBasicApplicationLayer(Arg.Any<ISessionLayer>(), Arg.Any<IDataSource>())
                .Returns(fakeBasicApplicationLayer);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            Assert.IsTrue(
                ControllerSessionsContainer.ControllerSessions.ContainsKey(ApplicationPrimary.DataSource.SourceId));
            Assert.AreEqual(CommandExecutionResult.OK, ApplicationPrimary.LastCommandExecutionResult);
            ApplicationPrimary.AppSettings.Received().LastUsedDevice = _selectedDataSource.SourceName;
            ApplicationPrimary.AppSettings.Received().SaveSettings();
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ToggleSource,
                Arg.Is<object>(obj => obj != null));
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ControllerInfo);
        }

        [TestCase]
        public void Connect_OnlyTransportIsFake_BasicControllerSessionLifelineIsCorrect()
        {
            //Arrange.
            var expectedMaxPayloadSize = 46; //oiait_expectedMaxPayloadSize
            var realControllerSession = new BasicControllerSession(ApplicationPrimary);
            var fakeBasicApplicationLayer = new BasicApplicationLayer(new SessionLayer(), new BasicFrameLayer(), new BasicLinkTransportLayer());
            var fakeConfigurationItem = Substitute.For<ConfigurationItem>();
            ApplicationPrimary.ConfigurationItem = fakeConfigurationItem;
            var fakeControllerSessionCreator = Substitute.For<ControllerSessionCreator>();
            fakeControllerSessionCreator.CreateControllerSession(Arg.Any<IDataSource>(), Arg.Any<IApplicationModel>())
                .Returns(realControllerSession);
            fakeControllerSessionCreator.CreateBasicApplicationLayer(Arg.Any<ISessionLayer>(), Arg.Any<IDataSource>())
                .Returns(fakeBasicApplicationLayer);

            ControllerSessionsContainer.ControllerSessionCreator = fakeControllerSessionCreator;
            var settingsVM = (SettingsViewModel)ApplicationPrimary.TraceCapture.TraceCaptureSettingsModel;

            //Act.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceName;
            settingsVM.ConnectCommand.Execute(_selectedDataSource);

            //Assert.
            ApplicationPrimary.Received().ClearCommandQueueCollection();
            //ApplicationPrimary.Received().TraceCapture.Init(Arg.Any<string>());
            // LoadConfigurationItem() - Security model is set inside - dunno what to do 
            // InitNodes() - 
            ApplicationPrimary.Received().ConfigurationItem.FillNodes(Arg.Any<NodeTag[]>());
            // if (_controller.Network.GetNodeInfo(node).IsEmpty) is false
            ApplicationPrimary.Received().ConfigurationItem.RefreshNodes();
            Assert.IsNotNull(ApplicationPrimary.SelectedNode);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.NodesList);
            Assert.IsNotNull(realControllerSession.SupervisionManager);
            Assert.IsNotNull(realControllerSession.SecurityManager);
            Assert.IsNotNull((ApplicationPrimary.Controller as Device).DSK);
            Assert.IsNotNull(realControllerSession.SecurityManager.SecurityManagerInfo.DSKNeededCallback);
            Assert.IsNotNull(realControllerSession.SecurityManager.SecurityManagerInfo.KEXSetConfirmCallback);
            Assert.IsNotNull(realControllerSession.SecurityManager.SecurityManagerInfo.CsaPinCallback);
            Assert.IsNotNull(realControllerSession.TransportServiceManager);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.ControllerInfo);
            Assert.AreEqual(expectedMaxPayloadSize, ApplicationPrimary.Controller.Network.TransportServiceMaxSegmentSize);
        }
    }
}