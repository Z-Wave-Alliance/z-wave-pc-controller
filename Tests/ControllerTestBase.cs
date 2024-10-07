/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.ObjectModel;
using BasicApplicationTests;
using NSubstitute;
using NUnit.Framework;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWave.BasicApplication;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.TransportService;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Models;
using ZWaveControllerUI.Models;
using System.Linq;
using ZWave.Security;
using ZWaveController.Configuration;
using ZWave.Configuration;
using System.IO;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class ControllerTestBase : TestBase
    {
        public const byte MAX_FRAME_SIZE = 0x40;
        public const byte DEVICE_CAPABILITY = 0xD3;
        private readonly byte[] NETWORK_KEY_S2_C0 = new byte[] { 0xc0, 0x07, 0x66, 0xd0, 0xa1, 0x66, 0x65, 0x1f, 0x64, 0x67, 0xe2, 0x01, 0xce, 0xa8, 0x07, 0x5b };
        private readonly byte[] NETWORK_KEY_S2_C1 = new byte[] { 0xc1, 0x62, 0xad, 0xfa, 0xe0, 0x62, 0xea, 0xf8, 0x40, 0x8c, 0x31, 0x97, 0x66, 0x7e, 0xda, 0x0e };
        private readonly byte[] NETWORK_KEY_S2_C2 = new byte[] { 0xc2, 0xfb, 0xdc, 0x1f, 0x16, 0x54, 0x5c, 0x09, 0xf2, 0x26, 0xa6, 0xa0, 0xa1, 0x9a, 0x28, 0x4d };
        private readonly byte[] NETWORK_KEY_S0 = new byte[] { 0x00, 0xb0, 0xe5, 0xd2, 0xe9, 0xd9, 0xea, 0xbc, 0xb4, 0x0c, 0x0d, 0x1b, 0xbe, 0x83, 0xa8, 0xba };
        private byte[] supportedCommandClasses =
            new byte[]
            {
                COMMAND_CLASS_ZWAVEPLUS_INFO.ID,
                COMMAND_CLASS_BASIC.ID,
                COMMAND_CLASS_VERSION.ID,
                COMMAND_CLASS_SUPERVISION.ID,
                COMMAND_CLASS_SECURITY.ID,
                COMMAND_CLASS_SECURITY_2.ID
            };

        protected IApplicationModel ApplicationPrimary;
        protected IApplicationModel ApplicationSecondary;
        protected IApplicationModel ApplicationSecondary2;

        [SetUp]
        public virtual void SetUp()
        {
            var configMock = Substitute.For<IConfig>();
            configMock.ControllerConfiguration.Dialogs = new System.Collections.Generic.List<DialogSettings>();
            configMock.LoadZWaveDefinition(Arg.Any<string>()).Returns(new ZWaveDefinition());
            configMock.TxOptions = TXO;
            ControllerSessionsContainer.Config = configMock;
            CommandsFactory.CommandRunner = new CommandRunner();
            _transport.ResetModule(_ctrlFirst.SessionId);
            _ctrlFirst.MemoryGetId();
            _transport.ResetModule(_ctrlSecond.SessionId);
            _ctrlSecond.MemoryGetId();

            ApplicationPrimary = StartApplicationOnFakeDevice(_ctrlFirst);
            ApplicationSecondary = StartApplicationOnFakeDevice(_ctrlSecond);
        }

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var item in ControllerSessionsContainer.ControllerSessions)
            {
                if (item.Value is BasicControllerSession bcs)
                {
                    bcs.StopListener();
                    bcs.StopSupportTasks();
                }
                if (item.Value.Controller.Network != null)
                {
                    var configFileName = ConfigurationItem.GetItemFileName(item.Value.Controller.HomeId, item.Value.Controller.Network.NodeTag);
                    if (!string.IsNullOrEmpty(configFileName))
                    {
                        // Remove config item from previous usages
                        File.Delete(configFileName);
                    }
                }
            }
            ControllerSessionsContainer.ControllerSessions.Clear();
        }

        protected IApplicationModel StartApplicationOnFakeDevice(Controller fakeDevice)
        {
            fakeDevice.Library = Libraries.ControllerStaticLib;
            var applicationModel = CreateApplicationModel(fakeDevice);
            ConnectToControllerSession(applicationModel, fakeDevice);

            return applicationModel;
        }

        private IApplicationModel CreateApplicationModel(Controller fakeDevice)
        {
            //OnStartup var dispatcherFake = new DirectDispatcher();
            var subscribeCollectionFactoryStub = new UListFactory();
            var dispatcherFake = new DirectDispatcher();
            var applicationModel = Substitute.For<MainViewModel>(new object[] { subscribeCollectionFactoryStub, dispatcherFake });
            applicationModel.AppSettings = Substitute.For<IAppSettings>();
            applicationModel.SaveFileDialogModel = Substitute.For<SaveFileDialogViewModel>(applicationModel);
            applicationModel.OpenFileDialogModel = Substitute.For<OpenFileDialogViewModel>(applicationModel);
            (applicationModel.SmartStartModel as SmartStartViewModel).ScanDSKDialog = Substitute.For<SmartStartScanDSKViewModel>(applicationModel);
            //new MainViewModel(subscribeCollectionFactoryStub, dispatcherFake)
            //{
            //    RefreshCurrentViewLayout = () => { }
            //};
            //var applicationModel = new MainViewModel(subscribeCollectionFactoryStub, dispatcherFake);            
            //applicationModel.ShowMessageDialog(Arg.Any<string>(), Arg.Any<string>());
            applicationModel.RefreshCurrentViewLayout = () => { /*MainWindow.CurrentView.UpdateLayout();*/ };
            applicationModel.DataSource = fakeDevice.DataSource;
            applicationModel.Controller = fakeDevice;

            return applicationModel;
        }

        protected virtual void ConnectToControllerSession(IApplicationModel applicationModel, Controller fakeDevice)
        {
            //ConnectCommand 
            var controllerSession = Substitute.For<BasicControllerSession>(applicationModel);
            fakeDevice.ApplicationNodeInformation(DeviceOptions.Listening, 1, 1, supportedCommandClasses);
            controllerSession._device = fakeDevice;
            controllerSession.Logger = new LogService(applicationModel);
            controllerSession.SenderHistoryService = Substitute.For<ISenderHistoryService>();
            controllerSession.PredefinedPayloadsService = Substitute.For<IPredefinedPayloadsService>();
            ControllerSessionsContainer.ControllerSessions.Add(fakeDevice.DataSource.SourceId, controllerSession);
            //LoadConfigurationItem: , InitNodes,
            //var confItem = Substitute.For<ConfigurationItem>();
            var confItem = ConfigurationItem.Load(fakeDevice.Network, applicationModel.CreateSelectableItem);
            var controllerNode = new NodeTag(fakeDevice.Id);
            confItem.CreateSelectedNodeItem = applicationModel.CreateSelectableItem;
            confItem.Nodes = new ObservableCollection<ISelectableItem<NodeTag>>
            {
                new SelectableItem<NodeTag>(controllerNode),
            };
            confItem.Node = new Collection<Node> { new Node(controllerNode) };
            confItem.Network = fakeDevice.Network;
            applicationModel.ConfigurationItem = confItem;
            applicationModel.Controller.Network.HomeId = fakeDevice.HomeId;
            applicationModel.Controller.Network.SetNodeInfo(controllerNode, new NodeInfo() { Capability = DEVICE_CAPABILITY });
            applicationModel.Controller.Network.SetCommandClasses(controllerNode, supportedCommandClasses);
            applicationModel.Controller.Network.SetSecuritySchemes(controllerNode, SecuritySchemeSet.ALLS2);
            applicationModel.Controller.Network.IsEnabledS0 = true;
            applicationModel.Controller.Network.IsEnabledS2_ACCESS = true;
            applicationModel.Controller.Network.IsEnabledS2_AUTHENTICATED = true;
            applicationModel.Controller.Network.IsEnabledS2_UNAUTHENTICATED = true;
            applicationModel.Controller.Network.TransportServiceMaxSegmentSize = MAX_FRAME_SIZE;
            applicationModel.Controller.Version = "Z-Wave 6.06";
            applicationModel.Controller.Network.RequestTimeoutMs = 99;

            InitSupervisionManager(fakeDevice);
            //controllerSession.InitSubstituteManagers();
            controllerSession.SecurityManager = SetUpSecurity(fakeDevice, NETWORK_KEY_S2_C0, NETWORK_KEY_S2_C1, NETWORK_KEY_S2_C2, NETWORK_KEY_S0);
            InitTransportServiceManager(applicationModel, controllerSession);

            controllerSession.StartSupportTasks();
            controllerSession.StartListener();
            controllerSession.DataSource = applicationModel.DataSource;
        }

        private void InitSupervisionManager(Controller fakeDevice)
        {
            var supervisionManagerFake = new SupervisionManager(fakeDevice.Network, (x, y) => true);
            //{
            //    if (nodeId > 0 && nodeId != 0xFF)
            //    {
            //        if (_controller.Network.HasCommandClass(new NodeTag(nodeId), COMMAND_CLASS_SUPERVISION.ID))
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //});
            //supervisionManager.SetWakeupDelayed = SetWakeupDelayed;
            //supervisionManager.UnSetWakeupDelayed = UnSetWakeupDelayed;
            //fakeDevice.SessionClient.AddSubstituteManager(new Crc16EncapManager());
            fakeDevice.SessionClient.AddSubstituteManager(supervisionManagerFake, new SupervisionReportTask(fakeDevice.Network, TXO));
        }

        private void InitTransportServiceManager(IApplicationModel applicationModel, BasicControllerSession controllerSession)
        {
            var transportServiceManagerInfoFake = new TransportServiceManagerInfo(TXO, x => true);
            var transportServiceManagerFake = new TransportServiceManager(applicationModel.Controller.Network, transportServiceManagerInfoFake);
            applicationModel.Controller.SessionClient.AddSubstituteManager(transportServiceManagerFake);
            controllerSession.TransportServiceManager = transportServiceManagerFake;
        }

        protected void SetupAsIncluded(IApplicationModel primaryAppModel, IApplicationModel secondaryAppModel)
        {
            _transport.SetUpModulesNetwork(primaryAppModel.Controller.SessionId, secondaryAppModel.Controller.SessionId);
            (primaryAppModel.Controller as Controller).MemoryGetId();
            (secondaryAppModel.Controller as Controller).MemoryGetId();
            var _primarySecurityManager = ((SecurityManager)primaryAppModel.Controller.SessionClient.GetSubstituteManager(typeof(SecurityManager))).SecurityManagerInfo;
            var _secondaryManager = ((SecurityManager)secondaryAppModel.Controller.SessionClient.GetSubstituteManager(typeof(SecurityManager))).SecurityManagerInfo;
            foreach (var scheme in SecuritySchemeSet.ALL)
            {
                _secondaryManager.SetNetworkKey(_primarySecurityManager.GetNetworkKey(scheme, false), scheme, false);
                if (scheme == SecuritySchemes.S0)
                {
                    _secondaryManager.ActivateNetworkKeyS0();
                    continue;
                }
                _secondaryManager.ActivateNetworkKeyS2ForNode(
                    new InvariantPeerNodeId(secondaryAppModel.Controller.Network.NodeTag, primaryAppModel.Controller.Network.NodeTag), scheme, false);
            }
            AddNodeToConfigItem(primaryAppModel, secondaryAppModel);
            AddNodeToConfigItem(secondaryAppModel, primaryAppModel);
            (primaryAppModel.Controller as Controller).ControllerCapability = (byte)ControllerCapabilities.IS_REAL_PRIMARY;
            (secondaryAppModel.Controller as Controller).ControllerCapability = (byte)ControllerCapabilities.IS_SECONDARY;
            _transport.SetControllerNetworkRole(primaryAppModel.Controller.SessionId, ControllerCapabilities.IS_REAL_PRIMARY);
            _transport.SetControllerNetworkRole(secondaryAppModel.Controller.SessionId, ControllerCapabilities.IS_SECONDARY);
        }

        private void UpdateSecurity(IApplicationModel model, NodeTag joinNode)
        {
            var _securityManagerInfo = ((SecurityManager)model.Controller.SessionClient.GetSubstituteManager(typeof(SecurityManager))).SecurityManagerInfo;

            _securityManagerInfo.Network.SetSecuritySchemes(SecuritySchemeSet.ALL);
            _securityManagerInfo.Network.SetSecuritySchemes(joinNode, SecuritySchemeSet.ALL);
            foreach (var scheme in SecuritySchemeSet.ALL)
            {
                _securityManagerInfo.ActivateNetworkKeyS2ForNode(new InvariantPeerNodeId(_securityManagerInfo.Network.NodeTag, joinNode), scheme, false);
            }
        }

        private void AddNodeToConfigItem(IApplicationModel dest, IApplicationModel source)
        {
            var destNet = dest.Controller.Network;
            var srcNet = source.Controller.Network;
            var secondaryNode = new NodeTag(source.Controller.Id);
            dest.ConfigurationItem.Nodes.Add(new SelectableItem<NodeTag>(secondaryNode));
            dest.ConfigurationItem.Node.Add(new Node(secondaryNode));
            destNet.SetNodeInfo(secondaryNode, new NodeInfo() { Capability = srcNet.GetNodeInfo().Capability });
            destNet.SetCommandClasses(secondaryNode, srcNet.GetCommandClasses());
            destNet.SetSecureCommandClasses(secondaryNode, srcNet.GetSecureCommandClasses());
            destNet.SetSecuritySchemes(secondaryNode, source.Controller.Network.GetSecuritySchemes());

            if (srcNet.HasCommandClass(COMMAND_CLASS_MULTI_CHANNEL_V2.ID))
            {
                var secondaryEP = new NodeTag(source.Controller.Id, 1);
                dest.ConfigurationItem.Nodes.Add(new SelectableItem<NodeTag>(secondaryEP));
                dest.ConfigurationItem.Node.Add(new Node(secondaryEP));
                destNet.SetNodeInfo(secondaryEP, new NodeInfo() { Capability = srcNet.GetNodeInfo().Capability });
                destNet.SetCommandClasses(secondaryEP, srcNet.GetCommandClasses());
            }
        }

        public void AddMultiChannelSupport(IApplicationModel applicationModel)
        {
            var controllerSession = (BasicControllerSession)ControllerSessionsContainer.ControllerSessions[applicationModel.DataSource.SourceId];
            controllerSession.StopSupportTasks();
            var ccs = applicationModel.Controller.Network.GetCommandClasses();
            var newSupport = ccs.Union(new byte[] { COMMAND_CLASS_MULTI_CHANNEL_V2.ID }).ToArray();
            applicationModel.Controller.Network.SetCommandClasses(newSupport);
            controllerSession.StartSupportTasks();
        }

        public void AddWakeUpSupport(IApplicationModel applicationModel)
        {
            var controllerSession = (BasicControllerSession)ControllerSessionsContainer.ControllerSessions[applicationModel.DataSource.SourceId];
            controllerSession.StopSupportTasks();
            var ccs = applicationModel.Controller.Network.GetCommandClasses();
            var newSupport = ccs.Union(new byte[] { COMMAND_CLASS_WAKE_UP_V2.ID }).ToArray();
            applicationModel.Controller.Network.SetCommandClasses(newSupport);
            controllerSession.StartSupportTasks();
        }

        public void AddSupportCommandClass(IApplicationModel applicationModel, byte commandClass)
        {
            var controllerSession = (BasicControllerSession)ControllerSessionsContainer.ControllerSessions[applicationModel.DataSource.SourceId];
            controllerSession.StopSupportTasks();
            var ccs = applicationModel.Controller.Network.GetCommandClasses();
            var newSupport = ccs.Union(new byte[] { commandClass }).ToArray();
            applicationModel.Controller.Network.SetCommandClasses(newSupport);
            controllerSession.StartSupportTasks();
        }

        public void SelectSecondaryNode(IApplicationModel primary, IApplicationModel secondary)
        {
            primary.SelectedNode = primary.ConfigurationItem.Nodes.FirstOrDefault(x => x.Item.Id == secondary.Controller.Id);
            primary.SelectedNode.IsSelected = true;
            primary.NetworkManagementModel.SelectedNodeItems = primary.ConfigurationItem.Nodes.Where(x => x.IsSelected).Select(x => x.Item).ToArray();
        }
    }
}