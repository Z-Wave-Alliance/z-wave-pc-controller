/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Diagnostics;
using Utils;
using Utils.UI.Logging;
using ZWave;
using ZWaveController.Interfaces;

namespace ZWaveController.Models
{
    public class LogAction : IDisposable
    {
        public string Caption { get; set; }
        public string BusyText { get; set; }
        public Stopwatch Stopwatch { get; set; }
        public IControllerSession ControllerSession { get; set; }
        public ActionToken Token { get; set; }
        public ActionStates State { get; set; }
        public LogRawData LogRawData { get; set; }

        public LogAction(IControllerSession controllerSession, string caption, string busyText, ActionToken token, LogRawData logRawData = null)
        {
            ControllerSession = controllerSession;
            Caption = caption;
            BusyText = busyText;
            Token = token;
            ControllerSession.ApplicationModel.SetBusyMessage(BusyText);
            ControllerSession.Logger.LogTitle($"{Caption} started");
            Stopwatch = Stopwatch.StartNew();
            LogRawData = logRawData;
        }

        public void Dispose()
        {
            Stopwatch.Stop();
            var elapsed = Stopwatch.Elapsed;
            var state = State != ActionStates.None ? State : Token?.State;
            if (state != null && state is ActionStates)
            {
                switch ((ActionStates)state)
                {
                    case ActionStates.Failed:
                    case ActionStates.Expired:
                        ControllerSession.Logger.LogFail("{0} {1} in {2:hh\\:mm\\:ss\\.fff}".FormatStr(Caption, state.ToString().ToLower(), elapsed), LogRawData);
                        break;
                    case ActionStates.Completed:
                        ControllerSession.Logger.LogOk("{0} {1} in {2:hh\\:mm\\:ss\\.fff}".FormatStr(Caption, state.ToString().ToLower(), elapsed), LogRawData);
                        break;
                    default:
                        ControllerSession.Logger.LogWarning("{0} {1} in {2:hh\\:mm\\:ss\\.fff}".FormatStr(Caption, state.ToString().ToLower(), elapsed), LogRawData);
                        break;
                }
            }
            else
            {
                ControllerSession.Logger.LogWarning("{0} {1} in {2:hh\\:mm\\:ss\\.fff}".FormatStr(Caption, "completed", elapsed), LogRawData);
            }
        }
    }
}
