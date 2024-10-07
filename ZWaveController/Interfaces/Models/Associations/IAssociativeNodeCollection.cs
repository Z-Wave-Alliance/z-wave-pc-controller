/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;

namespace ZWaveController.Interfaces
{
    public interface IAssociativeNodeCollection
    {
        List<IAssociativeNode> NodeIds { get; set; }
        IAssociativeGroup ParentGroup { get; set; }
        string Title { get; set; }
    }
}