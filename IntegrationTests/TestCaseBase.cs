/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Utils;
using Utils.UI.MVVM;
using ZWave.BasicApplication.Devices;
using ZWave.CommandClasses;
using ZWave.Enums;
using ZWave.Layers;
using ZWaveController.Models;
using ZWaveController.ViewModels;
using ZWaveControllerUI.Bind;
using ZWave.BasicApplication.Operations;

namespace IntegrationTests
{
    public class TestCaseBase
    {
        public const byte A_IncusionNodeId = 1;
        public const byte B_JoinNodeId = 2;

        public const int EXPECTED_NODESCOUNT_SETDAFAULT = 1; // expected(EXP) nodes count(NC) for SetDafult action
        public const CommandExecutionResult EXPEXTED_COMMAND_EXECUTION_RESULT = CommandExecutionResult.OK;
        public const CommandExecutionResult EXPECTED_COMMAND_CANCELED = CommandExecutionResult.Canceled;
        public const string TEMP_FILENAME = "MNVBackupRestore1348.zip";


        protected MainViewModel _MainVMPrimary;
        protected MainViewModel _MainVMSecondary;
        protected MainViewModel _MainVMBridge;
        private bool _IsUsingZFinger = false;
        protected ZFingerDevice _ZFingerDevice;
        protected SecurityUnderTest _MainSecurityScheme { get; private set; }

        //Default:
        public ControllerRoles EXPECTED_ROLE_MAIN = ControllerRoles.RealPrimary;
        public ControllerRoles EXPECTED_ROLE_SECOND = ControllerRoles.Secondary | ControllerRoles.OtherNetwork;

        public TestCaseBase(ApiTypeUnderTest type, SecurityUnderTest securityUnderTest, bool isUsingZFinger)
        {
            if (type == ApiTypeUnderTest.Zip)
            {
                EXPECTED_ROLE_MAIN = ControllerRoles.SIS;
                EXPECTED_ROLE_SECOND = ControllerRoles.OtherNetwork | ControllerRoles.NodeIdServerPresent | ControllerRoles.Inclusion;
                Assert.False(string.IsNullOrEmpty(SetUpFixture.Config.ZipSource.AddressIPv6), "Empty GW port address");
            }
            _MainSecurityScheme = securityUnderTest;
            SetUpFixture.SelectMainDataSource(type);
            _IsUsingZFinger = isUsingZFinger;
        }

        #region Base

        [SetUp]
        public void SetUp()
        {
            Tools.CreateDebugLogFile(this.GetType().Name + ".txt", true);
            CallbackDialogsHelper.ResetValues();
            _MainVMPrimary = SetUpFixture.MainVM;
            InitConnection(_MainVMPrimary);
            SetupSecuritySchemeDefault(_MainVMPrimary, _MainSecurityScheme);
            SetDefault(_MainVMPrimary);
            VM_DialogShown(_MainVMPrimary);
            InitZFingerDevice();
            SetDefaultTimeoutsS2();
            Console.WriteLine("-- Completed Setup --");
        }

        private static void SetDefaultTimeoutsS2()
        {
            AddNodeS2Operation.CMD_TIMEOUT = 10000;
            AddNodeS2Operation.CMD_USER_INPUT_TIMEOUT = 240000;
            SetLearnModeControllerS2Operation.CMD_TIMEOUT = 10000;
            SetLearnModeControllerS2Operation.CMD_USER_INPUT_TIMEOUT = 240000;
        }

        [TearDown]
        public void TearDown()
        {
            Tools.CloseDebugLogFile();
            Console.WriteLine("-- Starting teardown -- ");
            RemoveNodeInExcludeRequestState();
            DeinitZFinger();
            CloseConnection(_MainVMBridge);
            CloseConnection(_MainVMSecondary);
            CloseConnection(_MainVMPrimary);
        }

        private void RemoveNodeInExcludeRequestState()
        {
            if (_MainVMPrimary != null && _MainVMPrimary.Controller != null)
            {
                Execute("Unsolicited node exclusion", _MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null, 5000);
                CancelActive();
            }
        }

        public void InitConnection(MainViewModel mainVM)
        {
            var initResult = mainVM.OpenController();
            if (initResult != CommunicationStatuses.Done)
            {
                CloseConnection(mainVM);
                initResult = mainVM.OpenController();
            }
            Assert.AreEqual(CommunicationStatuses.Done, initResult);
        }

        public void InitSecondController(SecurityUnderTest secondaryControllerSecurity)
        {
            _MainVMSecondary = SetUpFixture.MainVMSecond;
            InitConnection(_MainVMSecondary);
            SetupSecuritySchemeDefault(_MainVMSecondary, secondaryControllerSecurity);
            SetDefault(_MainVMSecondary);
            VM_DialogShown(_MainVMSecondary);
            Delay(1000);
        }

        public void InitBridgeController(SecurityUnderTest secondaryControllerSecurity)
        {
            _MainVMBridge = SetUpFixture.MainVMBridge;
            InitConnection(_MainVMBridge);
            SetupSecuritySchemeDefault(_MainVMBridge, secondaryControllerSecurity);
            SetDefault(_MainVMBridge);
            VM_DialogShown(_MainVMBridge);
        }

        public void CloseConnection(MainViewModel mainVM)
        {
            if (mainVM != null)
            {
                if (mainVM.Controller != null)
                {
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS0Enabled = true;
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    mainVM.CommandClassesViewModel.IsDefaultSecurity = true;
                    ClearTestFrameProperties(mainVM);
                    ClearFramesS2(mainVM);
                    ClearTestExtensionsS2(mainVM);
                    ClearSecurParameters(mainVM);
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = false;
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS0Enabled = false; 
                    ApplySecuritySettings(mainVM);
                }
                mainVM.CloseController();
            }
        }

        private void InitZFingerDevice()
        {
            if (_IsUsingZFinger &&
                SetUpFixture.Config.ZFingerSource != null &&
                !string.IsNullOrEmpty(SetUpFixture.Config.ZFingerSource.PortAlias))
            {
                _ZFingerDevice = _MainVMPrimary.BasicApplicationLayer.CreateZFingerDevice();
                var communicationStatus = _ZFingerDevice.Connect(new SerialPortDataSource(SetUpFixture.Config.ZFingerSource.PortAlias));
                if (communicationStatus == CommunicationStatuses.Done)
                {
                    if (_MainVMPrimary != null)
                    {
                        RemoveNodeFromNetworkZFinger();
                        if (_MainVMPrimary.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                        {
                            Delay(500);
                            RemoveNodeFromNetworkZFinger();
                        }
                    }
                }
            }
        }

        private void DeinitZFinger()
        {
            if (!string.IsNullOrEmpty(SetUpFixture.Config.ZFingerSource.PortAlias) && _ZFingerDevice != null && _IsUsingZFinger)
            {
                _ZFingerDevice.Disconnect();
                _ZFingerDevice.Dispose();
            }
        }

        private void VM_DialogShown(MainViewModel mainVM)
        {
            mainVM.DialogShown += ((e) =>
            {
                if (e.Value is UserInputViewModel)
                {
                    UserInputViewModel input = (UserInputViewModel)e.Value;
                    if (input.Title == "Enter DSK")
                    {
                        if (input.Description == "Enter first five digits and verify all other numbers")
                        {
                            //DSKNeededCallback
                            var DskPin = CallbackDialogsHelper.DskPin;
                            if (CallbackDialogsHelper.IsWrongDsk)
                            {
                                DskPin = DskPin.ToCharArray().Reverse().ToString();
                            }
                            input.InputData = DskPin;
                            Delay(CallbackDialogsHelper.DelayDSKNeeded);
                            input.IsDialogOk = CallbackDialogsHelper.IsOkDSKNeeded;
                            CallbackDialogsHelper.WasShownDSKNeeded = true;
                        }
                        else if (input.Description == "Enter first 10 digits (4 bytes)")
                        {
                            //DSKVerificationOnReceiverCallback
                            var CsaPin = CallbackDialogsHelper.CsaPin;
                            if (CallbackDialogsHelper.IsWrongCsa)
                            {
                                CsaPin = CsaPin.ToCharArray().Reverse().ToString();
                            }
                            input.InputData = CsaPin;
                            Delay(CallbackDialogsHelper.DelayDSKVerification);
                            input.IsDialogOk = CallbackDialogsHelper.IsOkDSKVerification;
                            CallbackDialogsHelper.WasSnownDSKVerification = true;
                        }
                    }
                    else if (input.Title == "Device specific key")
                    {
                        //DskPinCallback
                        CallbackDialogsHelper.DskPin = input.AdditionalText;
                        Delay(CallbackDialogsHelper.DelayDskPin);
                        input.IsDialogOk = CallbackDialogsHelper.IsOkDskPin;
                        CallbackDialogsHelper.WasSnownDskPin = true;
                    }
                    else if (input.Title == "Client-side authentication")
                    {
                        //CsaPinCallback
                        CallbackDialogsHelper.CsaPin = input.AdditionalText;
                        Delay(CallbackDialogsHelper.DelayCsaPin);
                        input.IsDialogOk = CallbackDialogsHelper.IsOkCsaPin;
                        CallbackDialogsHelper.WasSnownCsaPin = true;
                    }
                }
                else if (e.Value is KEXSetConfirmViewModel)
                {
                    //KEXSetConfirmCallback
                    var dlg = (e.Value as KEXSetConfirmViewModel);
                    Delay(CallbackDialogsHelper.DelayKEXSetConfirm);
                    dlg.IsDialogOk = CallbackDialogsHelper.IsOkKEXSetConfirm;
                    CallbackDialogsHelper.WasSnownKEXSetConfirm = true;
                }
            });
        }

        #endregion

        #region Execution

        public void CancelActive()
        {
            if (_MainVMPrimary.CancelActiveCommand.CanExecute(null))
            {
                Execute("CancelActiveCommand", _MainVMPrimary, _MainVMPrimary.CancelActiveCommand, null);
                Delay(500);
                Assert.AreEqual(EXPECTED_COMMAND_CANCELED, _MainVMPrimary.LastCommandExecutionResult);
            }
        }

        public WaitHandle ExecuteAsync(MainViewModel mainVM, CommandBase cmd, object parameter)
        {
            mainVM.ReadySignal.Reset();
            CommandReference cmdRef = new CommandReference();
            cmdRef.Command = cmd;
            cmdRef.Execute(parameter);
            return mainVM.ReadySignal;
        }

        public void Execute(string commandName, MainViewModel mainVM, CommandBase cmd, object parameter)
        {
            Execute(commandName, mainVM, cmd, parameter, 90000);
        }

        public void Execute(string commandName, MainViewModel mainVM, CommandBase cmd, object parameter, int timeout)
        {
            Console.WriteLine("{0} >> {1}", mainVM.Controller.DataSource.SourceName, commandName);
            if (cmd.CanExecute(parameter))
            {
                var retSignal = ExecuteAsync(mainVM, cmd, parameter);
                if (mainVM.IsBusy)
                {
                    mainVM.ReadySignal.WaitOne(timeout);
                }
            }
            else
                Console.WriteLine("{0} Cannot execute {1}", mainVM.Controller.DataSource.SourceName, commandName);
        }

        #endregion

        #region Helper

        public void SetSelectedNode(int nodeId)
        {
            Assert.AreNotEqual(0, nodeId);
            Assert.Contains(nodeId, _MainVMPrimary.Nodes.Select(x => x.Item.Id).ToArray());
            if (_MainVMPrimary.SelectedNode != null)
            {
                _MainVMPrimary.SelectedNode.IsSelected = false;
            }
            _MainVMPrimary.SelectedNode = _MainVMPrimary.Nodes.Where(x => x.Item.Id == nodeId && x.Item.EndPointId == 0).FirstOrDefault();
            _MainVMPrimary.SelectedNode.IsSelected = true;
            Execute("OnSelectedItemsChangedCommand", _MainVMPrimary, _MainVMPrimary.OnSelectedItemsChangedCommand, null);
        }

        public void SetSelectedNode(MainViewModel mainVM, params byte[] nodeIds)
        {
            foreach (var item in mainVM.Nodes)
            {
                item.IsSelected = false;
                if (nodeIds.Contains(item.Item.Id) && item.Item.EndPointId == 0)
                {
                    item.IsSelected = true;
                    mainVM.SelectedNode = item;
                }
            }
            Execute("OnSelectedItemsChangedCommand", mainVM, mainVM.OnSelectedItemsChangedCommand, null);
        }

        /// <summary>value 0x00 = false, value > 0x00 = true</summary>
        public bool IsNodeBasicValueSetTo(bool isOn, byte nodeId)
        {
            bool ret = false;

            COMMAND_CLASS_BASIC.BASIC_GET cmd = new COMMAND_CLASS_BASIC.BASIC_GET();
            byte[] rptData = new COMMAND_CLASS_BASIC.BASIC_REPORT();
            var result = (_MainVMPrimary.ControllerModel as BasicControllerModel).RequestData(nodeId, cmd, ref rptData, 10000);
            if (result)
            {
                COMMAND_CLASS_BASIC.BASIC_REPORT getRpt = rptData;
                ret = (getRpt.value > 0x00) == isOn;
            }
            return ret;
        }

        public void Delay(int timeOut)
        {
            Thread.Sleep(timeOut);
        }

        public void PowerCycle()
        {
            string msg = "Power cycle connected device and press OK." + Environment.NewLine + " WOW! it used!";
            MessageBox.Show(msg);
            Delay(1000);
        }

        public void Assert_Slave_IsNonS2Secured(MainViewModel mainVM, byte nodeId)
        {
            if (mainVM.ControllerModel is ZipControllerModel)
            {
            }
            else
            {
                Assert.IsFalse((_MainVMPrimary.Controller as Controller).Network.IsNodeSecure(nodeId));
            }
        }

        public void Assert_Node_IsSecured(MainViewModel mainVM, byte nodeId)
        {
            if (mainVM.ControllerModel is ZipControllerModel)
            {
            }
            else
            {
                var network = (mainVM.Controller as Controller).Network;
                var sisNodeId = (mainVM.Controller as Controller).GetSucNodeId().SucNodeId;
                if (!network.IsNodeSecure(sisNodeId))
                {
                    Assert.IsFalse(network.IsNodeSecure(sisNodeId));
                }
                else
                {
                    if (network.IsNodeSecureS0(sisNodeId))
                    {
                        Assert.IsTrue(network.IsNodeSecureS0(nodeId));
                    }
                    if (network.IsNodeSecureS2(sisNodeId))
                    {
                        Assert.IsTrue(network.IsNodeSecureS2(nodeId));
                    }
                }
            }
        }

        public void AssertSecondarySecurity()
        {
            AssertSecondarySecurity(_MainVMSecondary);
        }

        public void AssertSecondarySecurity(MainViewModel secondaryMainVM)
        {
            if (_MainVMPrimary.ControllerModel is ZipControllerModel)
            {
            }
            else
            {
                var networkPrimary = (_MainVMPrimary.Controller as Controller).Network;
                var networkSecondary = (secondaryMainVM.Controller as Controller).Network;
                var nodeId = secondaryMainVM.Controller.Id;

                if (!networkPrimary.IsNodeSecure())
                {
                    Assert.IsFalse(networkPrimary.IsNodeSecure(nodeId));
                }
                else
                {
                    if (!networkSecondary.IsNodeSecure())
                    {
                        Assert.IsFalse(networkPrimary.IsNodeSecure(nodeId));
                    }
                    else
                    {
                        if (networkSecondary.IsNodeSecureS0())
                        {
                            Assert.IsTrue(networkPrimary.IsNodeSecureS0(nodeId));
                        }
                        if (networkSecondary.IsNodeSecureS2())
                        {
                            Assert.IsTrue(networkPrimary.IsNodeSecureS2(nodeId));
                        }
                    }
                }
            }
        }

        public void AssertNetworkKeys()
        {
            AssertNetworkKeys(_MainVMSecondary);
        }

        public void AssertNetworkKeys(MainViewModel secondaryMainVM)
        {
            if (_MainVMPrimary.ControllerModel is ZipControllerModel)
            {
            }
            else
            {
                var network = (_MainVMPrimary.Controller as Controller).Network;
                var nodeId = secondaryMainVM.Controller.Id;
                var secondarySecuritySettings = secondaryMainVM.SecuritySettingsViewModel.SecuritySettings;

                if (!network.IsNodeSecure())
                {
                    Assert.AreNotEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS0(), secondarySecuritySettings.GetNetworkKeysS0());
                    Assert.AreNotEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS2C0(), secondarySecuritySettings.GetNetworkKeysS2C0());
                }

                if (network.IsNodeSecureS0())
                {
                    if (network.IsNodeSecureS0(nodeId))
                    {
                        Assert.AreEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS0(), secondarySecuritySettings.GetNetworkKeysS0());
                    }
                    else
                    {
                        Assert.AreNotEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS0(), secondarySecuritySettings.GetNetworkKeysS0());
                    }
                }

                if (network.IsNodeSecureS2())
                {
                    if (network.IsNodeSecureS2(nodeId))
                    {
                        Assert.AreEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS2C0(), secondarySecuritySettings.GetNetworkKeysS2C0());
                    }
                    else
                    {
                        Assert.AreNotEqual(_MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS2C0(), secondarySecuritySettings.GetNetworkKeysS2C0());
                    }
                }
            }
        }

        #endregion

        #region Config Files Assertations

        private void ControllerConfiguration_OnControllerOpen_Loads()
        {
            Assert.IsNotNull(_MainVMPrimary.ControllerConfiguration);
        }

        private void ConfigurationItemFile_OnControllerOpen_Updates(MainViewModel mainVM, string oldItemFile)
        {
            Assert.IsNotNull(mainVM.ConfigurationItem);
            Assert.AreEqual(mainVM.Controller.Id, mainVM.ConfigurationItem.Id);
            Assert.AreEqual(mainVM.Controller.HomeId, mainVM.ConfigurationItem.HomeId);
            if (mainVM.Controller.Id == 1)
            {
                Assert.IsFalse(File.Exists(oldItemFile));
            }
            Assert.IsTrue(File.Exists(mainVM.ConfigurationItem.ItemFileName));
        }

        private void ConfigurationItem_AfterSecurityReset_NetworkKeysDiffs(MainViewModel mainVM, byte[] expected)
        {
            if (mainVM.ControllerModel is BasicControllerModel && _MainSecurityScheme != SecurityUnderTest.None)
            {
                if (mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_UNAUTHENTICATED)
                {
                    Assert.AreNotEqual(expected, mainVM.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS2C0());
                }
                else if (mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS0)
                {
                    Assert.AreNotEqual(expected, mainVM.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS0());
                }
            }
        }

        public void AssertConfigurationItem_NodesListUpdated_NodesPresents(MainViewModel mainVM)
        {
            Assert.IsNotNull(mainVM.ConfigurationItem.Items);
            Assert.AreEqual(mainVM.Nodes.Count, mainVM.ConfigurationItem.Items.Count);
            foreach (var item in mainVM.Nodes)
            {
                var configItem = mainVM.ConfigurationItem.Items.FirstOrDefault(x => x.Id == item.Item.Id && x.EndPointId == item.Item.EndPointId);
                Assert.IsFalse(string.IsNullOrEmpty(configItem.NodeInfo));
                var deviceInfo = configItem.ToDeviceInfo();
                Assert.AreEqual(item.Item.Capability, deviceInfo.Capability);
                Assert.AreEqual(item.Item.Basic, deviceInfo.Basic);
                Assert.AreEqual(item.Item.Generic, deviceInfo.Generic);
            }
        }

        #endregion

        #region Network Management

        public void SetDefault(MainViewModel mainVM)
        {
            //TC 63
            byte expectedId = 0x01;
            string expectedConfigurationItemFile = mainVM.ConfigurationItem.ItemFileName;
            byte[] expectedNetworkKey = (mainVM.ControllerModel is ZipControllerModel) ?
                new byte[16] :
                mainVM.SecuritySettingsViewModel.SecuritySettings.GetNetworkKeysS0();
            Execute("SetDefaultCommand", mainVM, mainVM.NetworkManagement.SetDefaultCommand, null);
            var actualId = mainVM.Controller.Id;

            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            Assert.AreEqual(expectedId, actualId);
            if (mainVM.ControllerModel is BasicControllerModel)
            {
                Assert.AreEqual(ControllerRoles.RealPrimary, mainVM.Controller.NetworkRole);
            }
            else
            {
                Assert.AreEqual(ControllerRoles.SIS, mainVM.Controller.NetworkRole);
            }
            Assert.AreEqual(EXPECTED_NODESCOUNT_SETDAFAULT, mainVM.Nodes.Count);
            ControllerConfiguration_OnControllerOpen_Loads();
            ConfigurationItemFile_OnControllerOpen_Updates(mainVM, expectedConfigurationItemFile);
            ConfigurationItem_AfterSecurityReset_NetworkKeysDiffs(mainVM, expectedNetworkKey);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(mainVM);
            Assert.IsNotNull(mainVM.ZWaveDefinition);
            Assert.AreNotEqual(0, mainVM.ZWaveDefinition.CommandClasses.Count);
        }

        public void StartLearnMode(MainViewModel mainVM)
        {
            Execute("StartLearnModeCommand", mainVM, mainVM.NetworkManagement.StartLearnModeCommand, null);
        }

        public void StartSlaveLearnMode(MainViewModel mainVM)
        {
            string oldConfigurationItemFile = mainVM.ConfigurationItem.ItemFileName;
            Execute("StartSlaveLearnModeCommand", mainVM, mainVM.NetworkManagement.StartSlaveLearnModeCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            ConfigurationItemFile_OnControllerOpen_Updates(mainVM, oldConfigurationItemFile);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(mainVM);
        }

        public void ShiftController()
        {
            Console.WriteLine("{0} Shift to  {1}", _MainVMPrimary.Controller.DataSource.SourceName, _MainVMSecondary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.NetworkManagement.ShiftControllerCommand.CanExecute(null))
            {
                ControllerRoles expRole = ControllerRoles.Secondary;
                ControllerRoles expRoleSecond = ControllerRoles.RealPrimary | ControllerRoles.OtherNetwork;
                var shiftSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ShiftControllerCommand, null);
                Delay(500);
                StartLearnMode(_MainVMSecondary);
                shiftSignal.WaitOne();
                if (_MainVMSecondary.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    shiftSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ShiftControllerCommand, null);
                    Delay(500);
                    StartLearnMode(_MainVMSecondary);
                    shiftSignal.WaitOne();
                }
                Delay(5000);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMSecondary.LastCommandExecutionResult);
                Assert.AreEqual(expRole, _MainVMPrimary.Controller.NetworkRole);
                Assert.AreEqual(expRoleSecond, _MainVMSecondary.Controller.NetworkRole);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMSecondary);
            }
            else
            {
                Console.WriteLine("Cannot execute ShiftController");
            }
        }

        public void AddNode(MainViewModel inclusionMainVM)
        {
            Console.WriteLine("{0} >> Start Include New Node", _MainVMPrimary.Controller.DataSource.SourceName);
            if (inclusionMainVM.NetworkManagement.AddNodeCommand.CanExecute(null))
            {
                if (_IsUsingZFinger)
                {
                    AddZFingerNode(inclusionMainVM);
                    if (inclusionMainVM.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                    {
                        RemoveNodeFromNetworkZFinger();
                        Delay(1000);
                        AddZFingerNode(inclusionMainVM);
                    }
                }
                else
                {
                    AddNodeManual(inclusionMainVM);
                }
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, inclusionMainVM.LastCommandExecutionResult);
            }
            else
            {
                Console.WriteLine("Cannot execute AddNode slave");
            }
        }

        private void AddNodeManual(MainViewModel mainVM)
        {
            Console.WriteLine("Manual Node Inclusion");
            MessageBox.Show(
                string.Format("Click OK and then press the pushbutton on the node to add {0} node to {1} controller"
                , _MainVMPrimary.Nodes.Count() + 1, _MainVMPrimary.Controller.NetworkRole.ToString()));
            Execute("AddNodeCommand", mainVM, mainVM.NetworkManagement.AddNodeCommand, null);
        }

        public void AddNode(MainViewModel InclusionMainVM_A, MainViewModel JoiningMainVM_B)
        {
            Console.WriteLine("{0} >> Start Include {1}", InclusionMainVM_A.Controller.DataSource.SourceName, JoiningMainVM_B.Controller.DataSource.SourceName);
            if (InclusionMainVM_A != null &&
                JoiningMainVM_B != null &&
                InclusionMainVM_A.NetworkManagement.AddNodeCommand.CanExecute(null))
            {
                var addSignal = ExecuteAsync(InclusionMainVM_A, InclusionMainVM_A.NetworkManagement.AddNodeCommand, null);
                Delay(1000);
                string oldConfigurationItemFile = JoiningMainVM_B.ConfigurationItem.ItemFileName;
                StartLearnMode(JoiningMainVM_B);
                addSignal.WaitOne();
                if (InclusionMainVM_A.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT ||
                    JoiningMainVM_B.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    Console.WriteLine("Retry");
                    if (JoiningMainVM_B.Controller.Id == 1)
                    {
                        SetDefault(JoiningMainVM_B);
                    }
                    addSignal = ExecuteAsync(InclusionMainVM_A, InclusionMainVM_A.NetworkManagement.AddNodeCommand, null);
                    Delay(500);
                    StartLearnMode(JoiningMainVM_B);
                    addSignal.WaitOne();
                }
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, InclusionMainVM_A.LastCommandExecutionResult);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, JoiningMainVM_B.LastCommandExecutionResult);
                Assert.AreEqual(InclusionMainVM_A.Controller.HomeId, JoiningMainVM_B.Controller.HomeId);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(InclusionMainVM_A);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(JoiningMainVM_B);
                //ConfigurationItemFile_OnControllerOpen_Updates(JoiningMainVM_B, oldConfigurationItemFile);

            }
            else
            {
                Console.WriteLine("Cannot execute AddNode Ctrl, is null A {0}, is null B {1}", InclusionMainVM_A != null, JoiningMainVM_B != null);
            }

        }

        public void AddNodeVirtual(MainViewModel A_MainViewModel_Inclusion, MainViewModel B_MainViewModel_JoiningBridge)
        {
            if (A_MainViewModel_Inclusion != null &&
                B_MainViewModel_JoiningBridge != null &&
                (B_MainViewModel_JoiningBridge.Controller is BridgeController) &&
                A_MainViewModel_Inclusion.NetworkManagement.AddNodeCommand.CanExecute(null))
            {
                var addSignal = ExecuteAsync(A_MainViewModel_Inclusion, A_MainViewModel_Inclusion.NetworkManagement.AddNodeCommand, null);
                Delay(500);
                StartSlaveLearnMode(B_MainViewModel_JoiningBridge);
                addSignal.WaitOne();
                if (A_MainViewModel_Inclusion.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT ||
                   B_MainViewModel_JoiningBridge.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    addSignal = ExecuteAsync(A_MainViewModel_Inclusion, A_MainViewModel_Inclusion.NetworkManagement.AddNodeCommand, null);
                    Delay(500);
                    StartSlaveLearnMode(B_MainViewModel_JoiningBridge);
                    addSignal.WaitOne();
                }
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, A_MainViewModel_Inclusion.LastCommandExecutionResult);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, B_MainViewModel_JoiningBridge.LastCommandExecutionResult);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(A_MainViewModel_Inclusion);
            }
            else
            {
                Console.WriteLine("Cannot execute AddNode Slave, is null A {0}, is null B {1}, is Bridge {2}",
                    A_MainViewModel_Inclusion != null, B_MainViewModel_JoiningBridge != null, (B_MainViewModel_JoiningBridge.Controller is BridgeController));
            }
        }

        public void ReplicateWithNode(MainViewModel inclusionMainVM)
        {
            Console.WriteLine("{0} >> Start Replicate Slave Node", _MainVMPrimary.Controller.DataSource.SourceName);
            if (inclusionMainVM.NetworkManagement.AddNodeCommand.CanExecute(null))
            {
                var expectedNC = inclusionMainVM.Nodes.Count();
                if (_IsUsingZFinger)
                {
                    AddZFingerNode(inclusionMainVM);
                    if (inclusionMainVM.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                    {
                        Delay(1000);
                        AddZFingerNode(inclusionMainVM);
                    }
                }
                else
                {
                    AddNodeManual(inclusionMainVM);
                }
                var actualNC = inclusionMainVM.Nodes.Count();
                //Assert
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, inclusionMainVM.LastCommandExecutionResult);
                Assert.AreEqual(expectedNC, actualNC);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(inclusionMainVM);
            }
            else
            {
                Console.WriteLine("Cannot execute ReplicateWithNode");
            }
        }

        public void SetAsSIS()
        {
            byte capabilities = 0x01;
            Assert.IsTrue(_MainVMPrimary.NetworkManagement.SetSucCommand.CanExecute(capabilities));
            Execute("SetSucCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SetSucCommand, capabilities);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void RemoveNodeFromNetwork(MainViewModel mainVM)
        {
            Console.WriteLine("{0} >> Start Removing Slave Node", mainVM.Controller.DataSource.SourceName);
            if (mainVM.NetworkManagement.RemoveNodeCommand.CanExecute(null))
            {
                if (_IsUsingZFinger)
                {
                    RemoveNodeFromNetworkZFinger();
                }
                else
                {
                    RemoveNodeFromNetworkManual();
                }
                Delay(2000);
                if (mainVM.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    if (_IsUsingZFinger)
                    {
                        RemoveNodeFromNetworkZFinger();
                    }
                    else
                    {
                        RemoveNodeFromNetworkManual();
                    }
                }

                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(mainVM);
            }
            else
                Console.WriteLine("Cannot execute RemoveNodeFromNetwork");
        }

        public void RemoveNodeFromNetworkManual()
        {
            MessageBox.Show("Click OK and then press the pushbutton on the node to remove it...");
            Execute("RemoveNodeCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null);
        }

        public void RemoveSecondController()
        {
            Console.WriteLine("{0} >> Start Removing {1}", _MainVMPrimary.Controller.DataSource.SourceName, _MainVMSecondary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.NetworkManagement.AddNodeCommand.CanExecute(null))
            {
                byte expectedId = _MainVMSecondary.Controller.Id;
                string oldConfigurationItemFile = _MainVMSecondary.ConfigurationItem.ItemFileName;

                var addSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null);
                Delay(500);
                StartLearnMode(_MainVMSecondary);
                addSignal.WaitOne();
                if (_MainVMSecondary.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    Console.WriteLine("Retry");
                    addSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null);
                    Delay(500);
                    StartLearnMode(_MainVMSecondary);
                    addSignal.WaitOne();
                }
                var actualId = _MainVMSecondary.Controller.Id;
                var actualNetworkRole = _MainVMSecondary.Controller.NetworkRole;
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMSecondary.LastCommandExecutionResult);
                Assert.AreNotEqual(expectedId, actualId);
                Assert.IsTrue(_MainVMPrimary.Controller.NetworkRole.HasFlag(EXPECTED_ROLE_MAIN));
                Assert.AreEqual(EXPECTED_ROLE_MAIN, actualNetworkRole);
                Assert.AreNotEqual(_MainVMPrimary.ConfigurationItem.SecuritySettings.NetworkKeys,
                                     _MainVMSecondary.ConfigurationItem.SecuritySettings.NetworkKeys);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMSecondary);
                ConfigurationItemFile_OnControllerOpen_Updates(_MainVMSecondary, oldConfigurationItemFile);

            }
            else
                Console.WriteLine("Cannot execute RemoveSecondController");
        }

        /// <summary>
        /// Start Network Wide inclusion,
        /// Wait for it ends,
        /// Check that nodes count has been increased.
        /// 
        /// IMPORTANT: AddNodeToNetworkNwiCommand.Execute() calls with parameter. Always remember that 
        /// this parameter is used ONLY for unit-testing. Make it null for UI application. 
        /// This parameter is a count of nodes to add. Network Wide Inclusion will 
        /// work untill you add to network as much nodes as you defined in this parameter
        /// </summary>
        public void NetworkWideInclusion()
        {
            int expected = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            MessageBox.Show("Click OK and then press the pushbutton on two nodes to add it after transfer presentation frame in Zniffer...");
            //The parameter passed to Execute method used only for testing! 
            Execute("AddNodeToNetworkNwiCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.AddNodeToNetworkNwiCommand, 2);
            var actual = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            Assert.AreEqual(expected + 2, actual);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
        }

        public void NetworkWideExclusion()
        {
            MessageBox.Show("Click OK and then press the pushbutton on two nodes to remove it after transfer presentation frame in Zniffer...");
            var NWESignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeFromNetworkWideCommand, null);
            while (_MainVMPrimary.Nodes.Count != EXPECTED_NODESCOUNT_SETDAFAULT)
            {
                Delay(2000);
            }
            CancelActive();
            NWESignal.WaitOne();
            //Assert
            Assert.AreEqual(EXPECTED_COMMAND_CANCELED, _MainVMPrimary.LastCommandExecutionResult);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
        }

        public void SendNOP()
        {
            Execute("SendNopCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SendNopCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IsFailedNode()
        {
            Execute("IsFailedNodeCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.IsFailedNodeCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void ReplaceFailedNodeWithController()
        {
            Console.WriteLine("{0} >> Start Replace Failed {1}", _MainVMPrimary.Controller.DataSource.SourceName, _MainVMSecondary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.NetworkManagement.ReplaceFailedCommand.CanExecute(null))
            {
                var ret = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ReplaceFailedCommand, null);
                Delay(500);
                string oldConfigurationItemFile = _MainVMSecondary.ConfigurationItem.ItemFileName;
                StartLearnMode(_MainVMSecondary);
                ret.WaitOne();
                if (_MainVMSecondary.LastCommandExecutionResult != EXPEXTED_COMMAND_EXECUTION_RESULT)
                {
                    Console.WriteLine("Retry");
                    ret = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ReplaceFailedCommand, null);
                    Delay(500);
                    StartLearnMode(_MainVMSecondary);
                }
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMSecondary.LastCommandExecutionResult);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMSecondary);
                ConfigurationItemFile_OnControllerOpen_Updates(_MainVMSecondary, oldConfigurationItemFile);
            }
            else
                Console.WriteLine("Cannot execute ReplaceFailedNode");
        }

        public void ReplaceFailedNodeWithSlave()
        {
            Console.WriteLine("{0} >> Start Replace Failed Slave", _MainVMPrimary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.NetworkManagement.ReplaceFailedCommand.CanExecute(null))
            {
                var ret = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ReplaceFailedCommand, null);
                Delay(3000);
                if (_IsUsingZFinger)
                {
                    _ZFingerDevice.PB1QuadPress();
                    ret.WaitOne();
                    Delay(11000);
                }
                else
                {
                    MessageBox.Show("Click OK and then press the pushbutton on the node to replicate it...");
                    ret.WaitOne();
                }
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            }
            else
                Console.WriteLine("Cannot execute ReplaceFailedNodeWithSlave");
        }

        public void RemoveFailedNode()
        {
            Execute("RemoveFailedCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveFailedCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void NeighborsUpdate()
        {
            Execute("RequestNodeNeighborUpdateCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.RequestNodeNeighborUpdateCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void RequestNodeInfo()
        {
            RequestNodeInfo(_MainVMPrimary);
        }

        public void RequestNodeInfo(MainViewModel mainVM)
        {
            Execute("RequestNodeInfoCommand", mainVM, mainVM.NetworkManagement.RequestNodeInfoCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(mainVM);
        }

        public void BasicSetOn(MainViewModel mainVM)
        {
            Execute("BasicSetOnCommand", mainVM, mainVM.NetworkManagement.BasicSetOnCommand, null);
            Delay(1000);
            //Assert
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
        }

        public void BasicSetOff(MainViewModel mainVM)
        {
            Execute("BasicSetOffCommand", mainVM, mainVM.NetworkManagement.BasicSetOffCommand, null);
            Delay(1000);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
        }

        public void SwitchAllOn()
        {
            Execute("SwitchAllOnCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SwitchAllOnCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void SwitchAllOff()
        {
            Execute("SwitchAllOffCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SwitchAllOffCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void BasicTest()
        {
            Execute("ToggleBasicGetCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.ToggleBasicGetCommand, null);
        }

        public void SetWakeupInterval()
        {
            Execute("SetWakeupIntervalCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SetWakeupIntervalCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void ControllerSendInformation()
        {
            Execute("SendNodeInformationCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.SendNodeInformationCommand, null);
            Assert.IsTrue(_MainVMPrimary.LastCommandExecutionResult == CommandExecutionResult.OK);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
        }

        public void ControllerUpdate()
        {
            Execute("RequestNetworkUpdateCommand", _MainVMPrimary, _MainVMPrimary.NetworkManagement.RequestNetworkUpdateCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void ResetSPAN(MainViewModel mainViewModel, byte nodeId)
        {
            mainViewModel.SelectedNode = mainViewModel.Nodes.Where(x => x.Item.Id == nodeId && x.Item.EndPointId == 0).FirstOrDefault();
            Execute("ResetSPANCommand", mainViewModel, mainViewModel.NetworkManagement.ResetSPANCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
        }

        public void NextSPAN(MainViewModel mainViewModel, byte nodeId)
        {
            mainViewModel.SelectedNode = mainViewModel.Nodes.Where(x => x.Item.Id == nodeId && x.Item.EndPointId == 0).FirstOrDefault();
            Execute("NextSPANCommand", mainViewModel, mainViewModel.NetworkManagement.NextSPANCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
        }

        public void VersionGet(MainViewModel mainViewModel)
        {
            Execute("VersionGetCommand", mainViewModel, mainViewModel.NetworkManagement.VersionGetCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
            Delay(500);
        }

        #endregion

        #region Command Classes

        public void SelectCommandOK()
        {
            Execute("Select CommandOk", _MainVMPrimary, _MainVMPrimary.CommandClassesViewModel.SelectCommandViewModel.CommandOk, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void SendCommand()
        {
            SendCommand(_MainVMPrimary);
        }

        public void SendCommand(MainViewModel mainVM)
        {
            Execute("SendCommand", mainVM, mainVM.CommandClassesViewModel.SendCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
        }

        public void ReloadXML()
        {
            Execute("ReloadXmlCommand", _MainVMPrimary, _MainVMPrimary.CommandClassesViewModel.ReloadXmlCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        #endregion

        #region Encrypt/Decrypt
        public void Encrypt()
        {
            Execute("EncryptS0Command", _MainVMPrimary, _MainVMPrimary.EncryptDecryptViewModel.EncryptS0Command, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void EncryptS2()
        {
            Execute("EncryptS2Command", _MainVMPrimary, _MainVMPrimary.EncryptDecryptViewModel.EncryptS2Command, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void Decrypt()
        {
            Execute("DecryptS0Command", _MainVMPrimary, _MainVMPrimary.EncryptDecryptViewModel.DecryptS0Command, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void DecryptS2()
        {
            Execute("DecryptS2Command", _MainVMPrimary, _MainVMPrimary.EncryptDecryptViewModel.DecryptS2Command, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        #endregion

        #region ERTT

        public void ERTTStart()
        {
            Console.WriteLine("{0} >> Start ERTT", _MainVMPrimary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.ERTTViewModel.StartStopCommand.CanExecute(null))
            {
                _MainVMPrimary.ERTTViewModel.IsTestReady = true;
                var startSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.ERTTViewModel.StartStopCommand, null);
                startSignal.WaitOne();
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute StartERTT");
        }

        public WaitHandle ERTTStartRet()
        {
            Console.WriteLine("{0} >> Start ERTT RET", _MainVMPrimary.Controller.DataSource.SourceName);
            WaitHandle startSignal = null;
            if (_MainVMPrimary.ERTTViewModel.StartStopCommand.CanExecute(null))
            {
                _MainVMPrimary.ERTTViewModel.IsTestReady = true;
                startSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.ERTTViewModel.StartStopCommand, null);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute ERTTStartRet");
            return startSignal;
        }

        public void ERTTStop()
        {
            Console.WriteLine("{0} >> Stop ERTT", _MainVMPrimary.Controller.DataSource.SourceName);
            if (_MainVMPrimary.ERTTViewModel.StartStopCommand.CanExecute(null))
            {
                _MainVMPrimary.ERTTViewModel.IsTestReady = false;
                var stopSignal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.ERTTViewModel.StartStopCommand, null);
                stopSignal.WaitOne();
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute StopERTT");
        }

        #endregion

        #region Polling

        public void PollingStart()
        {
            Execute("StartPollingCommand", _MainVMPrimary, _MainVMPrimary.PollingViewModel.StartPollingCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult PollingStart");
        }

        public void PollingStop()
        {
            Execute("StopPollingCommand", _MainVMPrimary, _MainVMPrimary.PollingViewModel.StopPollingCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult PollingStop");
        }

        #endregion

        #region SetupRoute
        public void SetupRouteAssign()
        {
            SetupRouteAssign(false);
        }

        public void SetupRouteAssign(bool isExpectFailure)
        {
            Execute("AssignReturnRouteCommand", _MainVMPrimary, _MainVMPrimary.SetupRouteViewModel.AssignReturnRouteCommand, null);
            if (isExpectFailure)
            {
                Assert.AreNotEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult SetupRouteAssign");
            }
            else
            {
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult SetupRouteAssign");
            }
        }

        public void SetupRouteDelete()
        {
            Execute("RemoveReturnRouteCommand", _MainVMPrimary, _MainVMPrimary.SetupRouteViewModel.RemoveReturnRouteCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult SetupRouteDelete");
        }

        public void PriorityRouteGet()
        {
            Execute("GetPriorityRouteCommand", _MainVMPrimary, _MainVMPrimary.SetupRouteViewModel.GetPriorityRouteCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult PriorityRouteGet");
        }

        public void PriorityRouteSet()
        {
            Execute("SetPriorityRouteCommand", _MainVMPrimary, _MainVMPrimary.SetupRouteViewModel.SetPriorityRouteCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult PriorityRouteSet");
        }

        #endregion

        #region Topology Map
        public void TopologyMapReload()
        {
            Execute("TopologyMapReloadCommand", _MainVMPrimary, _MainVMPrimary.TopologyMapViewModel.TopologyMapReloadCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult TopologyMapReload");
            Assert.IsNotNull(_MainVMPrimary.TopologyMapViewModel.Matrix);
        }
        #endregion

        #region Associations

        public void AssociationGetGroupsInfo()
        {
            int expected = 0;
            Execute("AssociationGetGroupsCommand", _MainVMPrimary, _MainVMPrimary.AssociationViewModel.AssociationGetGroupsCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            var actual = _MainVMPrimary.AssociationViewModel.SelectedAssociativeDevice.Groups.Count;
            Assert.Greater(actual, expected);
        }

        public void AssociationCreate()
        {
            Execute("AssociationCreateCommand", _MainVMPrimary, _MainVMPrimary.AssociationViewModel.AssociationCreateCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void AssociationRemove()
        {
            Execute("AssociationRemoveCommand", _MainVMPrimary, _MainVMPrimary.AssociationViewModel.AssociationRemoveCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void AssociationGetNodes()
        {
            Execute("AssociationGetCommand", _MainVMPrimary, _MainVMPrimary.AssociationViewModel.AssociationGetCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup);
        }

        /* not available
         * 
        public void AssociationGetGroupInfo()
        {
            Console.WriteLine("GetGroups");
            if (_MainVM.AssociationViewModel.AssociationGetGroupInfoCommand.CanExecute(null))
            {
                //Act
                Execute(_MainVM, _MainVM.AssociationViewModel.AssociationGetGroupInfoCommand, null);
                //Assert
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        public void AssociationGetCommandList()
        {
            Console.WriteLine("AssociationGetCommandList");
            if (_MainVM.AssociationViewModel.AssociationGetCommandListCommand.CanExecute(null))
            {
                //Act
                Execute(_MainVM, _MainVM.AssociationViewModel.AssociationGetCommandListCommand, null);
                //Assert
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }
        */

        #endregion

        #region IMA Network

        public void IMANetworkHealth()
        {
            byte EXP_MIN_NHV = 1;
            Execute("FullNetworkCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.FullNetworkCommand, null);
            int k = 0;
            while (IMAFullNetworkViewModel.RunningTests != 0)
            {
                Delay(10000);
                if (k++ > 10)
                {
                    Assert.Fail();
                }
            }
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            var imaItems = _MainVMPrimary.IMAFullNetworkViewModel.GetSelectedItems();
            if (imaItems == null)
            {
                imaItems = _MainVMPrimary.IMAFullNetworkViewModel.GetItems();
            }
            foreach (var actual in imaItems)
            {
                Assert.IsNotNull(actual.IMATestResults, "No result for item" + actual.Id);
                Assert.Greater(actual.NHV, EXP_MIN_NHV);
            }
        }

        public void IMARequestNodeInfo()
        {
            Execute("RequestNodeInfoCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.RequestNodeInfoCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IMAGetVersion()
        {
            Execute("GetVersionCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.GetVersionCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IMAPowerLevelTest()
        {
            Execute("PowerLevelTestCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.PowerLevelTestCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IMAPingNode()
        {
            Execute("PingNodeCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.PingNodeCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IMARediscovery()
        {
            Execute("RediscoveryCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.RediscoveryCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void IMAReloadRoutingInfo()
        {
            Execute("ReloadCommand", _MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.ReloadCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        #endregion

        #region OTA

        public void OTAGet()
        {
            Execute("FirmwareUpdateOTAGetCommand", _MainVMPrimary, _MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateOTAGetCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult, "LastCommandExecutionResult OTAGet");
        }

        public void OTABrowseFile()
        {
            _MainVMPrimary.FirmwareUpdateViewModel.FirmwareFileName = SetUpFixture.Config.OTAFileName;
            _MainVMPrimary.FirmwareUpdateViewModel.FirmwareData = HexFileHelper.
                GetBytes(HexFileHelper.ReadIntelHexFile(_MainVMPrimary.FirmwareUpdateViewModel.FirmwareFileName, 0xFF), 0xFF);
            _MainVMPrimary.FirmwareUpdateViewModel.FirmwareChecksum = Utils.Tools.CalculateCrc16Array(_MainVMPrimary.FirmwareUpdateViewModel.FirmwareData);
        }

        public void OTAUpdate()
        {
            Execute("FirmwareUpdateOTAUpdateCommand", _MainVMPrimary, _MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateOTAUpdateCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void OTAActivate()
        {
            Execute("FirmwareUpdateOTAActivateCommand", _MainVMPrimary, _MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateOTAActivateCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }
        #endregion

        #region NVM Backup/Restore

        public void Backup()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), TEMP_FILENAME);
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            _MainVMPrimary.NVMBackupRestoreViewModel.BackupFileName = tempFilePath;
            Execute("NVMBackupCommand", _MainVMPrimary, _MainVMPrimary.NVMBackupRestoreViewModel.NVMBackupCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void Restore()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), TEMP_FILENAME);
            Assert.IsTrue(File.Exists(tempFilePath), string.Format("The file {0} does not exist by path:{1}", TEMP_FILENAME, tempFilePath));
            _MainVMPrimary.NVMBackupRestoreViewModel.RestoreFileName = tempFilePath;
            Execute("NVMRestoreCommand", _MainVMPrimary, _MainVMPrimary.NVMBackupRestoreViewModel.NVMRestoreCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        #endregion

        #region Configuration Parameters

        public void GetList()
        {
            Execute("RetrieveConfigurationListCommand", _MainVMPrimary, _MainVMPrimary.ConfigurationViewModel.RetrieveConfigurationListCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        #endregion

        #region Settings

        public void Detect()
        {
            Execute("DetectCommand", _MainVMPrimary, _MainVMPrimary.SettingsViewModel.DetectCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void Refresh()
        {
            Execute("RefreshCommand", _MainVMPrimary, _MainVMPrimary.SettingsViewModel.RefreshCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void ClearAllZip()
        {
            Execute("ClearAllCommand", _MainVMPrimary, _MainVMPrimary.SettingsViewModel.ClearAllCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void DiscoverZIP()
        {
            Execute("DiscoverCommand", _MainVMPrimary, _MainVMPrimary.SettingsViewModel.DiscoverCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void AddCustomIPAdress()
        {
            Execute("AddSocketSourceCommand", _MainVMPrimary, _MainVMPrimary.SettingsViewModel.AddSocketSourceCommand, null);
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        public void ApplySettings(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SettingsViewModel))
            {
                CommandRunner.Execute(_MainVMPrimary.SettingsViewModel.CommandOk, _MainVMPrimary.SettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
            {
                Console.WriteLine("Can't apply setting.");
            }
        }

        #endregion

        #region CancelActions

        public void AddNodeCancel()
        {
            Console.WriteLine("AddNodeToNetworkCancel");
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.AddNodeCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
        }

        public void RemoveNodeFromNetworkCancel()
        {
            Console.WriteLine("RemoveNodeFromNetworkCancel");
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
        }

        public void NWICancel()
        {
            Console.WriteLine("NWICancel");
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.AddNodeToNetworkNwiCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
        }

        public void NWECancel()
        {
            Console.WriteLine("NWECancel");
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeFromNetworkWideCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
        }

        public void StartLearnModeCancel()
        {
            Console.WriteLine("StartLearnModeCancel");
            //Arrange
            int expectedCount = _MainVMPrimary.Nodes.Count;
            byte[] expctedHomeId = _MainVMPrimary.Controller.HomeId;
            //Act
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.StartLearnModeCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
            int actualCount = _MainVMPrimary.Nodes.Count;
            byte[] actualHomeId = _MainVMPrimary.Controller.HomeId;
            //Assert
            Assert.AreEqual(expectedCount, actualCount);
            Assert.AreEqual(expctedHomeId, actualHomeId);
        }

        public void ShiftCancel()
        {
            Console.WriteLine("ShiftCancel");
            //Arrange
            int expected = _MainVMPrimary.Nodes.Count;
            var expectedNetworkRole = _MainVMPrimary.Controller.NetworkRole;
            //Act
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.ShiftControllerCommand, null);
            Delay(2000);
            CancelActive();
            signal.WaitOne();
            var actual = _MainVMPrimary.Nodes.Count;
            var actualNetworkRole = _MainVMPrimary.Controller.NetworkRole;
            //Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedNetworkRole, actualNetworkRole);
        }

        public void IMANetworkHealthCancel()
        {
            Console.WriteLine("IMANetworkHealthCancel");
            if (_MainVMPrimary.IMAFullNetworkViewModel.FullNetworkCommand.CanExecute(null))
            {
                var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.FullNetworkCommand, null);
                Delay(1000);
                CancelActive();
                signal.WaitOne(10000);
                Assert.AreEqual(EXPECTED_COMMAND_CANCELED, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        public void IMAPowerLevelCancel()
        {
            Console.WriteLine("IMAPowerLevelCancel");
            if (_MainVMPrimary.IMAFullNetworkViewModel.PowerLevelTestCommand.CanExecute(null))
            {
                var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.IMAFullNetworkViewModel.PowerLevelTestCommand, null);
                Delay(300);
                CancelActive();
                signal.WaitOne(10000);
                Assert.AreEqual(EXPECTED_COMMAND_CANCELED, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        public void OTAUpdateCancel()
        {
            Console.WriteLine("OTAUpdateCancel");
            if (_MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateOTAUpdateCommand.CanExecute(null))
            {
                var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateOTAUpdateCommand, null);
                Delay(5000);
                CancelActive();
                signal.WaitOne(20000);
                Assert.AreEqual(EXPECTED_COMMAND_CANCELED, _MainVMPrimary.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }
        #endregion

        #region ZFingerDevice

        public void AddZFingerNode(MainViewModel mainVM)
        {
            Console.WriteLine("ZFINGER Node Inclusion");
            var signal = ExecuteAsync(mainVM, mainVM.NetworkManagement.AddNodeCommand, null);
            Delay(3000);
            _ZFingerDevice.PB1QuadPress();
            signal.WaitOne();
            Delay(10000);
        }

        public void RemoveNodeFromNetworkZFinger()
        {
            var signal = ExecuteAsync(_MainVMPrimary, _MainVMPrimary.NetworkManagement.RemoveNodeCommand, null);
            Delay(3000);
            _ZFingerDevice.PB1QuadPress();
            signal.WaitOne();
        }

        public void ZFingerDeviceNodeInfo()
        {
            Console.WriteLine("ZFinger Device node info from board");
            _ZFingerDevice.PB1QuadPress();
        }

        public void ZFingerHoldPress()
        {
            Console.WriteLine("ZFinger Press and Hold push button");
            _ZFingerDevice.PB1PressAndHold(30000);
        }
        #endregion

        #region SetupSecurity
        private void SetupSecuritySchemeDefault(MainViewModel mainVM, SecurityUnderTest securityUnderTest)
        {
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS0 = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_UNAUTHENTICATED = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_AUTHENTICATED = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsClientSideAuthS2Enabled = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS0Enabled = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[7].IsSet = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[0].IsSet = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[1].IsSet = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.TestNetworkKeys[2].IsSet = false;
            mainVM.SecuritySettingsViewModel.SecuritySettings.TestIsOverrideExistingExtensionsS2 = false;
            switch (securityUnderTest)
            {
                case SecurityUnderTest.S0:
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS0 = true;
                    break;
                case SecurityUnderTest.S2:
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS0 = true;
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_UNAUTHENTICATED = true;
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_AUTHENTICATED = true;
                    mainVM.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = true;
                    break;
                case SecurityUnderTest.None:
                default:
                    break;
            }
            ApplySecuritySettings(mainVM);
        }

        public void ApplySecuritySettings(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel))
            {
                Execute("SecuritySettingsViewModel Apply", mainVM, mainVM.SecuritySettingsViewModel.CommandApply, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute ApplySecuritySettings");
        }

        public void SetS2TestFrame(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandSetFrame.CanExecute(null))
            {
                Execute("CommandSetFrame", mainVM, mainVM.SecuritySettingsViewModel.CommandSetFrame, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute SetS2TestFrame");
        }

        public void DeleteSelectedFrameS2(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandDeleteSelectedFrame.CanExecute(null))
            {
                Execute("CommandDeleteSelectedFrame", mainVM, mainVM.SecuritySettingsViewModel.CommandDeleteSelectedFrame, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute DeleteSelectedFrameS2");
        }

        public void ClearFramesS2(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandClearFrames.CanExecute(null))
            {
                Execute("CommandClearFrames", mainVM, mainVM.SecuritySettingsViewModel.CommandClearFrames, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
                Assert.AreEqual(0, mainVM.SecuritySettingsViewModel.SecuritySettings.TestFramesS2.Count());
            }
            else
                Console.WriteLine("Cannot execute ClearFramesS2");
        }

        public void ClearTestFrameProperties(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel))
            {
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameIsMulticast.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameIsTemp.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameIsTemp.Value = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.IsSet = false;
                mainVM.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 0;
            }
        }

        public void AddTestExtensionS2(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandAddTestExtensionS2.CanExecute(null))
            {
                Execute("CommandAddTestExtensionS2", mainVM, mainVM.SecuritySettingsViewModel.CommandAddTestExtensionS2, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute AddTestExtensionS2");
        }

        public void RemoveSelectedTestExtensionS2(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandRemoveSelectedTestExtensionS2.CanExecute(null))
            {

                Execute("CommandRemoveSelectedTestExtensionS2", mainVM, mainVM.SecuritySettingsViewModel.CommandRemoveSelectedTestExtensionS2, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute RemoveSelectedTestExtensionS2");
        }

        public void ClearTestExtensionsS2(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandClearTestExtensionsS2.CanExecute(null))
            {
                Execute("CommandClearTestExtensionsS2", mainVM, mainVM.SecuritySettingsViewModel.CommandClearTestExtensionsS2, mainVM.SecuritySettingsViewModel);
                Assert.IsEmpty(mainVM.SecuritySettingsViewModel.SecuritySettings.TestExtensionsS2);
            }
            else
                Console.WriteLine("Cannot execute ClearTestExtensionsS2");
        }

        public void BrowseStorageFolder(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel))
            {
                Execute("CommandClearTestExtensionsS2", mainVM, mainVM.SecuritySettingsViewModel.BrowseStorageFolderCommand, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
            }
            else
                Console.WriteLine("Cannot execute BrowseStorageFolder");
        }

        public void ClearSecurParameters(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandClearSecurParameters.CanExecute(null))
            {
                Execute("CommandClearSecurParameters", mainVM, mainVM.SecuritySettingsViewModel.CommandClearSecurParameters, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
                Assert.IsEmpty(mainVM.SecuritySettingsViewModel.SecuritySettings.TestParametersS2);
            }
            else
                Console.WriteLine("Cannot execute CommandClearSecurParameters");
        }

        public void DeleteSelectedSecurParameter(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandDeleteSelectedSecurParameter.CanExecute(null))
            {
                var exp = mainVM.SecuritySettingsViewModel.SecuritySettings.TestParametersS2.Count - 1;
                Execute("CommandDeleteSelectedSecurParameter", mainVM, mainVM.SecuritySettingsViewModel.CommandDeleteSelectedSecurParameter, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
                var act = mainVM.SecuritySettingsViewModel.SecuritySettings.TestParametersS2.Count;
                Assert.AreEqual(exp, act);
            }
            else
                Console.WriteLine("Cannot execute CommandDeleteSelectedSecurParameter");
        }

        public void SaveSecurityTestParameter(MainViewModel mainVM)
        {
            if (mainVM.ShowSecuritySettingsCommand.CanExecute(mainVM.SecuritySettingsViewModel) &&
                mainVM.SecuritySettingsViewModel.CommandSaveSecurityTestParameter.CanExecute(null))
            {
                Execute("CommandDeleteSelectedSecurParameter", mainVM, mainVM.SecuritySettingsViewModel.CommandSaveSecurityTestParameter, mainVM.SecuritySettingsViewModel);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainVM.LastCommandExecutionResult);
                Assert.IsNotEmpty(mainVM.SecuritySettingsViewModel.SecuritySettings.TestParametersS2);
            }
            else
                Console.WriteLine("Cannot execute CommandSaveSecurityTestParameter");
        }
        #endregion

        #region MPAN
        public void LoadMpanTable(MainViewModel mainViewModel)
        {
            if (mainViewModel.NetworkManagement.ShowMpanTableConfigurationCommand.CanExecute(null))
            {
                Execute("LoadMpanTableCommand", mainViewModel, mainViewModel.NetworkManagement.MpanTableConfigurationVM.LoadMpanTableCommand, null);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
                Delay(500);
            }
            else
                Console.WriteLine("Cannot execute LoadMpanTable for ID:" + mainViewModel.Controller.Id);
        }

        public void AddUpdateMpanTable(MainViewModel mainViewModel)
        {
            if (mainViewModel.NetworkManagement.ShowMpanTableConfigurationCommand.CanExecute(null))
            {
                Execute("AddOrUpdateSelectedMpanItemCommand", mainViewModel, mainViewModel.NetworkManagement.MpanTableConfigurationVM.AddOrUpdateSelectedMpanItemCommand, null);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
                Delay(500);
            }
            else
                Console.WriteLine("Cannot execute LoadMpanTable for ID:" + mainViewModel.Controller.Id);
        }

        public void NextMPAN(MainViewModel mainViewModel)
        {
            if (mainViewModel.NetworkManagement.ShowMpanTableConfigurationCommand.CanExecute(null))
            {

                Execute("NextSelectedMpanItemCommand", mainViewModel, mainViewModel.NetworkManagement.MpanTableConfigurationVM.NextSelectedMpanItemCommand, null);
                Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, mainViewModel.LastCommandExecutionResult);
                Delay(500);
            }
            else
                Console.WriteLine("Cannot execute LoadMpanTable for ID:" + mainViewModel.Controller.Id);
        }
        #endregion

        public void RemoveInclusionSupportCC(MainViewModel mainViewModel)
        {
            if (mainViewModel.ControllerModel is ZipControllerModel)
            {
            }
            else
            {
                // Non secure
                var controllerDI = mainViewModel.Nodes.First(c => c.Item.Id == mainViewModel.Controller.Id).Item;
                controllerDI.SupportedCommandClasses = controllerDI.SupportedCommandClasses.Where(id => id != COMMAND_CLASS_INCLUSION_CONTROLLER.ID).ToArray();
                mainViewModel.ConfigurationItem.UpdateNodeData(controllerDI, mainViewModel.Controller);
                mainViewModel.ConfigurationItem.Save();
                (mainViewModel.ControllerModel as BasicControllerModel).SetControllerNodeInformation();

                // Secure
                var controllerNetwork = (mainViewModel.Controller as Controller).Network;
                var secureCCs = controllerNetwork.GetCommandClassesSecure();
                byte[] secureCCsWithouthInclusionCC = null;
                if (secureCCs != null)
                {
                    secureCCsWithouthInclusionCC = secureCCs.Where(id => id != COMMAND_CLASS_INCLUSION_CONTROLLER.ID).ToArray();
                }
                if (secureCCsWithouthInclusionCC != null)
                {
                    controllerNetwork.SetCommandClassesSecure(secureCCsWithouthInclusionCC);
                }
            }
        }
    }
}
