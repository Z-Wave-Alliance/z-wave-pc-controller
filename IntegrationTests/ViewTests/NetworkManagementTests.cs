/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using NUnit.Framework;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Security;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class NetworkManagementTests : TestCaseBase
    {
        public NetworkManagementTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void AddNodeToNetwork_AfterInclusion_AddsOneNode()
        {
            int expectedNodesCount = 2;

            AddNode(_MainVMPrimary);
            int actualNodesCount = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();

            Assert.AreEqual(expectedNodesCount, actualNodesCount);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            Assert_Node_IsSecured(_MainVMPrimary, 2);
        }

        [Test]
        public void AddNodeToNetwork_WhenCanceled_NotIncludes()
        {
            int expected = _MainVMPrimary.Nodes.Count;

            AddNodeCancel();
            var actual = _MainVMPrimary.Nodes.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RemoveNodeFromNetwork_WhenCanceled_NodeListNotChanges()
        {
            AddNode(_MainVMPrimary);
            int expected = _MainVMPrimary.Nodes.Count;

            RemoveNodeFromNetworkCancel();
            var actual = _MainVMPrimary.Nodes.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NetworkWideInclusion_WhenCancelled_NodesListNotChanges()
        {
            AddNode(_MainVMPrimary);
            int expected = _MainVMPrimary.Nodes.Count;

            NWICancel();
            var actual = _MainVMPrimary.Nodes.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NetworkWideExclusion_WhenCancelled_NodesListNotChanges()
        {
            AddNode(_MainVMPrimary);
            int expected = _MainVMPrimary.Nodes.Count;

            NWECancel();
            var actual = _MainVMPrimary.Nodes.Count;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SendNOP_ToSelectedNode_NotFails()
        {
            AddNode(_MainVMPrimary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            SendNOP();
        }

        [Test]
        public void SendNOP_ToSpecifiedNodId_NotFails()
        {
            AddNode(_MainVMPrimary);
            SetSelectedNode(0x01);
            _MainVMPrimary.NetworkManagement.SendNopNodeId = 0x02;
            SendNOP();
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void IsFailedNode_AfterTurnOff2nd_ReturnsTrue(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.NetworkManagement.IsFailedNodeCommand.CanExecute(null))
            {
                InitSecondController(secondaryControllerSecurity);
                AddNode(_MainVMPrimary, _MainVMSecondary);
                SetSelectedNode(0x02);
                SetDefault(_MainVMSecondary);

                IsFailedNode();

                Assert.IsTrue(_MainVMPrimary.Nodes.Any(p => p.Item.IsFailed));
                Assert.IsTrue(_MainVMPrimary.SelectedNode.Item.IsFailed);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void ReplaceFailedNode_NoSIS_NodeReplaces(SecurityUnderTest secondaryControllerSecurity)
        {
            //Arrange.
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(0x02);
            RequestNodeInfo();
            Assert.IsTrue(_MainVMPrimary.SelectedNode.Item.IsListening);
            var expectedS0 = _MainVMPrimary.SelectedNode.Item.SecurelyS0SupportedCommandClasses;
            var expectedS2 = _MainVMPrimary.SelectedNode.Item.SecurelyS2SupportedCommandClasses;
            //Act
            if (_MainVMPrimary.NetworkManagement.IsFailedNodeCommand.CanExecute(null))
            {
                SetDefault(_MainVMSecondary);
                IsFailedNode();
                Assert.IsTrue(_MainVMPrimary.Nodes.Any(p => p.Item.IsFailed));
                Assert.IsTrue(_MainVMPrimary.SelectedNode.Item.IsFailed);
                ReplaceFailedNodeWithController();
                SetSelectedNode(0x02);
                IsFailedNode();
                RequestNodeInfo();
                var actualS0 = _MainVMPrimary.SelectedNode.Item.SecurelyS0SupportedCommandClasses;
                // Due to change in RequestNodeInfo, when node is asked only S2 CCs:
                // When node is replaced it is asked all S0 and S2 CCs.
                if (actualS0 != null && expectedS0 != null && actualS0.Length > expectedS0.Length)
                {
                    actualS0 = expectedS0;
                }
                var actualS2 = _MainVMPrimary.SelectedNode.Item.SecurelyS2SupportedCommandClasses;
                //Assert
                Assert.IsFalse(_MainVMPrimary.SelectedNode.Item.IsFailed);
                Assert.AreEqual(expectedS0, actualS0);
                Assert.AreEqual(expectedS2, actualS2);
                Assert_Node_IsSecured(_MainVMPrimary, 0x02);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void RemoveFailedNode_AfterCall_NodeRemoves(SecurityUnderTest secondaryControllerSecurity)
        {
            //Arrange.
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(_MainVMSecondary.Controller.Id);
            //Act
            if (_MainVMPrimary.NetworkManagement.IsFailedNodeCommand.CanExecute(null))
            {
                var expected = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count() - 1;
                SetDefault(_MainVMSecondary);
                IsFailedNode();
                RemoveFailedNode();
                var actual = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
                //Assert
                Assert.AreEqual(expected, actual);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SetAsSISSecond_OnSecondaryController_SISEnables(SecurityUnderTest secondaryControllerSecurity)
        {
            //Act
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            SetAsSIS();
            var actualSucResult = (_MainVMPrimary.Controller as Controller).GetSucNodeId();
            //Assert
            Assert.IsTrue(actualSucResult.IsStateCompleted);
            Assert.AreEqual(_MainVMPrimary.SelectedNode.Item.Id, actualSucResult.SucNodeId);
        }

        [Test]
        public void NeighborsUpdate_AfterCall_Success()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            //Act
            NeighborsUpdate();
        }

        [Test]
        public void RequestNodeInfo_ToSelectedNode_NodeInfoUpdates()
        {
            AddNode(_MainVMPrimary);
            SetSelectedNode(0x02);

            RequestNodeInfo();

            Assert_Node_IsSecured(_MainVMPrimary, 0x02);
        }

        /// <summary>
        /// Send Basic Report Command and check returned value (valid values can be greater than 0x00, 0xFF or 0x63)
        /// Send Basic Report Command and check returned value equal 0x00
        /// </summary>
        [Test]
        public void BasicSetOnOff_ToAddedNode_SetsValue()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            //Act
            BasicSetOn(_MainVMPrimary);
            Assert.IsTrue(IsNodeBasicValueSetTo(true, _MainVMPrimary.SelectedNode.Item.Id));
            Delay(5000);
            BasicSetOff(_MainVMPrimary);
            Assert.IsTrue(IsNodeBasicValueSetTo(false, _MainVMPrimary.SelectedNode.Item.Id));
        }

        [Test]
        public void SwitchAllOnOff_ToLastAddedNode_Success()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            //Act
            SwitchAllOn();
            Delay(10000);
            SwitchAllOff();
        }

        [Test]
        public void BasicTest_AfterStartOnOneNode_SuccessfullyStopes()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            _MainVMPrimary.SelectedNode = _MainVMPrimary.Nodes.Last();
            //Act1
            _MainVMPrimary.NetworkManagement.IsToggleBasicGetStarted = true;
            BasicTest();
            //Assert1
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
            //Act2
            Delay(5000);
            _MainVMPrimary.NetworkManagement.IsToggleBasicGetStarted = false;
            BasicTest();
            //Assert2
            Assert.AreEqual(EXPEXTED_COMMAND_EXECUTION_RESULT, _MainVMPrimary.LastCommandExecutionResult);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void Shift_ToSecondaryController_NetworkRoleChanges(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.NetworkManagement.ShiftControllerCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);
                //Act
                AddNode(_MainVMPrimary, _MainVMSecondary);

                var actualSecondaryNetworkRole = _MainVMSecondary.Controller.NetworkRole;
                var actualMainNetworkRole = _MainVMPrimary.Controller.NetworkRole;

                ShiftController();

                var actualSecondaryNetworkRoleAfterShift = _MainVMSecondary.Controller.NetworkRole;
                var actualMainNetworkRoleAfterSghift = _MainVMPrimary.Controller.NetworkRole;


                //Assert
                Assert.AreEqual(EXPECTED_ROLE_SECOND, actualSecondaryNetworkRole);
                Assert.AreEqual(EXPECTED_ROLE_MAIN, actualMainNetworkRole);

            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Test]
        public void Shift_ToSecondaryControllerCancelled_NetworkRoleNotChanges()
        {
            //Arrange
            _MainVMSecondary = SetUpFixture.MainVMSecond;
            InitConnection(_MainVMSecondary);
            SetDefault(_MainVMSecondary);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            //Act
            ShiftCancel();
        }

        [Test]
        public void StartLearnMode_WhenCancelled_NotFails()
        {
            //Act
            StartLearnModeCancel();
        }

        /// <summary>
        /// The Test Case of correctly removing secondary controller from network
        /// after non secured inclusion and reseting network key in it.
        /// 1. Enable security test scheme
        /// 2. Add 2nd ctrl - check on diff netkeys
        /// 3. Remove 2nd ctrl - check on diff netkeys
        /// 4. Disable security test scheme
        /// </summary>
        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AddNode_SecurityTestSchemeEnabled_NetworkKeysAreDiff(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.ShowSecuritySettingsCommand.CanExecute(_MainVMPrimary.SettingsViewModel))
            {
                //Arrange
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                ClearSecurParameters(_MainVMPrimary);
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.SelectedParameterS2Type = ParameterS2Type.Span;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestParameterS2Value = new byte[16];
                SaveSecurityTestParameter(_MainVMPrimary);
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS0Enabled = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.DelaySchemeGet.IsSet = true;
                _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.DelaySchemeGet.Value = 15;
                ApplySecuritySettings(_MainVMPrimary);
                InitSecondController(secondaryControllerSecurity);
                //Act
                AddNode(_MainVMPrimary, _MainVMSecondary);

                AssertNetworkKeys();

                SetSelectedNode(0x02);
                RequestNodeInfo();
                Delay(1000);
                SetAsSIS();
                AddNode(_MainVMSecondary);
                ControllerUpdate();
                SetSelectedNode(0x03);
                RequestNodeInfo();
                Assert_Node_IsSecured(_MainVMPrimary, 0x02);
                Assert_Node_IsSecured(_MainVMPrimary, 0x03);
                Assert_Node_IsSecured(_MainVMSecondary, 0x03);
            }
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void RequestNodeInfo_AfterSISInclusion_Updates(SecurityUnderTest secondaryControllerSecurity)
        {
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            SetSelectedNode(_MainVMSecondary.Controller.Id);
            SetAsSIS();
            AddNode(_MainVMSecondary);
            ControllerUpdate();
            SetSelectedNode(_MainVMPrimary.Nodes.Last().Item.Id);
            RequestNodeInfo();

            Assert.AreEqual(_MainVMSecondary.Nodes.Last().Item.Id, _MainVMPrimary.Nodes.Last().Item.Id);
            Assert.AreEqual(_MainVMSecondary.Nodes.Last().Item.SupportedCommandClasses, _MainVMSecondary.Nodes.Last().Item.SupportedCommandClasses);
            Assert.AreEqual(_MainVMSecondary.Nodes.Last().Item.SecurelyS0SupportedCommandClasses, _MainVMSecondary.Nodes.Last().Item.SecurelyS0SupportedCommandClasses);
            Assert.AreEqual(_MainVMSecondary.Nodes.Last().Item.SecurelyS2SupportedCommandClasses, _MainVMSecondary.Nodes.Last().Item.SecurelyS2SupportedCommandClasses);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void ReplicateWithNodeBySecondary_WhenMainIsSIS_SecurityAreSame(SecurityUnderTest secondaryControllerSecurity)
        {
            SetSelectedNode(_MainVMPrimary.Controller.Id);
            SetAsSIS();
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            AddNode(_MainVMSecondary);
            bool expectedIsNodeSecureS0 = ((Controller)_MainVMSecondary.Controller).Network.IsNodeSecureS0(0x03);
            bool expectedIsNodeSecureS2 = ((Controller)_MainVMSecondary.Controller).Network.IsNodeSecureS2(0x03);

            ReplicateWithNode(_MainVMSecondary);
            bool actualIsNodeSecureS0 = ((Controller)_MainVMSecondary.Controller).Network.IsNodeSecureS0(0x03);
            bool actualIsNodeSecureS2 = ((Controller)_MainVMSecondary.Controller).Network.IsNodeSecureS2(0x03);

            SetSelectedNode(0x03);
            RequestNodeInfo();
            if (_MainSecurityScheme != SecurityUnderTest.S0 && secondaryControllerSecurity != SecurityUnderTest.None)
            {
                Assert_Node_IsSecured(_MainVMPrimary, 0x03);
            }
            Assert.AreEqual(expectedIsNodeSecureS0, actualIsNodeSecureS0);
            Assert.AreEqual(expectedIsNodeSecureS2, actualIsNodeSecureS2);
        }
    }
}