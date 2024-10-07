/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Bind;
using Utils.UI.Interfaces;
using ZWave.Devices;
using ZWave.Xml.Application;

namespace ZWaveController.Interfaces
{
    public interface IRouteCollection
    {
        ZWaveDefinition ZWaveDefinition { get; }
        ISubscribeCollection<ISelectableItem<NodeTag>> Nodes { get; set; }
        ISelectableItem<NodeTag> SelectedNode { get; set; }
    }
}
