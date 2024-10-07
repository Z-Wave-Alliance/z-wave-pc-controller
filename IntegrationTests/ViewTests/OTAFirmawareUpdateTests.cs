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
    [Ignore]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.None)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S0)]
    [TestFixture(ApiTypeUnderTest.Basic, SecurityUnderTest.S2)]
    //[TestFixture(ApiTypeUnderTest.Zip, SecurityUnderTest.S2)]
    class OTAFirmawareUpdateTests : TestCaseBase
    {
        public OTAFirmawareUpdateTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, true)
        {
        }
        
        [Test]
        public void OTAUpdate_TwoTries_SuccessAndCanceled()
        {
            AddNode(_MainVMPrimary);
            SetSelectedNode(_MainVMPrimary.Nodes[1].Item.Id);
            OTAGet();
            Assert.IsNotNull(_MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateCommandClassVersion);
            OTABrowseFile();
            OTAUpdate();
            StringAssert.Contains("successfully", _MainVMPrimary.FirmwareUpdateViewModel.UpdateResultStatus);
            if (Convert.ToInt16(_MainVMPrimary.FirmwareUpdateViewModel.FirmwareUpdateCommandClassVersion) > 3)
            {
                OTAActivate();
            }
            Delay(2000);
            OTAUpdate();
            StringAssert.Contains("successfully", _MainVMPrimary.FirmwareUpdateViewModel.UpdateResultStatus);
            Delay(2000);
            OTAUpdateCancel();
            StringAssert.DoesNotContain("successfully", _MainVMPrimary.FirmwareUpdateViewModel.UpdateResultStatus);
        }

    }
}
