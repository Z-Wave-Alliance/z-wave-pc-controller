/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWaveController.Models;
using ZWave.CommandClasses;
using ZWave.BasicApplication.Devices;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class PollingTests : TestCaseBase
    {
        const int POLL_TIME = 2;
        const int POLL_COUNT = 5;

        public PollingTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }
        
        [Test]
        public void StartStopPolling_LastAddedNode()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowPollingCommand.CanExecute(null))
            {
                AddNode(_MainVMPrimary);
                var polNode = _MainVMPrimary.PollingViewModel.Nodes.First(x => x.Device.EndPointId == 0);
                polNode.PollTime = POLL_TIME;
                polNode.ReportTime = 1;

                PollingStart();

                int totalSec = POLL_COUNT * POLL_TIME - POLL_TIME / 2;
                Delay(totalSec * 1000);
                PollingStop();
                Delay(POLL_TIME * 1000);

                Assert.AreEqual(POLL_COUNT, polNode.Requests, "Polling. Requests count error");

                int expMisReport = POLL_COUNT;
                if ((_MainVMPrimary.Nodes[0x01].Item.SupportedCommandClasses != null && _MainVMPrimary.Nodes[0x01].Item.SupportedCommandClasses.Contains(COMMAND_CLASS_VERSION.ID)) ||
                    (_MainVMPrimary.Nodes[0x01].Item.SecurelyS0SupportedCommandClasses != null && _MainVMPrimary.Nodes[0x01].Item.SecurelyS0SupportedCommandClasses.Contains(COMMAND_CLASS_VERSION.ID)) ||
                    (_MainVMPrimary.Nodes[0x01].Item.SecurelyS2SupportedCommandClasses != null && _MainVMPrimary.Nodes[0x01].Item.SecurelyS2SupportedCommandClasses.Contains(COMMAND_CLASS_VERSION.ID)))
                {
                    expMisReport = 0;
                }
                Assert.AreEqual(expMisReport, polNode.MissingReports);
                Assert.AreEqual(0, polNode.Failures);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }
    }
}
