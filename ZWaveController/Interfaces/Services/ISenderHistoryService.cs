/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using Utils.UI.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public interface ISenderHistoryService
    {
        void Load();
        void Save();
        IList<ISelectableItem<PayloadHistoryItem>> GetFilteredHistory(DateTime? date = null);
        void Add(PayloadItem itemToStore);
        void Add(byte[] data);
        void Delete(uint id);
        void Delete(List<uint> ids);
        void Clear();
    }
}