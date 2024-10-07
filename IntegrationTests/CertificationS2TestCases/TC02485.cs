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
    public class TC02485 : TestCaseBase
    {
        public TC02485(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void ExecuteTest_Step1(byte attemp)
        {
            InitSecondController(SecurityUnderTest.S2);

            Console.WriteLine("1.- Include DUT to PC Controller (1), but setting PC Controller (1) for each attempt to: ");

            switch (attemp)
            {
                case (1):
                    Console.WriteLine(".1- To grant a subset of the network keys");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXSet;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_SET()
                            {
                                selectedKexScheme = 02,
                                selectedEcdhProfile = 01,
                                grantedKeys = 0x81
                            };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case (2):
                    Console.WriteLine(".2- To send a wrong KEX Report Echo");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXReportEcho;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_REPORT()
                    {
                        properties1 = new COMMAND_CLASS_SECURITY_2.KEX_REPORT.Tproperties1() { echo = 1 },
                    };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case (3):
                    Console.WriteLine(".3- To announce only Access and Authenticated keys, but attempt to request Unauthenticated during inclusion");
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_UNAUTHENTICATED = false;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.NetworkKeyGet_S2Authenticated;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_GET()
                    {
                        requestedKey = 0
                    };
                    SetS2TestFrame(_MainVMSecondary);
                    ApplySecuritySettings(_MainVMSecondary);
                    break;
                case (4):
                    Console.WriteLine(".4- To attempt to request first Unauthenticated key, and then the rest");
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.NetworkKeyGet_S2Access;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_GET()
                    {
                        requestedKey = 0
                    };
                    SetS2TestFrame(_MainVMSecondary);
                    ApplySecuritySettings(_MainVMSecondary);
                    break;
                case (5):
                    Console.WriteLine(".5- To send S2 Network Key Report with a non requested network key");
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_ACCESS = false;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsEnabledSecurityS2_AUTHENTICATED = false;
                    ApplySecuritySettings(_MainVMSecondary);
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.NetworkKeyReport_S2Unauthenticated;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.SECURITY_2_NETWORK_KEY_REPORT()
                    {
                        grantedKey = 0x02,
                        networkKey = new byte[16]
                    };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case(6):
                    Console.WriteLine(".6- To delay KEX Report echo 120 seconds");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXReportEcho;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 120;
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case(7):
                    Console.WriteLine(".7- To delay KEX Report echo 120 seconds and then send KEX Fail instead");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXReportEcho;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameDelay.Value = 120;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_FAIL { kexFailType = 0x05 };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case(8):
                    Console.WriteLine(".8- PC Controller grants zero keys to DUT");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXSet;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_SET()
                    {
                        selectedKexScheme = 02,
                        selectedEcdhProfile = 01,
                        grantedKeys = 0x00
                    };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
            }

            AddNode(_MainVMPrimary, _MainVMSecondary);
            //Verifications:
            if (attemp == 1 || attemp == 6)
            {
                //In 1.1. verify that DUT continues with inclusion normally [009F.01.00.11.092], verify also that DUT sends the security 2 Network Key Verify encrypted with the recently exchanged key [009F.01.00.11.095]
                //In 1.6 verify that DUT continues S2 bootstrapping [009F.01.00.11.097]
                Assert_Node_IsSecured(_MainVMPrimary, B_JoinNodeId);
            }
            else
            {
                //In 1.2 verify that DUT aborts S2 bootstrapping [009F.01.00.11.064]
                //In 1.3 verify that DUT aborts S2 bootstrapping [009F.01.00.11.066]
                //In 1.4 verify that DUT aborts S2 bootstrapping [009F.01.00.11.066]
                //In 1.5 verify that DUT aborts S2 bootstrapping [009F.01.00.11.094]
                //In 1.7 verify that DUT aborts S2 bootstrapping [009F.01.00.11.097]
                //In 1.8 verify that DUT aborts S2 bootstrapping and reports inclusion as non-secure and successful [009F.01.00.11.098]
                Assert_Slave_IsNonS2Secured(_MainVMPrimary, B_JoinNodeId);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void ExecuteTest_Step2(byte attemp)
        {
              InitSecondController(SecurityUnderTest.S2);

            Console.WriteLine("1.- Include DUT to PC Controller (1), but setting PC Controller (1) for each attempt to: ");

            switch (attemp)
            {
                case (1):
                    Console.WriteLine(".1- PC Controller (1) To send a wrong KEX Set Echo");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXSetEcho;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_SET()
                            {
                                properties1 = new COMMAND_CLASS_SECURITY_2.KEX_SET.Tproperties1 { echo = 0x01},
                                selectedKexScheme = 0x00,
                                selectedEcdhProfile = 0x00,
                                grantedKeys = 0x00
                            };
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case(2):
                    Console.WriteLine(".2- PC Controller (1) To send Network key verify encrypted with a wrong key");
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.NetworkKeyVerify_S2Access;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameIsEncrypted.Value = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.IsSet = true;
                    _MainVMPrimary.SecuritySettingsViewModel.SecuritySettings.TestFrameNetworkKey.Value = new byte[16];
                    SetS2TestFrame(_MainVMPrimary);
                    ApplySecuritySettings(_MainVMPrimary);
                    break;
                case(3):
                    Console.WriteLine(".3- DUT grants 0 network keys to PC Controller (1)");
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.IsTestSchemaS2Enabled = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.ActiveTestFrameIndex = SecurityS2TestFrames.KEXSet;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.IsSet = true;
                    _MainVMSecondary.SecuritySettingsViewModel.SecuritySettings.TestFrameCommand.Value = new COMMAND_CLASS_SECURITY_2.KEX_SET()
                    {
                        selectedKexScheme = 02,
                        selectedEcdhProfile = 01,
                        grantedKeys = 0x00
                    };
                    SetS2TestFrame(_MainVMSecondary);
                    ApplySecuritySettings(_MainVMSecondary);
                    
                    break;
            }
            Console.WriteLine("2.- Include PC Controller (1) to DUT, but setting for each attempt:");
            AddNode(_MainVMSecondary, _MainVMPrimary);
            //In 2.1 verify that DUT aborts secure inclusion [009F.01.00.11.063]
            //In 2.2 verify that DUT aborts secure inclusion 
            //In 2.3 verify that DUT reports inclusion as non-secure and successful [009F.01.00.11.098]
            Assert_Slave_IsNonS2Secured(_MainVMSecondary, B_JoinNodeId);
        }


    }

}