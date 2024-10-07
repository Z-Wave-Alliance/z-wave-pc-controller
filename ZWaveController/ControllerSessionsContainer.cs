/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController
{
    public static class ControllerSessionsContainer
    {
        public static Dictionary<string, IControllerSession> ControllerSessions { get; set; } = new Dictionary<string, IControllerSession>();

        public static IConfig Config { get; set; }

        public static ControllerSessionCreator ControllerSessionCreator { get; set; } = new ControllerSessionCreator();

        public static bool Add(string sourceId, IControllerSession controllerSession)
        {
            if (ControllerSessions.ContainsKey(sourceId))
                return false;

            ControllerSessions.Add(sourceId, controllerSession);
            return true;
        }

        public static void Remove(string sourceId)
        {
            if (ControllerSessions.ContainsKey(sourceId))
            {
                ControllerSessions[sourceId].Disconnect();
                ControllerSessions.Remove(sourceId);
            }
        }
    }
}
