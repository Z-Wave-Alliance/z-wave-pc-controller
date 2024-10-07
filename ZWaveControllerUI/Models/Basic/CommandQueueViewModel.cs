/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class CommandQueueViewModel : DialogVMBase
    {
        public CommandBase DeleteCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            for (int i = ApplicationModel.CommandQueueCollection.Count - 1; i >= 0; i--)
            {
                if (ApplicationModel.CommandQueueCollection[i].IsSelected)
                {
                    ApplicationModel.Invoke(() => ApplicationModel.CommandQueueCollection.RemoveAt(i));
                }
            }
        });
        public CommandBase ClearCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => ApplicationModel.Invoke(() => ApplicationModel.CommandQueueCollection.Clear()));

        public CommandQueueViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Commands Queue";
        }
    }
}
