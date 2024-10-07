/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Enums;
using Utils.UI.Interfaces;
using Utils.UI.Logging;

namespace ZWaveController.Services
{
    public class LogBaseService : ILogService
    {
        public ILogModel LogModel { get; set; }
        private string currentIndent = string.Empty;
        private int indentStep = 2;

        public LogBaseService(IAppLogModel applicationModel)
        {
            LogModel = applicationModel.LogDialog;
        }

        public void LogTitle(string text, LogRawData logRawData = null)
        {
            var logSettings = new LogSettings { Level = LogLevels.Title, IndentBefore = LogIndents.None, IndentAfter = LogIndents.None };
            Log(text, logSettings, logRawData);
        }

        public virtual void LogFail(string text, LogRawData logRawData = null)
        {
            var logSettings = new LogSettings { Level = LogLevels.Fail, IndentBefore = LogIndents.None, IndentAfter = LogIndents.None };
            Log(text, logSettings, logRawData);
        }

        public void LogWarning(string text, LogRawData logRawData = null)
        {
            var logSettings = new LogSettings { Level = LogLevels.Warning, IndentBefore = LogIndents.None, IndentAfter = LogIndents.None };
            Log(text, logSettings, logRawData);
        }

        public void LogOk(string text, LogRawData logRawData = null)
        {
            var logSettings = new LogSettings { Level = LogLevels.Ok, IndentBefore = LogIndents.None, IndentAfter = LogIndents.None };
            Log(text, logSettings, logRawData);
        }

        public void Log(string text)
        {
            Log(text, new LogSettings());
        }

        public void Log(string text, LogSettings logSettings, LogRawData logRawData = null)
        {
            Dyes dye = Dyes.Black;
            bool isBold = false;
            switch (logSettings.Level)
            {
                case LogLevels.Title:
                    isBold = true;
                    break;
                case LogLevels.Fail:
                case LogLevels.Error:
                    dye = Dyes.Red;
                    isBold = true;
                    break;
                case LogLevels.Done:
                case LogLevels.Ok:
                    dye = Dyes.Green;
                    isBold = true;
                    break;
                case LogLevels.Warning:
                    dye = Dyes.DarkOrange;
                    break;
            }

            SetIndent(logSettings.IndentBefore);
            lock (LogModel.Queue)
            {
                if (LogModel.Queue.Count > LogModel.QCapacity)
                {
                    LogModel.Queue.Dequeue();
                }

                var logPacket = CreatePacket($"{currentIndent}{text}", dye, isBold, logSettings.Level, logRawData);
                LogModel.Queue.Enqueue(logPacket);
            }
            SetIndent(logSettings.IndentAfter);
        }

        public virtual LogPacket CreatePacket(string text, Dyes dye, bool isBold, LogLevels level, LogRawData logRawData)
        {
            return new LogPacket(text, dye, isBold, level, logRawData);
        }

        private void SetIndent(LogIndents? indent)
        {
            switch (indent)
            {
                case LogIndents.None:
                    currentIndent = "";
                    break;
                case LogIndents.Increase:
                    currentIndent = currentIndent.PadRight(currentIndent.Length + indentStep, ' ');
                    break;
                case LogIndents.Decrease:
                    if (currentIndent.Length > indentStep)
                        currentIndent = currentIndent.Substring(0, currentIndent.Length - indentStep);
                    else
                        currentIndent = "";
                    break;
            }
        }
    }
}
