/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    /// <summary>
    /// Interface for Group of predefined commands <see cref="PredefinedPayload"/>
    /// </summary>
    public interface IPredefinedCommandGroup
    {
        /// <summary>
        /// The name of group.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Collection of Predefined Commands (<see cref="PredefinedPayload"/>).
        /// </summary>
        IList<ISelectableItem<PredefinedPayload>> Items { get; set; }

        /// <summary>
        /// Gets next Id from Max in Items
        /// </summary>
        /// <returns>Free Item Id for insertion in Items</returns>
        uint GetFreeFavoriteItemId();
    }
}