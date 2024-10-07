/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NSubstitute;
using NUnit.Framework;
using ZWave.BasicApplication.Enums;
using ZWave.Enums;
using ZWaveController;
using ZWaveController.Enums;
using ZWaveController.Models;
using ZWaveControllerUI.Models;

namespace ZWaveControllerTests
{
    [TestFixture]
    public class TransmitSettingsTests : ControllerTestBase
    {
        [Test]
        public void TransmitSettings_SetPowerLevel_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (TransmitSettingsViewModel)ApplicationPrimary.TransmitSettingsModel;
            ApplicationPrimary.Controller.ChipType = ChipTypes.ZW070x;
            model.NormalTxPower = 15;
            model.Measured0dBmPower = 7;

            //Act.
            model.SetPowerLevelCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.NormalTxPower == 15);
            Assert.IsTrue(model.Measured0dBmPower == 7);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }

        [Test]
        public void TransmitSettings_SetRegion_Success()
        {
            //Arrange.
            CommandsFactory.CurrentSourceId = ApplicationPrimary.DataSource.SourceId;
            var model = (TransmitSettingsViewModel)ApplicationPrimary.TransmitSettingsModel;
            ApplicationPrimary.Controller.ChipType = ChipTypes.ZW070x;
            model.RfRegion = RfRegions.US;

            //Act.
            model.SetRfRegionCommand.Execute(null);

            //Assert.
            Assert.IsTrue(model.RfRegion == RfRegions.US);
            ApplicationPrimary.Received().NotifyControllerChanged(NotifyProperty.CommandSucceeded, Arg.Any<NotifyCommandData>());
        }
    }
}