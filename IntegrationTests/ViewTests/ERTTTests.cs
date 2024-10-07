/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using ZWaveController.Models;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class ERTTTests : TestCaseBase
    {
        private void SelectSecondNode()
        {
            _MainVMPrimary.ERTTViewModel.SelectedNodes.Clear();
            _MainVMPrimary.ERTTViewModel.SelectedNodes.Add(_MainVMPrimary.Nodes[1].Item);
        }

        public ERTTTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }

        [Test]
        public void ERTTStart_TestIterations_PacketsEqualIterations()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowERTTCommand.CanExecute(null))
            {
                //Arrange.
                const int expErrorCount = 0;
                AddNode(_MainVMPrimary);
                SelectSecondNode();
                //Act.
                _MainVMPrimary.ERTTViewModel.TestIterations = 20;
                _MainVMPrimary.ERTTViewModel.IsRunForever = false;
                //mMainVM.ERTTViewModel.IsRetransmission = false;
                ERTTStart();
                //Assert.
                Assert.AreEqual(expErrorCount, _MainVMPrimary.ERTTViewModel.ResultItems[0].ErrorsCount);
                Assert.AreEqual(_MainVMPrimary.ERTTViewModel.TestIterations, _MainVMPrimary.ERTTViewModel.PacketsSent);
                Assert.AreEqual(_MainVMPrimary.ERTTViewModel.TestIterations, _MainVMPrimary.ERTTViewModel.PacketsRecieved);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [TestCase(SecurityUnderTest.None)]
        [TestCase(SecurityUnderTest.S0)]
        [TestCase(SecurityUnderTest.S2)]
        public void ERTTStart_StopOnError_ExpectedErrorCount(SecurityUnderTest secondaryControllerSecurity)
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowERTTCommand.CanExecute(null))
            {
                //Arrange.
                InitSecondController(secondaryControllerSecurity);
                const int expected = 1;
                AddNode(_MainVMPrimary, _MainVMSecondary);
                SelectSecondNode();
                //Act.
                _MainVMPrimary.ERTTViewModel.IsRunForever = true;
                _MainVMPrimary.ERTTViewModel.IsStopOnError = true;
                var ret = ERTTStartRet();
                Delay(2000);
                SetDefault(_MainVMSecondary);
                ret.WaitOne();
                //Assert.
                Assert.Greater(_MainVMPrimary.ERTTViewModel.PacketsSent, 0);
                Assert.Greater(_MainVMPrimary.ERTTViewModel.PacketsRecieved, 0);
                Assert.AreEqual(expected, _MainVMPrimary.ERTTViewModel.ResultItems[0].ErrorsCount, "Errors count wrong");
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Test]
        public void ERTTStop_RunForever_StopedAfter10Seconds()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowERTTCommand.CanExecute(null))
            {
                //Arrange.
                AddNode(_MainVMPrimary);
                SelectSecondNode();
                _MainVMPrimary.ERTTViewModel.IsRunForever = true;
                //Act.
                var ret = ERTTStartRet();
                Delay(10000);
                ERTTStop();
                ret.WaitOne();
                //Assert.
                Assert.Greater(_MainVMPrimary.ERTTViewModel.PacketsSent, 0);
                Assert.Greater(_MainVMPrimary.ERTTViewModel.PacketsRecieved, 0);
                Assert.AreEqual(0, _MainVMPrimary.ERTTViewModel.ResultItems[0].ErrorsCount);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

        [Ignore("Test for TX controller module")]
        [Test]
        public void ERTTStart_TxControllerByModule_Expected()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowERTTCommand.CanExecute(null))
            {
                Assert.IsTrue(false, "Not realized!");
            }
        }
    }
}
