/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWave.BasicApplication.Devices;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    class InclusionBridgeCustomTests : TestCaseBase
    {
        public InclusionBridgeCustomTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AddNodeToSecondSIS_AfterMainUpdate_MainNodeListChanges(SecurityUnderTest secondaryControllerSecurity)
        {
            InitBridgeController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMBridge);
            SetSelectedNode(0x02);
            SetAsSIS();
            AddNode(_MainVMBridge);

            Assert.AreNotEqual(_MainVMPrimary.Nodes.Count, _MainVMBridge.Nodes.Count);
            var expected = _MainVMBridge.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            ControllerUpdate();
            var actual = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            //Assert
            Assert.AreEqual(expected, actual);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            SetSelectedNode(3);
            RequestNodeInfo();
            Assert_Node_IsSecured(_MainVMBridge, 0x03);
            Assert_Node_IsSecured(_MainVMPrimary, 0x03);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AddNodeToPrimary_WhenSecondarySIS_AutoUpdatesNodeList(SecurityUnderTest secondaryControllerSecurity)
        {
            //Arrange.
            InitBridgeController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMBridge);
            SetSelectedNode(2);
            SetAsSIS();
            //Act2
            AddNode(_MainVMPrimary);
            var expected = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            Delay(500);
            var actual = _MainVMBridge.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            //Assert
            Assert.AreEqual(expected, actual);
            Assert_Node_IsSecured(_MainVMPrimary, 3);
            Assert_Node_IsSecured(_MainVMBridge, 3);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void SecondSISNoInclusionCC_InclusionAddNode_NodeAddedWithNeededScheme(SecurityUnderTest secondaryControllerSecurity)
        {
            InitBridgeController(secondaryControllerSecurity);

            RemoveInclusionSupportCC(_MainVMPrimary);
            RemoveInclusionSupportCC(_MainVMBridge);
            AddNode(_MainVMPrimary, _MainVMBridge);
            SetSelectedNode(0x02);
            SetAsSIS();
            AddNode(_MainVMBridge);

            Assert.AreNotEqual(_MainVMPrimary.Nodes.Count, _MainVMBridge.Nodes.Count);
            var expected = _MainVMBridge.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            ControllerUpdate();
            var actual = _MainVMPrimary.Nodes.Where(x => x.Item.EndPointId == 0).Count();
            //Assert
            Assert.AreEqual(expected, actual);
            AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            SetSelectedNode(3);
            RequestNodeInfo();
            Assert_Node_IsSecured(_MainVMBridge, 0x03);
            Assert_Node_IsSecured(_MainVMPrimary, 0x03);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AddNode_ReplicationWhen1stSIS_SecuritySame(SecurityUnderTest secondaryControllerSecurity)
        {
            //TO# 07041
            InitBridgeController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMBridge);
            bool expectedIsNodeSecureS0 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS0(_MainVMPrimary.Nodes.Last().Item.Id);
            bool expectedIsNodeSecureS2 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS2(_MainVMPrimary.Nodes.Last().Item.Id);
            SetSelectedNode(_MainVMPrimary.Controller.Id);
            SetAsSIS();
            Delay(2000);
            AddNode(_MainVMPrimary, _MainVMBridge);
            bool actualIsNodeSecureS0 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS0(_MainVMPrimary.Nodes.Last().Item.Id);
            bool actualIsNodeSecureS2 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS2(_MainVMPrimary.Nodes.Last().Item.Id);

            AssertNetworkKeys(_MainVMBridge);
            AssertSecondarySecurity(_MainVMBridge);
            Assert.AreEqual(expectedIsNodeSecureS0, actualIsNodeSecureS0);
            Assert.AreEqual(expectedIsNodeSecureS2, actualIsNodeSecureS2);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AddNode_ReplicationWhen2ndSIS_SecuritySame(SecurityUnderTest secondaryControllerSecurity)
        {
            InitBridgeController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMBridge);
            bool expectedIsNodeSecureS0 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS0(_MainVMPrimary.Nodes.Last().Item.Id);
            bool expectedIsNodeSecureS2 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS2(_MainVMPrimary.Nodes.Last().Item.Id);
            SetSelectedNode(_MainVMPrimary.Nodes.Last().Item.Id);
            SetAsSIS();
            AddNode(_MainVMPrimary, _MainVMBridge);
            bool actualIsNodeSecureS0 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS0(_MainVMPrimary.Nodes.Last().Item.Id);
            bool actualIsNodeSecureS2 = ((Controller)_MainVMPrimary.Controller).Network.IsNodeSecureS2(_MainVMPrimary.Nodes.Last().Item.Id);

            AssertNetworkKeys(_MainVMBridge);
            AssertSecondarySecurity(_MainVMBridge);
            Assert.AreEqual(expectedIsNodeSecureS0, actualIsNodeSecureS0);
            Assert.AreEqual(expectedIsNodeSecureS2, actualIsNodeSecureS2);
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void ReplaceFailedOnSIS_InclusionCtrlToSlave_NodeInfoChanged(SecurityUnderTest secondaryControllerSecurity)
        {
            SetSelectedNode(_MainVMPrimary.Controller.Id);
            SetAsSIS();
            InitBridgeController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary, _MainVMBridge);
            SetDefault(_MainVMBridge);
            SetSelectedNode(0x02);
            var expectedIsSlaveApi = _MainVMPrimary.Nodes.Last().Item.IsSlaveApi;

            IsFailedNode();
            ReplaceFailedNodeWithSlave();
            var actualIsSlaveApi = _MainVMPrimary.Nodes.Last().Item.IsSlaveApi;

            Assert.AreNotEqual(expectedIsSlaveApi, actualIsSlaveApi);
            Assert_Node_IsSecured(_MainVMPrimary, 0x02);
        }
    }
}
