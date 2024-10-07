/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    class ConfigurationItemTests : TestCaseBase
    {
        public ConfigurationItemTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        { }

        [Test]
        public void Create_ConfigurationItemFile_OnAppStart()
        {
            string ciFileName = _MainVMPrimary.ConfigurationItem.ItemFileName;
            CloseConnection(_MainVMPrimary);
            File.Delete(ciFileName);
            InitConnection(_MainVMPrimary);
            Assert.IsTrue(File.Exists(ciFileName), "File wasn't recreated");
        }

        [Test]
        public void OpenUpdate_SetDafualtController_PreviousCIDeleted()
        {
            //All check are included in SetDafault testmethod
            SetDefault(_MainVMPrimary);
        }

        [Test]
        public void Open_ApplySettings_SettingVMPressOk()
        {
            string ciFileName = _MainVMPrimary.ConfigurationItem.ItemFileName;
            File.Delete(ciFileName);
            ApplySettings(_MainVMPrimary);
            Assert.IsTrue(File.Exists(ciFileName), "File wasn't recreated");
        }

        [Test]
        public void Open_ApplySecurity_SecuritySettingVMPressOk()
        {
            string ciFileName = _MainVMPrimary.ConfigurationItem.ItemFileName;
            File.Delete(ciFileName);
            ApplySecuritySettings(_MainVMPrimary);
            Assert.IsTrue(File.Exists(ciFileName), "File wasn't recreated");
        }
             
    }
}
