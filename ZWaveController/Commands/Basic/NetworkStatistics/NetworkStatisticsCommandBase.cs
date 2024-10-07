/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public abstract class NetworkStatisticsCommandBase : CommandBasicBase
    {
        public INetworkStatisticsModel NetworkStatisticsModel
        {
            get; private set;
        }

        public NetworkStatisticsCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            NetworkStatisticsModel = controllerSession.ApplicationModel.NetworkStatisticsModel;
            UseBackgroundThread = true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return base.CanExecuteModelDependent(param);
        }
    }
}
