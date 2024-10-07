/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;

namespace ZWaveController.Interfaces
{
    public interface INodeInformationService
    {
        byte[] GetSelectedCommandClasses();
        byte[] GetDefaultCommandClasses();
    }
}
