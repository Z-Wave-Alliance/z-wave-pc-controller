/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ControllerSessionCommandBase : CommandBase
    {
        public IControllerSession ControllerSession { get; private set; }
        public IApplicationModel ApplicationModel => ControllerSession?.ApplicationModel;
        public bool IsModelBusy { get; set; } = true;
        protected virtual bool IsExecuteOnEndDeviceAlloved { get; } = false;

        public ControllerSessionCommandBase(IControllerSession controllerSession) : base(null)
        {
            ControllerSession = controllerSession;
        }

        protected override sealed bool CanExecuteAction(object param)
        {
            return ControllerSession != null &&
                (ControllerSession.Controller != null || IsExecuteOnEndDeviceAlloved && ControllerSession.IsEndDeviceLibrary) &&
                ControllerSession.ApplicationModel != null &&
                CanExecuteUIDependent(param) &&
                CanExecuteModelDependent(param);
        }

        public virtual bool CanExecuteUIDependent(object param)
        {
            return true;
        }

        public virtual bool CanExecuteModelDependent(object param)
        {
            return true;
        }

        protected override sealed void ExecuteAction(object param)
        {
            var previousActiveCommand = ApplicationModel.ActiveCommand;
            if (!ApplicationModel.IsBusy)
            {
                ApplicationModel.Invoke(() => ApplicationModel.SetBusy(IsModelBusy));
            }
            ApplicationModel.ActiveCommand = this;
            ExecuteInner(param);
            ApplicationModel.ActiveCommand = previousActiveCommand;
            if (IsModelBusy)
            {
                ApplicationModel.Invoke(() => ApplicationModel.SetBusy(false));
            }
        }

        protected virtual void ExecuteInner(object param)
        {
        }

        /// <summary>
        /// Add log text, at current indent 
        /// </summary>
        /// <param name="text"></param>
        public void Log(string text)
        {
            ControllerSession.Logger.Log(text);
        }
    }
}
