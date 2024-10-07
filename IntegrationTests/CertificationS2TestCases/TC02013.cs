/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWave.CommandClasses;
using ZWaveController.ViewModels;
using ZWave.BasicApplication.Security;
using ZWave.BasicApplication.Enums;
using ZWave.BasicApplication.Devices;
using ZWaveController.Models;
using ZWave;
using ZWave.Enums;
using Utils.UI.MVVM;
using Utils;
using ZWave.BasicApplication.Operations;

namespace IntegrationTests.CertificationS2TestCases
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    public class TC02013 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02013

        public TC02013(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);
            InitBridgeController(SecurityUnderTest.S2);

            Console.WriteLine("1. Add the 10 switch on/off to the PC Controller (1).");
            AddNode(_MainVMPrimary, _MainVMBridge);
            Assert_Node_IsSecured(_MainVMPrimary, _MainVMPrimary.Nodes.Last().Item.Id);
            for (int i = 0; i < 10; i++)
            {
                AddNodeVirtual(_MainVMPrimary, _MainVMBridge);
                //In 1. and 2. verify that the inclusion was successful and that security v2 is enabled.
                Assert_Node_IsSecured(_MainVMPrimary, _MainVMPrimary.Nodes.Last().Item.Id);
            }
            Console.WriteLine("2. Add DUT to the PC Controller (1) network.");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Assert_Node_IsSecured(_MainVMPrimary, _MainVMPrimary.Nodes.Last().Item.Id);


            Console.WriteLine("3. Send BASIC GET from DUT to, switch on/off with node ID from 3 to 11 (in this order) and lastly to PC Controller ()1.");
            _MainVMSecondary.CommandClassesViewModel.Payload = new COMMAND_CLASS_BASIC_V2.BASIC_GET();
            for (byte i = 3; i < 11; i++)
            {
                SetSelectedNode(_MainVMSecondary, i);
                SendCommand(_MainVMSecondary);
                //"In 3. verify that all the receiver node's SPAN are in synch with the Primary Controller (No S2_NONCE_REPORT_sos is sent)"
                Assert_Node_IsSecured(_MainVMSecondary, i);
            }

            Console.WriteLine("4. Send BASIC GET to switch on/off with node ID 2.");
            SetSelectedNode(_MainVMSecondary, 0x02);
            SendCommand();
            //"In 4. verify that DUT request a new SPAN from node 2."
            Assert_Node_IsSecured(_MainVMSecondary, 0x02);

            //*

            Console.WriteLine("5. Send BASIC GET to switch on/off with node ID from 4 to 11 (in this order) then to PC Controller (1) and lastly to switch on/off with node ID 2.");
            for (byte i = 4; i < 11; i++)
            {
                SetSelectedNode(_MainVMSecondary, i);
                SendCommand(_MainVMSecondary);
                //"In 5. verify that all the receiver node's SPAN are in synch with DUT (No S2_NONCE_REPORT_sos is sent)"
                Assert_Node_IsSecured(_MainVMSecondary, i);
            }
            Console.WriteLine("5..and lastly to switch on/off with node ID 2.");
            SetSelectedNode(_MainVMSecondary, 0x02);
            SendCommand();
            Assert_Node_IsSecured(_MainVMSecondary, 0x02);

            Console.WriteLine("6. Send BASIC GET to switch on/off with node ID 3.");
            SetSelectedNode(_MainVMSecondary, 0x03);
            SendCommand();
            //In 6. verify that DUT request a new SPAN from node 3.
            Assert_Node_IsSecured(_MainVMSecondary, 0x03);
        }
    }
}