/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Net;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class UnsolicitedDestBaseCommand : ControllerSessionCommandBase
    {
        public UnsolicitedDestBaseCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            UseBackgroundThread = true;
        }

        protected void RestartUnsolicitedDestination(string address, ushort portNo)
        {
            ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.Failed;
            var zipControllerSession = (ZipControllerSession)ControllerSession;

            if (string.IsNullOrEmpty(address) ||
                !IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                zipControllerSession.LogError("Invalid IP address", address);
                return;
            }

            if (portNo == 0)
            {
                zipControllerSession.LogError("Invalid primary port number", portNo);
                return;
            }

            if (ApplicationModel.SelectLocalIpAddressDialog.SecondaryPort != 0)
            {
                ControllerSessionsContainer.Config.ControllerConfiguration.UnsolicitedPortNoSecondary = ApplicationModel.SelectLocalIpAddressDialog.SecondaryPort;
            }

            zipControllerSession.RestartUnsolicitedListeners(address, portNo);
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
            ApplicationModel.NotifyControllerChanged(NotifyProperty.UnsolicitedDestination, ApplicationModel.SelectLocalIpAddressDialog);
        }
    }
}
