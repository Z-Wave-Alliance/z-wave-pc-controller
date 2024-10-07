/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface IAssociativeGroup
    {
        List<string> GroupCommandClasses { get; set; }
        IAssociativeGroupInfo GroupInfo { get; set; }
        string GroupName { get; set; }
        byte Id { get; set; }
        byte MaxNodesSupported { get; set; }
        IAssociativeNodeCollection Nodes { get; set; }
        IAssociativeDevice ParentDevice { get; set; }

        void Update(byte maxNodesSupported, IList<NodeTag> nodeIds);
        NodeTag[] GetNodeIds();
        void SetGroupInfo(string profile1, string profile2);
    }
}