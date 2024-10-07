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
    class SettingsTests : TestCaseBase
    {
        const string TC_HARD_NAME = "ZWaveControllerDump";
        const string TC_EXT = ".zwlf";

        public SettingsTests(ApiTypeUnderTest type, SecurityUnderTest SecurityUnderTestMain)
            : base(type, SecurityUnderTestMain, false)
        {
        }

        [Test]
        public void TraceCapture_TraceFileCreation()
        {
            string folder = Path.GetTempPath();
            string comPortName = _MainVMPrimary.DataSource.SourceName;

            string expFile = Path.Combine(folder, string.Concat(TC_HARD_NAME, "_", comPortName, TC_EXT));

            _MainVMPrimary.SettingsViewModel.TraceCaptureFolder = folder;
            _MainVMPrimary.SettingsViewModel.IsTraceCapturing = true;
            _MainVMPrimary.SettingsViewModel.IsTraceCaptureAutoSplit = false;
            _MainVMPrimary.WriteSettingsParameters();
            _MainVMPrimary.TraceCaptureOpen();

            Assert.IsTrue(File.Exists(expFile), "TraceCapture file doesn't exists");

            _MainVMPrimary.TraceCaptureClose();
            if (File.Exists(expFile))
            {
                File.Delete(expFile);
            }
        }

    }
}
