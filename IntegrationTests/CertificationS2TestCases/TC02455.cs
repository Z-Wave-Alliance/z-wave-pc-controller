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
    public class TC02455 : TestCaseBase
    {
        //  http://zensys05/systestdb/tcs_main.php?action=view&id=02455

        public TC02455(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void ExecuteTest()
        {
            InitSecondController(SecurityUnderTest.S2);
            InitBridgeController(SecurityUnderTest.S2);
            List<ReceivedLogItem> actLogItems = new List<ReceivedLogItem>();
            (_MainVMSecondary.ControllerModel as BasicControllerModel).Controller.
                ListenData((x) => actLogItems.Add(new ReceivedLogItem(B_JoinNodeId, x.SrcNodeId, x.Command, x.SecurityScheme)));

            Console.WriteLine("1.- Include DUT");
            AddNode(_MainVMPrimary, _MainVMSecondary);
            Console.WriteLine("And switch on/off (1,2,3,4,5) to PC Controller (1)");
            AddNode(_MainVMPrimary, _MainVMBridge);
            const byte slId1 = 3; const byte slId2 = 4; const byte slId3 = 5; const byte slId4 = 6; const byte slId5 = 7;
            for (int i = 0; i < 5; i++)
            {
                AddNodeVirtual(_MainVMPrimary, _MainVMBridge);
            }
            AddNode(_MainVMPrimary, _MainVMSecondary);

            Console.WriteLine("2.- DUT requests Node Info from all Switch nodes and PC Controller (1);");
            foreach (var item in _MainVMSecondary.Nodes)
            {
                if (item.Item.Id == _MainVMSecondary.Controller.Id) continue;
                SetSelectedNode(_MainVMSecondary, item.Item.Id);
                RequestNodeInfo(_MainVMSecondary);
                Assert_Node_IsSecured(_MainVMSecondary, item.Item.Id);
            }
            Console.WriteLine("..2.- DUT sends a multicast frame to; PC Controller (1), switches (1,2);");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId1, slId2);
            BasicSetOn(_MainVMSecondary);
            Console.WriteLine("..2. and another multicast frame to; PC Controller (1), switches (2,3).");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId2, slId3);
            BasicSetOn(_MainVMSecondary);
            LoadMpanTable(_MainVMSecondary);
            Assert.IsTrue((_MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.Count() >= 2));
            Assert.AreNotEqual(
                _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[0].GroupId,
                _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[1].GroupId,
                "In 2. verify that, the multicast groups have different GROUP ID [009F.01.00.11.025]");
            var catched = actLogItems.Where(x => x.SenderNodeId == A_IncusionNodeId &&
                        x.Command[0] == COMMAND_CLASS_SUPERVISION.ID &&
                        x.Command[1] == COMMAND_CLASS_SUPERVISION.SUPERVISION_REPORT.ID);
            Assert.AreEqual(2, catched.Count(), "PC Controller sends a S2 singlecast follow up to each S2 Multicast group member after sending the multicast frame [009F.01.00.12.007][009F.01.00.12.009]");


            Console.WriteLine("3.- DUT sends a multicast frame to; PC Controller (1), switches (1,2);");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId1, slId2);
            BasicSetOn(_MainVMSecondary);
            Console.WriteLine(" and another multicast frame to PC Controller (1), switches (2,3)");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId2, slId3);
            BasicSetOn(_MainVMSecondary);
            LoadMpanTable(_MainVMSecondary);
            Assert.IsTrue((_MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.Count() >= 2));
            var reqResyncMPAN = _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.
                            FullMpanTable.FirstOrDefault(x => x.OwnerId == B_JoinNodeId && x.IsMos);
            Assert.IsNull(reqResyncMPAN, "In 3. verify that DUT does not require resynchronization to maintain multiple multicast groups [009F.01.00.13.001]");
            Assert.AreNotEqual(_MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[0].MpanValue,
                                _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable[1].MpanValue,
                                "verify that each group uses a different MPAN [009F.01.00.11.026]");

            Console.WriteLine("4.- PC Controller (1) sends a multicast frame to DUT and switches (1,2)");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            BasicSetOn(_MainVMPrimary);
            LoadMpanTable(_MainVMPrimary);
            Assert.IsTrue(_MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.Count > 0);
            //byte actualGroupId 
            var mpanPrim = _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.
                    FirstOrDefault(x => x.OwnerId == A_IncusionNodeId &&
                                    x.NodeIds.Contains(B_JoinNodeId) &&
                                    x.NodeIds.Contains(slId1) &&
                                    x.NodeIds.Contains(slId2));
            var primMpanValue = mpanPrim.MpanValue;
            var groupId = mpanPrim.GroupId;

            Console.WriteLine("5.- DUT sends a multicast frame to PC Controller (1) and switches (1,2)");
            SetSelectedNode(_MainVMSecondary, A_IncusionNodeId, slId1, slId2);
            BasicSetOn(_MainVMSecondary);
            LoadMpanTable(_MainVMSecondary);
            var secMpanValue = _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.FullMpanTable.
                FirstOrDefault(x => x.OwnerId == B_JoinNodeId &&
                                    x.NodeIds.Contains(A_IncusionNodeId) &&
                                    x.NodeIds.Contains(slId1) &&
                                    x.NodeIds.Contains(slId2)).MpanValue;

            Assert.AreNotEqual(primMpanValue, secMpanValue, "In 5. verify that DUT does not re-use the MPAN used by PC Controller (1) [009F.01.00.11.027]");

            Console.WriteLine("6.- PC Controller (1) generates next MPAN");
            _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.SelectedMpanItem =
                    _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.
                    FullMpanTable.FirstOrDefault(x => x.OwnerId == A_IncusionNodeId && x.GroupId == groupId);
            NextMPAN(_MainVMPrimary);

            Console.WriteLine("7.- PC Controller (1) sends a multicast frame to DUT and switches (1,2) using the newly generated MPAN");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            BasicSetOn(_MainVMPrimary);
            Console.WriteLine("IN ZNIFFER only: [Optional] In 7. verify that DUT iterates its own MPAN to decrypt the received multicast message [009F.01.00.13.002] [009F.01.00.12.00A]");

            Console.WriteLine("8.- PC Controller (1) generates next 7 MPAN");
            LoadMpanTable(_MainVMPrimary);
            _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.SelectedMpanItem =
                    _MainVMPrimary.NetworkManagement.MpanTableConfigurationVM.
                    FullMpanTable.FirstOrDefault(x => x.OwnerId == A_IncusionNodeId && x.GroupId == groupId);
            for (int i = 0; i < 7; i++)
            {
                NextMPAN(_MainVMPrimary);
            }

            Console.WriteLine("9.- PC Controller (1) sends a multicast frame to DUT and switches (1,2) using the newly generated MPAN");
            SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
            BasicSetOn(_MainVMPrimary);
            LoadMpanTable(_MainVMSecondary);
            var actualMPAN = _MainVMSecondary.NetworkManagement.MpanTableConfigurationVM.
                            FullMpanTable.FirstOrDefault(x => x.OwnerId == A_IncusionNodeId && x.GroupId == groupId);
            Assert.IsNotNull(actualMPAN);
            Console.WriteLine("IN ZNIFFER only: verify that DUT sets the MOS flag for the actual Group ID [009F.01.00.11.02A]");
            //Assert.IsTrue(actualMPAN.IsMos, "ZNIFFER ONLY");
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            Assert_Node_IsSecured(_MainVMPrimary, slId1);
            Assert_Node_IsSecured(_MainVMPrimary, slId2);
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine("10.- PC Controller (1) sends a multicast frame to the following groups:");
                Console.WriteLine("DUT and switches (1,2),");
                SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
                BasicSetOn(_MainVMPrimary);
                Console.WriteLine("DUT and switches (2,3),");
                SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId2, slId3);
                BasicSetOn(_MainVMPrimary);
                Console.WriteLine("DUT and switches (3,4),");
                SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId3, slId4);
                BasicSetOn(_MainVMPrimary);
                Console.WriteLine("DUT and switches (4,5),");
                SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId4, slId5);
                BasicSetOn(_MainVMPrimary);
                Console.WriteLine("DUT and switches (1,5)");
                SetSelectedNode(_MainVMPrimary, B_JoinNodeId, slId1, slId2);
                BasicSetOn(_MainVMPrimary);
                Console.WriteLine("11.- Repeat 10.");
                LoadMpanTable(_MainVMPrimary);
            }
            Console.WriteLine("IN ZNIFFER only: In 11. verify that DUT is able to keep synchronization of the 5 multicast groups [009F.01.00.11.02B]");
            Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            Assert_Node_IsSecured(_MainVMPrimary, slId1);
            Assert_Node_IsSecured(_MainVMPrimary, slId2);
            Assert_Node_IsSecured(_MainVMPrimary, slId3);
            Assert_Node_IsSecured(_MainVMPrimary, slId4);
            Assert_Node_IsSecured(_MainVMPrimary, slId5);
        }

    }

}