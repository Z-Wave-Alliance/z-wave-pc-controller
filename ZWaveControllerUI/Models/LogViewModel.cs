/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Bind;
using Utils.UI.Interfaces;
using Utils.UI.Logging;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class LogViewModel : DialogVMBase, ILogModel
    {
        const int _qCapacity = 1000;
        public int QCapacity => _qCapacity;
        public Queue<LogPacket> Queue { get; set; }
        public ClearLogCommand ClearLogCommand => CommandsFactory.CommandControllerSessionGet<ClearLogCommand>();

        public LogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Log";
            DialogSettings.IsTopmost = false;
            DialogSettings.IsResizable = true;
            DialogSettings.Width = 600;
            DialogSettings.Height = 400;
            Queue = new Queue<LogPacket>(QCapacity);
        }

        private ISubscribeCollection<LogPacket> _logPackets;
        public ISubscribeCollection<LogPacket> LogPackets
        {
            get { return _logPackets ?? (_logPackets = ApplicationModel.SubscribeCollectionFactory.Create<LogPacket>()); }
            set
            {
                _logPackets = value;
                Notify("LogPackets");
            }
        }

        private LogPacket _lastLogPacket;
        public LogPacket LastLogPacket
        {
            get { return _lastLogPacket; }
            set
            {
                _lastLogPacket = value;
                Notify("LastLogPacket");
            }
        }

        private LogPacket _lastLogActionPacket;
        public LogPacket LastLogActionPacket
        {
            get { return _lastLogActionPacket; }
            set
            {
                _lastLogActionPacket = value;
                Notify("LastLogActionPacket");
            }
        }

        private bool _isAutoScrollLogMessages = true;
        public bool IsAutoScrollLogMessages
        {
            get { return _isAutoScrollLogMessages; }
            set
            {
                _isAutoScrollLogMessages = value;
                Notify("IsAutoScrollLogMessages");
            }
        }

        public void FeedStoredLogPackets()
        {
            while (Queue.Count > 0)
            {
                var itemRef = Queue.Dequeue();
                if (itemRef != null)
                {
                    while (LogPackets.Count > 1000)
                        LogPackets.RemoveAt(0);
                    LogPackets.Add(itemRef);
                }
            }
        }

        public void Clear()
        {
            LogPackets.Clear();
        }
    }
}
