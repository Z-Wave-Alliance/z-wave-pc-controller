/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZWaveController.Models;

namespace IntegrationTests
{
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    class NVMBackupRestoreTests : TestCaseBase
    {
        public NVMBackupRestoreTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }
        
        [Test]
        public void NVM_AfterBackup_RestoresState()
        {
            if (_MainVMPrimary.MainMenuViewModel.ShowNVMBackupRestoreCommand.CanExecute(null))
            {
                //Arrange
                AddNode(_MainVMPrimary);
                int epxected = _MainVMPrimary.Nodes.Count();
                string expctedConfigName = _MainVMPrimary.ConfigurationItem.ItemFileName;
                //Act
                Backup();
                SetDefault(_MainVMPrimary);
                Restore();
                int actual = _MainVMPrimary.Nodes.Count();
                var actualCongiName = _MainVMPrimary.ConfigurationItem.ItemFileName;
                //Assert
                Assert.AreEqual(epxected, actual);
                Assert.AreEqual(expctedConfigName, actualCongiName);
                AssertConfigurationItem_NodesListUpdated_NodesPresents(_MainVMPrimary);
            }
            else
                Console.WriteLine("Cannot execute. Not supported by ZIP Controller");
        }

    }
}
