/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class TopologyMapCommandBase : CommandBasicBase
    {
        public ITopologyMapModel TopologyMapModel
        {
            get; private set;
        }

        public TopologyMapCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            TopologyMapModel = ControllerSession.ApplicationModel.TopologyMapModel;
            UseBackgroundThread = true;
        }
    }
}