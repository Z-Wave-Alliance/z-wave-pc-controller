/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using NUnit.Framework;
using Utils;
using ZWave.BasicApplication.Operations;
using ZWave.BasicApplication;
using ZWave.BasicApplication.TransportService.Operations;
using ZWave;
using ZWave.Security;

namespace ZWaveControllerTests
{
    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            int TIMEOUT = 777;

            SetLearnModeS0Operation.CMD_TIMEOUT = TIMEOUT;

            InclusionS2TimeoutConstants.Joining.SetTestTimeouts(TIMEOUT);

            InclusionS2TimeoutConstants.Including.SetTestTimeouts(TIMEOUT);

            SendDataSecureTask.NONCE_REQUEST_TIMER = TIMEOUT;
            SendDataSecureTask.NONCE_REQUEST_INCLUSION_TIMER = TIMEOUT;

            SendDataSecureS2Task.NONCE_REQUEST_TIMER = TIMEOUT;
            SendDataSecureS2Task.NONCE_REQUEST_INCLUSION_TIMER = TIMEOUT;

            //RequestNodeInfoSecureTask.CMD_SUPPORTED = TIMEOUT;
            RequestNodeInfoSecureTask.START_DELAY = 15;

            CallbackApiOperation.RET_TIMEOUT = TIMEOUT;
            CallbackApiOperation.CALLBACK_TIMEOUT = TIMEOUT;

            RequestApiOperation.RET_TIMEOUT = TIMEOUT;

            ActionToken.DefaultTimeout = 7777;
            ActionToken.ThrowExceptionOnDefaultTimeoutExpired = true;
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
        }
    }
}
