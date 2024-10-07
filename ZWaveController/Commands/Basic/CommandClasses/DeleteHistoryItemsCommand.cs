/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class DeleteHistoryItemsCommand : CommandBasicBase
    {
        public DeleteHistoryItemsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return ApplicationModel.SenderHistoryModel.SelectedHistoryRecord != null;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.SenderHistoryService.Delete(ApplicationModel.SenderHistoryModel.SelectedHistoryRecord.Item.Id);
        }
    }
}
