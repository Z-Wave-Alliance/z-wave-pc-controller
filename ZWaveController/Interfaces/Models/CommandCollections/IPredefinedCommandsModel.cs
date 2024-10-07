/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{

    /// <summary>
    /// TODO: REVIEW
    /// </summary>
    public interface IPredefinedCommandsModel
    {
        IPredefinedCommandGroup CreateFavoriteGroup(string name);
        IList<IPredefinedCommandGroup> PredefinedGroups { get; set; }
        IPredefinedCommandGroup SelectedGroup { get; set; }
        string TempGroupName { get; set; }
        ISelectableItem<PredefinedPayload> SelectedItem { get; set; }
        ISelectableItem<PredefinedPayload> GetItem(PayloadItem payloadItem);



    }
}
