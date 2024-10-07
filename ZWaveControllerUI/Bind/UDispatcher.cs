/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows.Threading;
using Utils.UI;
using ZWave.Exceptions;

namespace ZWaveControllerUI.Bind
{
    public class UDispatcher : IDispatch
    {
        public Dispatcher Dispatcher { get; set; }
        public UDispatcher(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }
        #region IDispatch Members

        public void BeginInvoke(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        public void Invoke(Action action)
        {
            if (CheckAccess())
            {
                action();
            }
            else
            {
                try
                {
                    Dispatcher.Invoke(action);
                }
                catch (Exception)
                { }
            }
        }

        public bool InvokeBackground(Action action)
        {
            return InvokeBackground(action, 0);
        }

        public bool InvokeBackground(Action action, int timeoutMs)
        {
            //Dispatcher.Invoke(action);
            DispatcherOperation dop = Dispatcher.BeginInvoke(action, DispatcherPriority.Background);
            if (timeoutMs > 0)
            {
                dop.Wait(TimeSpan.FromMilliseconds(timeoutMs));
            }
            else
            {
                dop.Wait();
            }
            if (dop.Status == DispatcherOperationStatus.Pending || dop.Status == DispatcherOperationStatus.Executing)
                dop.Abort();
            if (dop.Status == DispatcherOperationStatus.Completed)
                return true;
            else if (dop.Status == DispatcherOperationStatus.Aborted)
                return false;
            else
                OperationException.Throw("Dispatcher operation status:" + dop.Status);
            return false;
        }

        public bool CheckAccess()
        {
            return Dispatcher.CheckAccess();
        }

        #endregion
    }
}
