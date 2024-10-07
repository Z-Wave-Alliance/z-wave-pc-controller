/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI;
using Utils.UI.Bind;
using Utils.UI.Interfaces;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class MainVMBase : VMBase
    {
        #region Properties For Unit Tests

        //private readonly ManualResetEvent _readySignal = new ManualResetEvent(false);
        //public ManualResetEvent ReadySignal
        //{
        //    get { return _readySignal; }
        //}

        #endregion

        //public Func<string, LogLevels, LogIndents, LogIndents, bool> ConfirmationCallback { private get; set; }
        public Action<string, int, int> ProgressCallback { private get; set; }

        public ISubscribeCollectionFactory SubscribeCollectionFactory { get; set; }
        public IDispatch Dispatcher { get; set; }

        //public event EventDelegate<EventArgs<bool>> BusySet;

        private string _connectionName;
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                Notify("ConnectionName");
            }
        }

        private CommandBase _activeCommand;
        public CommandBase ActiveCommand
        {
            get { return _activeCommand; }
            set
            {
                _activeCommand = value;
                Notify("ActiveCommand");
            }
        }

        public FolderBrowserDialogViewModel FolderBrowserDialogViewModel { get; set; }

        public MainVMBase(ISubscribeCollectionFactory subscribeCollectionFactory, IDispatch dispatcher) : base(null)
        {
            SubscribeCollectionFactory = subscribeCollectionFactory;
            Dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            if (Dispatcher != null)
            {
                Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        //public bool Confirmation(string text)
        //{
        //    if (ConfirmationCallback != null)
        //        return ConfirmationCallback(text, LogLevels.Text, LogIndents.None, LogIndents.None);
        //    return false;
        //}

        //public virtual void LogTitle(string text)
        //{
        //    Log(text, LogLevels.Title, LogIndents.None, LogIndents.None);
        //}

        //public virtual void LogFail(string text)
        //{
        //    Log(text, LogLevels.Fail, LogIndents.None, LogIndents.None);
        //}

        //public virtual void LogWarning(string text)
        //{
        //    Log(text, LogLevels.Warning, LogIndents.None, LogIndents.None);
        //}

        //public virtual void LogOk(string text)
        //{
        //    Log(text, LogLevels.Ok, LogIndents.None, LogIndents.None);
        //}

        //public virtual void Log(string text)
        //{
        //    LogVM.Log(text, LogLevels.Text, LogIndents.Current, LogIndents.Current);
        //}

        //public virtual void Log(string text, LogLevels level, LogIndents indentBefore, LogIndents indentAfter)
        //{
        //    LogVM.Log(text, level, indentBefore, indentAfter);
        //}

        //public void Log(string message, LogLevels level)
        //{
        //    LogVM.Log(message, level, LogIndents.Current, LogIndents.Current);
        //}

        //public void AnnounceProgress(string text, int current, int total)
        //{
        //    if (ProgressCallback != null)
        //        ProgressCallback(text, current, total);
        //}
    }
}
