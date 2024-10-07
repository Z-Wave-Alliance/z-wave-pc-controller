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
            ApplicationModel.Invoke(() => ApplicationModel.SetBusyMessage($"Waiting for Completed Action"));
            ApplicationModel.ActiveCommand = this;
            ExecuteInner(param);
            ApplicationModel.ActiveCommand = null;
            ApplicationModel.Invoke(() => ApplicationModel.SetBusy(false));
        }

        protected virtual void ExecuteInner(object param)
        {
        }

        protected override bool CanExecuteAction(object param)
        {
            return !ApplicationModel.IsBusy;
        }
    }
}
