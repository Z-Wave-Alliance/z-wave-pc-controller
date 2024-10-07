/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Bind;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IAssociationsModel
    {
        ISubscribeCollection<IAssociativeApplication> AssociativeApplications { get; set; }
        object ExpandObject { get; set; }
        bool IsAssignReturnRoutes { get; set; }
        IAssociativeDevice SelectedAssociativeDevice { get; set; }
        IAssociativeNode SelectedAssociativeNode { get; set; }
        IAssociativeGroup SelectedGroup { get; set; }
        List<NodeTag> SelectedNodeIds { get; set; }
        object SelectedObject { get; set; }

        object Clone();
        void UpdateAssociativeDevice(NodeTag nodeId);
    }
}