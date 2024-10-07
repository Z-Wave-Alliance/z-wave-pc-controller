/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;

namespace ZWaveController.Commands
{
    public class CommandBase
    {
        public event Action OnCompleted;
        public event Action OnCancelled;
        protected Func<object, bool> _canExecute;
        protected Action<object> _execute;
        protected Func<object, bool> _canCancel;
        protected Action<object> _cancel;

        public string Text { get; set; }
        public bool UseBackgroundThread { get; set; }
        public bool IsCancelling { get; set; }
        public List<CommandBase> InferiorCommands { get; set; } = new List<CommandBase>();

        public CommandBase() { }

        public CommandBase(Action<object> execute, Func<object, bool> canExecute = null) : this(execute, canExecute, null, null)
        {
        }

        public CommandBase(Action<object> execute, Func<object, bool> canExecute, Action<object> cancel, Func<object, bool> canCancel)
        {
            _execute = execute;
            _canExecute = canExecute;
            _cancel = cancel;
            _canCancel = canCancel;
        }


        public bool CanExecute(object param)
        {
            var canExecute = _canExecute ?? CanExecuteAction;
            return canExecute(param);
        }

        protected virtual bool CanExecuteAction(object param)
        {
            return true;
        }

        public void Execute(object param)
        {
            IsCancelling = false;
            var execute = _execute ?? ExecuteAction;
            execute(param);
            OnCompleted?.Invoke();
        }

        protected virtual void ExecuteAction(object param)
        {
        }

        public bool CanCancel(object param)
        {
            var canCancel = _canCancel ?? CanCancelAction;
            return canCancel(param);
        }

        protected virtual bool CanCancelAction(object param)
        {
            return false;
        }

        public void Cancel(object param)
        {
            IsCancelling = true;
            var cancel = _cancel ?? CancelAction;
            cancel(param);
            OnCancelled?.Invoke();
        }

        protected virtual void CancelAction(object param)
        {
        }

        public virtual bool IsSupportedAndReady()
        {
            return true;
        }
    }
}
