/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class AssociationsTests : TestCaseBase
    {
        public AssociationsTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void AssociationCreate_BetweenTwoNodes_ControllerAndLastAddedNodeAreExist(SecurityUnderTest secondaryControllerSecurity)
        {
            //TC 55 -> association was created.
            //TC 56 -> association was created.
            if (_MainVMPrimary.MainMenuViewModel.ShowAssociationsCommand.CanExecute(null))
            {
                //Arrange
                byte[] expected = new byte[] { 0x01, 0x03 };
                byte testNodeId = 0x02;
                InitSecondController(secondaryControllerSecurity);
                AddNode(_MainVMPrimary);
                AddNode(_MainVMPrimary, _MainVMSecondary);
                //Act.
                SetSelectedNode(testNodeId);
                //Assert.
                Assert.AreEqual(2, _MainVMPrimary.AssociationViewModel.AssociativeApplications.Count);
                //Arrange.
                var ad = _MainVMPrimary.AssociationViewModel.AssociativeApplications.
                    First(x => x.RootDevice.Id == testNodeId).Devices.
                    First(x => x.Device.Id == testNodeId && x.Device.EndPointId == 0);
                _MainVMPrimary.AssociationViewModel.SelectedAssociativeDevice = ad;
                AssociationGetGroupsInfo();
                Assert.IsNotNull(ad.Groups);
                _MainVMPrimary.AssociationViewModel.SelectedGroup = ad.Groups.First();
                SetSelectedNode(3);
                _MainVMPrimary.AssociationViewModel.SelectedNodeIds.Add(_MainVMPrimary.GetNodePairId(_MainVMPrimary.SelectedNode.Item.Id, 0));
                //Act.
                AssociationCreate();
                var actual = _MainVMPrimary.AssociationViewModel.SelectedGroup.Nodes.NodeIds.Select(x => x.Item).ToArray();
                //Assert.
                Assert.AreEqual(expected, actual);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupName);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupInfo);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupCommandClasses);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Test]
        public void AssociationRemove_WithController_EmptyAssociationGroup()
        {

            //TC 56 -> association was removed.
            if (_MainVMPrimary.MainMenuViewModel.ShowAssociationsCommand.CanExecute(null))
            {
                //Arrange.
                AddNode(_MainVMPrimary);
                //Act.
                SetSelectedNode(_MainVMPrimary.Nodes.Last().Item.Id);
                //Assert.
                Assert.AreEqual(1, _MainVMPrimary.AssociationViewModel.AssociativeApplications.Count);
                //Arrange.
                var ad = _MainVMPrimary.AssociationViewModel.AssociativeApplications.
                    First(x => x.RootDevice.Id == _MainVMPrimary.Nodes.Last().Item.Id).Devices.
                    First(x => x.Device.Id == _MainVMPrimary.Nodes.Last().Item.Id && x.Device.EndPointId == 0);
                _MainVMPrimary.AssociationViewModel.SelectedAssociativeDevice = ad;
                AssociationGetGroupsInfo();
                _MainVMPrimary.AssociationViewModel.SelectedGroup = ad.Groups.First();
                _MainVMPrimary.AssociationViewModel.SelectedAssociativeNode = _MainVMPrimary.AssociationViewModel.SelectedGroup.Nodes.NodeIds.First();
                _MainVMPrimary.AssociationViewModel.SelectedAssociativeNode = _MainVMPrimary.AssociationViewModel.SelectedGroup.Nodes.NodeIds[0];
                //Act.
                AssociationRemove();
                //Assert.
                Assert.IsNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.Nodes);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupName);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupInfo);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupCommandClasses);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Test]
        public void AssociationGetNodes_Default_ToController()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowAssociationsCommand.CanExecute(null))
            {
                //Arrange.
                byte[] expected = new byte[] { 0x01 };
                AddNode(_MainVMPrimary);
                //Act.
                SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
                //Assert.
                Assert.AreEqual(1, _MainVMPrimary.AssociationViewModel.AssociativeApplications.Count);
                //Arrange.
                var ad = _MainVMPrimary.AssociationViewModel.AssociativeApplications.
                    First(x => x.RootDevice.Id == _MainVMPrimary.Nodes.Last().Item.Id).Devices.
                    First(x => x.Device.Id == _MainVMPrimary.Nodes.Last().Item.Id && x.Device.EndPointId == 0);
                _MainVMPrimary.AssociationViewModel.SelectedAssociativeDevice = ad;
                AssociationGetGroupsInfo();
                _MainVMPrimary.AssociationViewModel.SelectedGroup = ad.Groups[0];
                //Act.
                AssociationGetNodes();
                //Assert.
                var actualGetNodes = _MainVMPrimary.AssociationViewModel.SelectedGroup.Nodes.NodeIds.Select(x => x.Item).ToArray();
                Assert.AreEqual(expected, actualGetNodes);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupName);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupInfo);
                Assert.IsNotNull(_MainVMPrimary.AssociationViewModel.SelectedGroup.GroupCommandClasses);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

    }
}