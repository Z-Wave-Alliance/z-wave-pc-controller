/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWaveController.ViewModels;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class IMANetworkTests : TestCaseBase
    {
        private void SelectIMASourceNode()
        {
            ((IMADeviceInfo)_MainVMPrimary.IMAFullNetworkViewModel
                .Items
                .Last(x => x is IMADeviceInfo))
                .IsSelected = true;

            _MainVMPrimary.IMAFullNetworkViewModel.SourceNodeId = _MainVMPrimary.Nodes[1].Item.Id;
        }

        public IMANetworkTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void IMARequestNodeInfo_CallToSelected_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMARequestNodeInfo();
        }

        [Test]
        public void IMAGetVersionCommand_CallToSelected_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMAGetVersion();
        }

        [Test]
        public void IMAPingNode_CallToSelected_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMAPingNode();
        }

        [Test]
        public void IMARediscovery_CallToSelected_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMARediscovery();
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void IMAPowerLevelTest_TwoNodes_DoesNotFail(SecurityUnderTest secondaryControllerSecurity)
        {
            //Arrange.
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary);
            SetSelectedNode(2);
            RequestNodeInfo();
            Assert.IsTrue(_MainVMPrimary.Nodes.First(x => x.Item.Id == 0x02 && x.Item.EndPointId == 0).Item.IsCommandClassSupported(ZWave.CommandClasses.COMMAND_CLASS_POWERLEVEL.ID),
                "Added Slave doesn't support COMMAND_CLASS_POWERLEVEL");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Assert.IsTrue(_MainVMPrimary.Nodes.Last().Item.IsCommandClassSupported(ZWave.CommandClasses.COMMAND_CLASS_POWERLEVEL.ID),
                "Added 2nd controller doesn't support COMMAND_CLASS_POWERLEVEL");
            Assert.IsTrue(_MainVMPrimary.IMAFullNetworkViewModel.Items.Any(x => x.Id == 0x02), "Slave is not in IMA Nodes List");
            Assert.IsTrue(_MainVMPrimary.IMAFullNetworkViewModel.Items.Any(x => x.Id == 0x03), "2nd Controller is not in IMA List");
            _MainVMPrimary.IMAFullNetworkViewModel.SourceNodeId = 2;
            _MainVMPrimary.IMAFullNetworkViewModel.DestinationNodeId = 3;
            IMAPowerLevelTest();
        }

        [Test]
        public void IMAPowerLevelCancel_WhenCanceled_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            _MainVMPrimary.IMAFullNetworkViewModel.SourceNodeId = 2;
            _MainVMPrimary.IMAFullNetworkViewModel.DestinationNodeId = 1;
            //Act
            IMAPowerLevelCancel();
            Delay(10000);
        }

        [Test]
        public void IMAReloadRoutingInfo_Always_Executes()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            //Act
            IMAReloadRoutingInfo();
        }

        [Ignore("THIS TC HANGS THE SYSTEM - ApplicationNodeInformationOperation ISSUE, wait for fix")]
        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void IMANetworkHealth_NoSelectedFrom2Nodes_FullNetworkTest(SecurityUnderTest secondaryControllerSecurity)
        {
            //Arrange.
            InitSecondController(secondaryControllerSecurity);
            AddNode(_MainVMPrimary);
            AddNode(_MainVMPrimary, _MainVMSecondary);
            //Act
            IMANetworkHealth();
        }

        [Test]
        public void IMANetworkHealth_CallToSelected_SelectedNodeTest()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMANetworkHealth();
        }

        [Test]
        public void IMANetworkHealthCancel_WhenCanceled_DoesNotFail()
        {
            //Arrange
            AddNode(_MainVMPrimary);
            SelectIMASourceNode();
            //Act
            IMANetworkHealthCancel();
            Delay(10000);
        }

    }
}
