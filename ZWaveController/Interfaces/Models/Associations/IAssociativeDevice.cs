/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IAssociativeDevice
    {
        NodeTag Device { get; set; }
        List<IAssociativeGroup> Groups { get; set; }
        IAssociativeApplication ParentApplication { get; set; }
        void UpdateGroup(byte groupId, byte maxNodesSupported, IList<NodeTag> nodeIds);
        void SetGroups(byte groupCount);
        NodeTag[] GetNodeIds();
    }
}