/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class RequestNodeInfoIMACommand : IMACommandBase
    {
        public RequestNodeInfoIMACommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Z-Wave Network Health. Request Node Information.";
            IsCancelAtController = true;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Getting Request Node Information.");
            var imaItems = ImaViewModel.GetSelectedItems();
            if (imaItems == null)
                imaItems = ImaViewModel.GetItems();
            if (imaItems != null)
            {
                foreach (IIMADeviceInfo item in imaItems)
                {
                    ControllerSession.RequestNodeInfo(item.Device, _token);
                    if (IsCancelling)
                        break;
                }
            }
        }
    }
}
