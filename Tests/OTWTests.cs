/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using System.Linq;
using Utils.UI.Enums;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class OTWTests : ControllerTestBase
    {
        [Test]
        public void OTW_Chip0x40_Unsupported() {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            ApplicationPrimary.Controller.ChipType = ChipTypes.ZW030x;
            var model = ((MainViewModel)ApplicationPrimary).MainMenuViewModel;

            //Act.
            model.FirmwareUpdateLocalCommand.Execute(null);

            //Assert.
            ApplicationPrimary.DidNotReceive().NotifyControllerChanged(NotifyProperty.CommandSucceeded);
            Assert.IsTrue(ApplicationPrimary.LogDialog.Queue.Any(i => i.LogLevel == LogLevels.Fail));
        }
    }
}
