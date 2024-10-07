/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI;

namespace IntegrationTests
{
    public class DispatchFake : IDispatch
    {
        public void BeginInvoke(Action action)
        {
            action();
        }

        public void Invoke(Action action)
        {
            action();
        }

        public bool InvokeBackground(Action action, int timeoutMs)
        {
            action();
            return true;
        }

        public bool InvokeBackground(Action action)
        {
            action();
            return true;
        }

        public bool CheckAccess()
        {
            return true;
        }
    }
}
