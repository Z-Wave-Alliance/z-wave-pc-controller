/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Utils.UI;
using ZWave.Devices;
using ZWaveController.Commands;
using System.Linq;
using ZWaveController;
using ZWaveController.Interfaces;
using Utils.UI.Wrappers;

namespace ZWaveControllerUI.Models
{
    public class ERTTViewModel : VMBase, IERTTModel
    {
        #region Properties
        public List<NodeTag> SelectedNodes { get; set; }

        private volatile bool _isTestReady = true;
        public bool IsTestReady
        {
            get { return _isTestReady; }
            set
            {
                _isTestReady = value;
                Notify("IsTestReady");
            }
        }

        private decimal _testIterations = 1;
        public decimal TestIterations
        {
            get { return _testIterations; }
            set
            {
                _testIterations = value;
                Notify("TestIterations");
            }
        }

        private bool _isRunForever;
        public bool IsRunForever
        {
            get { return _isRunForever; }
            set
            {
                _isRunForever = value;
                Notify("IsRunForever");
            }
        }

        private void SetBasicSetDefaultCheckedValues()
        {
            _isBasicSetValue0 = false;
            _isBasicSetValue255 = false;
            _isBasicSetValue0_255 = false;

            Notify("IsBasicSetValue0");
            Notify("IsBasicSetValue255");
            Notify("IsBasicSetValue0_255");
        }

        private bool _isBasicSetValue0 = true;
        public bool IsBasicSetValue0
        {
            get { return _isBasicSetValue0; }
            set
            {
                SetBasicSetDefaultCheckedValues();
                _isBasicSetValue0 = value;
                Notify("IsBasicSetValue0");
            }
        }

        private bool _isBasicSetValue255;
        public bool IsBasicSetValue255
        {
            get { return _isBasicSetValue255; }
            set
            {
                SetBasicSetDefaultCheckedValues();
                _isBasicSetValue255 = value;
                Notify("IsBasicSetValue255");
            }
        }

        private bool _isBasicSetValue0_255;
        public bool IsBasicSetValue0_255
        {
            get { return _isBasicSetValue0_255; }
            set
            {
                SetBasicSetDefaultCheckedValues();
                _isBasicSetValue0_255 = value;
                Notify("IsBasicSetValue0_255");
            }
        }

        private bool _isStopOnError;
        public bool IsStopOnError
        {
            get { return _isStopOnError; }
            set
            {
                _isStopOnError = value;
                Notify("IsStopOnError");
            }
        }

        private int _packetsSent;
        public int PacketsSent
        {
            get { return _packetsSent; }
            set
            {
                _packetsSent = value;
                Notify("PacketsSent");
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


        private int _packetsRecieved;
        public int PacketsRecieved
        {
            get { return _packetsRecieved; }
            set
            {
                _packetsRecieved = value;
                Notify("PacketsRecieved");
            }
        }

        private int _UARTErrors;
        public int UARTErrors
        {
            get { return _UARTErrors; }
            set
            {
                _UARTErrors = value;
                Notify("UARTErrors");
            }
        }

        private int _packetsWithRouteTriesSent;
        public int PacketsWithRouteTriesSent
        {
            get { return _packetsWithRouteTriesSent; }
            set
            {
                _packetsWithRouteTriesSent = value;
                Notify("PacketsWithRouteTriesSent");
            }
        }

        private bool _isRetransmission;
        public bool IsRetransmission
        {
            get { return _isRetransmission; }
            set
            {
                _isRetransmission = value;
                Notify("IsRetransmission");
            }
        }

        private bool _isCustomTxOptions;
        public bool IsCustomTxOptions
        {
            get { return _isCustomTxOptions; }
            set
            {
                _isCustomTxOptions = value;
                Notify("IsCustomTxOptions");
            }
        }

        private byte _customTxOptions;
        public byte CustomTxOptions
        {
            get { return _customTxOptions; }
            set
            {
                _customTxOptions = value;
                Notify("CustomTxOptions");
            }
        }

        private bool _isLowPower;
        public bool IsLowPower
        {
            get { return _isLowPower; }
            set
            {
                _isLowPower = value;
                Notify("IsLowPower");
            }
        }

        private int _txMode1Delay = 100;
        public int TxMode1Delay
        {
            get { return _txMode1Delay; }
            set
            {
                _txMode1Delay = value;
                Notify("TxMode1Delay");
            }
        }

        private bool _isTxControlledByModuleEnabled;
        public bool IsTxControlledByModuleEnabled
        {
            get { return _isTxControlledByModuleEnabled; }
            set
            {
                _isTxControlledByModuleEnabled = value;
                Notify("IsTxControlledByModuleEnabled");
                if (!value)
                    IsTxControlledByModule = false;
            }
        }

        private bool _isTxControlledByModule;
        public bool IsTxControlledByModule
        {
            get { return _isTxControlledByModule; }
            set
            {
                _isTxControlledByModule = value;
                Notify("IsTxControlledByModule");
            }
        }

        private int _txMode2Delay = 1;
        public int TxMode2Delay
        {
            get { return _txMode2Delay; }
            set
            {
                _txMode2Delay = value;
                Notify("TxMode2Delay");
            }
        }

        private int _payloadLength = 2;
        public int PayloadLength
        {
            get { return _payloadLength; }
            set
            {
                _payloadLength = value;
                Notify("PayloadLength");
            }
        }

        private List<string> _testCommands;

        private int _testCommandIndex = 2;

        public int TestCommandIndex
        {
            get { return _testCommandIndex; }
            set
            {
                _testCommandIndex = value;
                Notify("PayloadLength");
            }
        }
        public List<string> TestCommands
        {
            get { return _testCommands; }
            set
            {
                _testCommands = value;
                Notify("TestCommands");
            }
        }

        private IList<IResultItem> _resultItems;
        public IList<IResultItem> ResultItems
        {
            get { return _resultItems; }
            set
            {
                _resultItems = value;
                Notify("ResultItems");
            }
        }

        #endregion

        #region Commands

        public StartStopCommand StartStopCommand => CommandsFactory.CommandControllerSessionGet<StartStopCommand>();

        #endregion

        public object _lockObject { get; set; } = new object();
        public ERTTViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            SelectedNodes = new List<NodeTag>();
            ApplicationModel.NodesChanged += new NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            ApplicationModel.SelectedNodesChanged += new Action(MainVM_SelectedNodesChanged);
            Title = "ERTT";

            TestCommands = new List<string>();
            TestCommands.Add("sendData ");
            TestCommands.Add("sendDataMeta ");
            TestCommands.Add("sendData with Basic Set toggle ON/OFF between rounds ");
            TestCommands.Add("sendDataMeta with Basic Set toggle ON/OFF between rounds ");
            TestCommands.Add("sendData with one group result frame (serial) at every round end ");
            TestCommands.Add("sendDataMeta with one group result frame (serial) at every round end ");
            TestCommands.Add("sendData with Basic Set toggle ON/OFF and one group result frame (serial) between rounds ");
            TestCommands.Add("sendDataMeta with Basic Set toggle ON/OFF and one group result frame (serial) between rounds ");

            ResultItems = ApplicationModel.SubscribeCollectionFactory.Create<IResultItem>();
        }


        public void AddResultItem(NodeTag item)
        {
            ResultItems.Add(new ResultItem(item));
        }

        public void FillResultItems(NodeTag controller)
        {
            lock (_lockObject)
            {
                ApplicationModel.Invoke(() =>
                {
                    ResultItems.Clear();
                    foreach (var item in SelectedNodes)
                    {
                        if (item != controller)
                        {
                            AddResultItem(item);
                        }
                    }

                });
            }
        }

        void MainVM_SelectedNodesChanged()
        {
            lock (_lockObject)
            {
                SelectedNodes.Clear();
            }
            var addedNodes = ApplicationModel.ConfigurationItem.Nodes.ToArray();
            foreach (var item in addedNodes)
            {
                if (item.IsSelected && item.IsEnabled)
                {
                    lock (_lockObject)
                    {
                        SelectedNodes.Add(item.Item);
                    }
                }
            }
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0 && e.OldItems[0] is SelectableItem<NodeTag>)
                    {
                        var di = (e.OldItems[0] as SelectableItem<NodeTag>).Item;
                        lock (_lockObject)
                        {
                            int index = -1;
                            for (int i = 0; i < ResultItems.Count; i++)
                            {
                                if (ResultItems[i].Device.Id == di.Id)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index > -1)
                                ResultItems.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    lock (_lockObject)
                    {
                        Counter = 0;
                        PacketsSent = 0;
                        PacketsRecieved = 0;
                        UARTErrors = 0;
                        PacketsWithRouteTriesSent = 0;
                        ResultItems.Clear();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public class ResultItem : EntityBase, IResultItem
    {
        public ResultItem(NodeTag device)
        {
            Device = device;
        }

        #region Properties

        private NodeTag mDevice;
        public NodeTag Device
        {
            get { return mDevice; }
            set
            {
                mDevice = value;
                Notify("Device");
            }
        }

        private string mTransmitStatus;
        public string TransmitStatus
        {
            get { return mTransmitStatus; }
            set
            {
                mTransmitStatus = value;
                Notify("TransmitStatus");
            }
        }

        private int mErrorsCount;
        public int ErrorsCount
        {
            get { return mErrorsCount; }
            set
            {
                mErrorsCount = value;
                Notify("ErrorsCount");
            }
        }

        private int mElapsedMs;
        public int ElapsedMs
        {
            get { return mElapsedMs; }
            set
            {
                mElapsedMs = value;
                Notify("ElapsedMs");
            }
        }

        #endregion
    }
}
