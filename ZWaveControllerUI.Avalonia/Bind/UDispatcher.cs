// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Threading;
using Avalonia.Threading;
using Utils.UI;

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
            Dispatcher.Post(action);
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
            if (CheckAccess())
            {
                action();
                return true;
            }
            using (var completed = new ManualResetEventSlim(false))
            {
                Dispatcher.Post(() =>
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        completed.Set();
                    }
                }, DispatcherPriority.Background);
                return completed.Wait(timeoutMs > 0 ? timeoutMs : Timeout.Infinite);
            }
        }

        public bool CheckAccess()
        {
            return Dispatcher.CheckAccess();
        }

        #endregion

        /// <summary>
        /// Runs a view's handler of a model event on the UI thread. The models notify on whichever thread changed
        /// them (a command's, or the Z-Wave library's for SmartStart), while controls may only be touched from the
        /// UI thread; WPF bindings hid this, code-behind handlers must not rely on it.
        /// </summary>
        public static void RunOnUIThread(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
                action();
            else
                Dispatcher.UIThread.Post(action);
        }
    }
}
