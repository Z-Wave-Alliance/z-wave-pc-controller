/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Devices;

namespace ZWaveController.Interfaces
{
    public interface ISetupRouteModel
    {
        IRouteCollection SourceRouteCollection { get; set; }
        IRouteCollection DestinationRouteCollection { get; set; }
        bool UseAssignReturnRoute { get; set; }
        bool UseAssignSUCRetrunRoute { get; set; }
        bool UseAssignPriorityReturnRoute { get; set; }
        bool UseAssignPrioritySUCReturnRoute { get; set; }
        bool UsePriorityRoute { get; set; }
        NodeTag[] PriorityRoute { get; set; } 
        byte RouteSpeed { get; set; }
        bool IsSourceListEnabled { get; }
        bool IsDestListEnabled { get; set; }
        bool IsPriorityEnabled { get; }
        NodeTag Source { get; set; }
        NodeTag Destionation { get; set; }
    }
}
