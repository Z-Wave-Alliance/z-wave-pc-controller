/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System;
using System.Collections.Specialized;
using Utils.UI;
using Utils.UI.Bind;
using Utils.UI.Wrappers;
using ZWave.Devices;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class PollingViewModel : VMBase, IPollingModel
    {
        #region Properties
        private ISubscribeCollection<IPollDeviceInfo> _nodes;
        public ISubscribeCollection<IPollDeviceInfo> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                Notify("Nodes");
            }
        }

        private bool _isTestReady = true;
        public bool IsTestReady
        {
            get { return _isTestReady; }
            set
            {
                _isTestReady = value;
                Notify("IsTestReady");
            }
        }

        private TimeSpan _duration = TimeSpan.FromMinutes(2);
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                Notify("Duration");
            }
        }

        private bool _useResetSpan = false;
        public bool UseResetSpan
        {
            get { return _useResetSpan; }
            set
            {
                _useResetSpan = value;
                Notify("UseResetSpan");
            }
        }

        private int _resetSpanMode = 0;
        public int ResetSpanMode
        {
            get { return _resetSpanMode; }
            set
            {
                _resetSpanMode = value;
                Notify("ResetSpanMode");
            }
        }

        private bool _useBasicCC = false;
        public bool UseBasicCC
        {
            get { return _useBasicCC; }
            set
            {
                _useBasicCC = value;
                Notify("UseBasicCC");
            }
        }

        private int _counter;
        public int Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                Notify("Counter");
            }
        }
        public object NodesOperationsLockObject { get; } = new object();

        #endregion

        #region Commands

        public StartPollingCommand StartPollingCommand => CommandsFactory.CommandControllerSessionGet<StartPollingCommand>();
        public StopPollingCommand StopPollingCommand => CommandsFactory.CommandControllerSessionGet<StopPollingCommand>();

        #endregion

        public PollingViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            ApplicationModel.NodesChanged += new NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            Title = "Polling";

            Nodes = ApplicationModel.SubscribeCollectionFactory.Create<IPollDeviceInfo>();
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0 && e.NewItems[0] is SelectableItem<NodeTag>)
                    {
                        var di = (e.NewItems[0] as SelectableItem<NodeTag>).Item;
                        if (di.Id != ApplicationModel.Controller.Id)
                        {
                            lock (NodesOperationsLockObject)
                            {
                                Nodes.Add(new PollDeviceInfo(di));
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0 && e.OldItems[0] is SelectableItem<NodeTag>)
                    {
                        var di = (e.OldItems[0] as SelectableItem<NodeTag>).Item;
                        lock (NodesOperationsLockObject)
                        {
                            int index = -1;
                            for (int i = 0; i < Nodes.Count; i++)
                            {
                                if (Nodes[i].Device.Id == di.Id)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index > -1)
                                Nodes.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    lock (NodesOperationsLockObject)
                    {
                        Nodes.Clear();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public class PollDeviceInfo : EntityBase, IPollDeviceInfo
    {
        public PollDeviceInfo(NodeTag deviceInfo)
        {
            Device = deviceInfo;
            LastPollTime = DateTime.MinValue;
        }

        private bool _isPollEnabled;
        public bool IsPollEnabled
        {
            get { return _isPollEnabled; }
            set
            {
                _isPollEnabled = value;
                Notify("IsPollEnabled");
            }
        }

        private NodeTag _device;
        public NodeTag Device
        {
            get { return _device; }
            set
            {
                _device = value;
                Notify("Device");
            }
        }

        private DateTime _lastPollTime;
        public DateTime LastPollTime
        {
            get { return _lastPollTime; }
            set
            {
                _lastPollTime = value;
                Notify("LastPollTime");
            }
        }

        private int _pollTime;
        public int PollTime
        {
            get { return _pollTime; }
            set
            {
                _pollTime = value;
                Notify("PollTime");
            }
        }

        private int _reportTime = 1;
        public int ReportTime
        {
            get { return _reportTime; }
            set
            {
                _reportTime = value;
                Notify("ReportTime");
            }
        }

        private int _requests;
        public int Requests
        {
            get { return _requests; }
            set
            {
                _requests = value;
                Notify("Requests");
            }
        }

        private int _failures;
        public int Failures
        {
            get { return _failures; }
            set
            {
                _failures = value;
                Notify("Failures");
            }
        }

        private int _missingReports;
        public int MissingReports
        {
            get { return _missingReports; }
            set
            {
                _missingReports = value;
                Notify("MissingReports");
            }
        }

        private int _maxCommandTime;
        public int MaxCommandTime
        {
            get { return _maxCommandTime; }
            set
            {
                _maxCommandTime = value;
                Notify("MaxCommandTime");
            }
        }

        private int _totalCommandTime;
        public int TotalCommandTime
        {
            get { return _totalCommandTime; }
            set
            {
                _totalCommandTime = value;
                Notify("TotalCommandTime");
            }
        }

        private int _avgCommandTime;
        public int AvgCommandTime
        {
            get { return _avgCommandTime; }
            set
            {
                _avgCommandTime = value;
                Notify("AvgCommandTime");
            }
        }

        public void Reset()
        {
            MaxCommandTime = 0;
            AvgCommandTime = 0;
            TotalCommandTime = 0;
            MissingReports = 0;
            Failures = 0;
            Requests = 0;
        }
    }
}