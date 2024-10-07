/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using ZWaveController.Models;

namespace IntegrationTests
{
    [Ignore("Manual")]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    //[TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    //[TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class ManualTests : TestCaseBase
    {
        public ManualTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void NetworkWideInclusionExclusion_WhenCalled_AddesAndRemovesTwoNodes()
        {
            //Arrange
            RemoveNodeFromNetwork(_MainVMPrimary);
            RemoveNodeFromNetwork(_MainVMPrimary);
            //Act1
            NetworkWideInclusion();
            //Act2
            NetworkWideExclusion();
            //Assert
            Assert.AreEqual(EXPECTED_NODESCOUNT_SETDAFAULT, _MainVMPrimary.Nodes.Count);
        }

        [Test]
        public void SetWakeUpInterval_ToNonListeningNode_CommandsQueueIncreases()
        {
            //Arrange
            RemoveNodeFromNetwork(_MainVMPrimary);
            AddNode(_MainVMPrimary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            Assert.IsFalse(_MainVMPrimary.SelectedNode.Item.IsListening);
            var expected = 0;
            //Act
            _MainVMPrimary.NetworkManagement.WakeupIntervalValue = 1;
            SetWakeupInterval();
            if (_MainVMPrimary.Controller is BasicControllerModel)
            {
                var actual = (_MainVMPrimary.Controller as BasicControllerModel).WakeUpNodesWorker.QueueItems.Count();
                Assert.Greater(actual, expected);
            }
        }

        [Test]
        public void CommandQueue_AfterOperationsWithListeningNode_Changes()
        {
            Console.WriteLine("CommandQueueCheck");
            if (_MainVMPrimary.ShowCommandQueueCommand.CanExecute(null))
            {
                //Assert
                var QueueItems = (_MainVMPrimary.ControllerModel as BasicControllerModel).WakeUpNodesWorker.QueueItems;
                AddNode(_MainVMPrimary);
                SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
                Assert.IsFalse(_MainVMPrimary.SelectedNode.Item.IsListening);
                var expected = QueueItems.Count(x => x.Item.NodeId == _MainVMPrimary.SelectedNode.Item.Id) + 2;
                //Act1
                SendNOP();
                _MainVMPrimary.NetworkManagement.WakeupIntervalValue = 1;
                SetWakeupInterval();
                var actual = QueueItems.Count(x => x.Item.NodeId == _MainVMPrimary.SelectedNode.Item.Id);
                //Assert1
                Assert.AreEqual(expected, actual);
                //Act2
                MessageBox.Show("Press Reset button on the board to send Wake Up Notification from node and OK after");
                var actual2 = QueueItems.Count(x => x.Item.NodeId == _MainVMPrimary.SelectedNode.Item.Id);
                //Assert2
                Assert.Less(actual2, actual);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }


    }
}
