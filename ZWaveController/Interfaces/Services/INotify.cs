/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;

namespace ZWaveController.Interfaces.Services
{
    public interface INotify
    {
        void NotifyAll(string notifyName, object data);
        void NotifyGroup<T>(string notifyName, T data);
        void NotifyGroup(string notifyName);
        void NotifyErrorToGroup(string error);
        Tuple<bool, object> ShowDialog(string name, string title, string descr, object dialogData, string connectionId);
    }
}
