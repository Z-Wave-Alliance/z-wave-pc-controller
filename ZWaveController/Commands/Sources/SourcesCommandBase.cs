/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using ZWaveController.Interfaces.Services;

namespace ZWaveController.Commands
{
    public class SourcesCommandBase : CommandBase
    {
        public IApplicationModel ApplicationModel { get; private set; }
        public ISourcesInfoService SourcesInfoService { get; set; }

        public SourcesCommandBase(IApplicationModel applicationModel)
        {
            ApplicationModel = applicationModel;
            UseBackgroundThread = true;
        }

        protected override sealed void ExecuteAction(object param)
        {
            ApplicationModel.Invoke(() => ApplicationModel.SetBusy(true));
            ApplicationModel.Invoke(() => ApplicationModel.SetBusyMessage(BusyMessage));
            ApplicationModel.ActiveCommand = this;
            try
            {
                ExecuteInner(param);
            }
            finally
            {
                ApplicationModel.ActiveCommand = null;
                ApplicationModel.Invoke(() => ApplicationModel.SetBusy(false));
            }
        }

        /// <summary>Message shown in the busy overlay while the command runs. Override to show a specific message.</summary>
        protected virtual string BusyMessage => "Waiting for Completed Action";

        protected virtual void ExecuteInner(object param)
        {
        }

        protected override bool CanExecuteAction(object param)
        {
            return !ApplicationModel.IsBusy;
        }
    }
}
