/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWave.Devices;
using ZWave.Enums;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveController.Services
{
    public class WakeUpNodesService : EntityBase, IWakeUpNodesService
    {
        private Thread _worker;
        private Task _timerTickTask;
        private readonly object _lockObject = new object();
        private AutoResetEvent _waitSignal = new AutoResetEvent(false);
        private AutoResetEvent _rxSignal = new AutoResetEvent(false);
        private AutoResetEvent _wakeupCheckSignal = new AutoResetEvent(false);
        private const int WAKEUP_CHECK_INTERVAL_MS = 3000;

        private Queue<Tuple<NodeTag, SecuritySchemes, ReceiveStatuses>> _innerQueue = new Queue<Tuple<NodeTag, SecuritySchemes, ReceiveStatuses>>();
        private bool _isRunning;
        private bool _isTimerRunning;
        private Action<NodeTag, SecuritySchemes> _wakeUpNoMoreCallback;
        private Action<NodeTag, SecuritySchemes> _wakeupIntervalSetCallback;
        private Func<QueueItem, ISelectableItem<QueueItem>> _createSelectableItem;
        private IDispatch _dispatcher;
        private ObservableCollection<ISelectableItem<QueueItem>> _queueItems;
        private Action<string, NodeTag> _logWarningCallback;
        private Action<string, NodeTag> _logOkCallback;
        public bool IsWakeupDelayed { get; set; }
        public ILogService Logger { get; set; }
        public ConcurrentDictionary<NodeTag, WakeUpMonitorContainer> WakeUpNodeHealthStatuses { get; set; } = new ConcurrentDictionary<NodeTag, WakeUpMonitorContainer>();


        public WakeUpNodesService(Action<NodeTag, SecuritySchemes> wakeUpNoMoreCallback, Action<NodeTag, SecuritySchemes> wakeupIntervalSetCallback, IDispatch dispatcher,
            ObservableCollection<ISelectableItem<QueueItem>> commandQueue, Func<QueueItem, ISelectableItem<QueueItem>> createSelectableItem, Action<string, NodeTag> logWarningCallback, Action<string, NodeTag> logOkCallback)
        {
            _wakeUpNoMoreCallback = wakeUpNoMoreCallback;
            _wakeupIntervalSetCallback = wakeupIntervalSetCallback;
            _createSelectableItem = createSelectableItem;
            _dispatcher = dispatcher;
            _innerQueue = new Queue<Tuple<NodeTag, SecuritySchemes, ReceiveStatuses>>();
            _queueItems = commandQueue;
            _logWarningCallback = logWarningCallback;
            _logOkCallback = logOkCallback;
        }

        public void Start()
        {
            lock (_lockObject)
            {
                _isRunning = true;
            }
            if (_worker == null)
            {
                _worker = new Thread(DoWork);
                _worker.IsBackground = true;
                _worker.Start();
            }
            if (_timerTickTask == null)
            {
                _isTimerRunning = true;
                _timerTickTask = Task.Run(() => PollingTimerTick());
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                _isRunning = false;
            }
            _waitSignal.Set();
            if (_worker != null)
            {
                _worker.Join();
                _worker = null;
            }
            if (_timerTickTask != null)
            {
                _isTimerRunning = false;
                WakeUpNodeHealthStatuses.Clear();
                _wakeupCheckSignal.Set();
                _timerTickTask.Wait();
                _timerTickTask = null;
            }
        }

        private void PollingTimerTick()
        {
            while (_isTimerRunning)
            {
                foreach (var wakeUpNodeHealthStatus in WakeUpNodeHealthStatuses)
                {
                    // Last received time + wakeup interval + 2 seconds (just to be sure)
                    if (DateTime.Now > wakeUpNodeHealthStatus.Value.LastReceivedTimestamp.AddSeconds(wakeUpNodeHealthStatus.Value.WakeUpInterval).AddMilliseconds(WAKEUP_CHECK_INTERVAL_MS - 1000))
                    {
                        // Notify user that node seems dead
                        if (wakeUpNodeHealthStatus.Value.IsAlive)
                        {
                            wakeUpNodeHealthStatus.Value.IsAlive = false;
                            _logWarningCallback($"WakeUp node with Id:{wakeUpNodeHealthStatus.Key} did not send WakeUp Notification. Check if it is properly working.", wakeUpNodeHealthStatus.Key);
                        }
                    }
                    else
                    {
                        // Recover dead node
                        if (!wakeUpNodeHealthStatus.Value.IsAlive)
                        {
                            wakeUpNodeHealthStatus.Value.IsAlive = true;
                            _logOkCallback($"WakeUp node with Id:{wakeUpNodeHealthStatus.Key} is back online.", wakeUpNodeHealthStatus.Key);
                        }
                    }
                }
                _wakeupCheckSignal.WaitOne(WAKEUP_CHECK_INTERVAL_MS);
            }
        }

        public void Enqueue(NodeTag node, CommandBase command, int delayMs)
        {
            lock (_lockObject)
            {
                _dispatcher.Invoke(() => _queueItems.Add(_createSelectableItem(new QueueItem(node, command, delayMs))));
            }
        }

        public void WakeUp(NodeTag node, SecuritySchemes securityScheme, ReceiveStatuses receiveStatus)
        {
            lock (_lockObject)
            {
                _innerQueue.Enqueue(new Tuple<NodeTag, SecuritySchemes, ReceiveStatuses>(node, securityScheme, receiveStatus));
            }
            _waitSignal.Set();

            if (WakeUpNodeHealthStatuses.ContainsKey(node))
            {
                WakeUpNodeHealthStatuses[node].LastReceivedTimestamp = DateTime.Now;
            }
            else
            {
                WakeUpNodeHealthStatuses.TryAdd(node, new WakeUpMonitorContainer());
            }
        }

        private NodeTag rxNode = NodeTag.Empty;
        public void RxSet(NodeTag node)
        {
            if (node == rxNode)
            {
                " set {0}"._DLOG(node.Id);
                _rxSignal.Set();
            }
        }

        public void RxWait(NodeTag node, int timeoutMs)
        {
            " wait {0} for {1}ms"._DLOG(node.Id, timeoutMs);
            _rxSignal.WaitOne(timeoutMs);
            rxNode = NodeTag.Empty;
        }

        public void RxReset(NodeTag node)
        {
            " reset {0}"._DLOG(node.Id);
            rxNode = node;
        }

        private void DoWork()
        {
            while (_isRunning)
            {
                if (_innerQueue.Count == 0)
                {
                    _waitSignal.WaitOne();
                }
                if (_isRunning && !IsWakeupDelayed)
                {
                    NodeTag node = NodeTag.Empty;
                    SecuritySchemes scheme = SecuritySchemes.NONE;
                    ReceiveStatuses receiveStatus = 0;
                    lock (_lockObject)
                    {
                        if (_innerQueue.Count > 0)
                        {
                            var v = _innerQueue.Dequeue();
                            node = v.Item1;
                            scheme = v.Item2;
                            receiveStatus = v.Item3;
                        }
                    }
                    if (node.Id > 0)
                    {
                        // CC:0084.01.07.12.002
                        if (receiveStatus.HasFlag(ReceiveStatuses.TypeBroad) || receiveStatus.HasFlag(ReceiveStatuses.TypeMulti))
                        {
                            _wakeupIntervalSetCallback(node, scheme);
                        }
                        else
                        {
                            //if (!IsWakeupDelayed)
                            {
                                ISelectableItem<QueueItem> qItem;
                                do
                                {
                                    lock (_lockObject)
                                    {
                                        qItem = _queueItems.FirstOrDefault(x => x.Item.Node == node);
                                        if (qItem != null)
                                        {
                                            _dispatcher.Invoke(() => _queueItems.Remove(qItem));
                                        }
                                    }

                                    if (qItem != null)
                                    {
                                        RxReset(qItem.Item.Node);
                                        CommandsFactory.CommandRunner.Execute(qItem.Item.Command, null, true);
                                        if (qItem.Item.DelayMs > 0)
                                        {
                                            RxWait(qItem.Item.Node, qItem.Item.DelayMs);
                                        }
                                    }
                                } while (qItem != null);
                            }
                            _wakeUpNoMoreCallback(node, scheme);
                        }
                    }
                }
            }
        }
    }

    public class QueueItem : EntityBase
    {
        private NodeTag _node;
        public NodeTag Node
        {
            get { return _node; }
            set
            {
                _node = value;
                Notify("Node");
            }
        }

        private DateTime mTimestamp;
        public DateTime Timestamp
        {
            get { return mTimestamp; }
            set
            {
                mTimestamp = value;
                Notify("Timestamp");
            }
        }

        private int mDelayMs;
        public int DelayMs
        {
            get { return mDelayMs; }
            set
            {
                mDelayMs = value;
                Notify("DelayMs");
            }
        }

        private CommandBase mCommand;
        public CommandBase Command
        {
            get { return mCommand; }
            set
            {
                mCommand = value;
                Notify("Command");
            }
        }

        public QueueItem(NodeTag node, CommandBase command, int delayMs)
        {
            Node = node;
            Command = command;
            Timestamp = DateTime.Now;
            DelayMs = delayMs;
        }
    }

    public class WakeUpMonitorContainer
    {
        public const int DefaultWakeUpIntervalValue = 60 * 5;
        // in case of reboot PC Controller we start counter from reboot time
        public DateTime LastReceivedTimestamp { get; set; } = DateTime.Now;
        public int WakeUpInterval { get; set; } = DefaultWakeUpIntervalValue;
        public bool IsAlive { get; set; } = true;
    }
}
