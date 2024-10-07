/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface ISenderHistoryModel
    {
        IList<ISelectableItem<PayloadHistoryItem>> History { get; set; }
        ISelectableItem<PayloadHistoryItem> SelectedHistoryRecord { get; set; }
        ISelectableItem<PayloadHistoryItem> GetPayloadHistoryItem(PayloadItem payloadItem);
        void UpdatedFilters();

    }
}