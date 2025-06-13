/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Utils.UI;
using Utils.UI.Bind;
using Utils.UI.Wrappers;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWaveController.Commands;
using ZWaveController.Models;
using ZWaveController.Interfaces;
using ZWaveController;
using Utils.UI.Interfaces;
using ZWaveController.Enums;

namespace ZWaveControllerUI.Models
{
    public class IMAFullNetworkViewModel : VMBase, IIMAFullNetworkModel
    {
        public const byte GREEN = 0x0A;
        public const byte YELLOW = 0x05;
        public const byte RED = 0x03;
        public const byte NA = 0x00;

        private Random _randomGen = new Random();

        public CommandBase ReloadCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.Failed;
            lock (_lockObject)
            {
                Items.Clear();
                for (int i = 1; i < 233; i++)
                {
                    var imaItem = new IMADeviceInfo(ApplicationModel, Layout, new NodeTag((byte)i)) { NHV = (byte)_randomGen.Next(250) };
                    imaItem.IMATestResults = ApplicationModel.SubscribeCollectionFactory.Create<IIMATestResult>();
                    Items.Add(imaItem);
                }
                ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
            }
        });
        public CommandBase ResetLayoutCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => Layout.Clear());
        public CommandBase AddLinesCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            lock (_lockObject)
            {
                var deviceInfo = Items.FirstOrDefault((x) => x.Id == ApplicationModel.Controller.Network.NodeTag) as IMADeviceInfo;
                if (deviceInfo != null)
                {
                    deviceInfo.StartLines.Clear();
                    deviceInfo.EndLines.Clear();
                    for (int i = Items.Count - 1; i >= 0; i--)
                    {
                        if (Items[i].Type == IMAEntityTypes.Line)
                            Items.RemoveAt(i);
                    }
                    for (ushort i = 1; i < NetworkViewPoint.MAX_NODES; i++)
                    {
                        if (i != ApplicationModel.Controller.Id)
                        {
                            var deviceInfoNested = Items.FirstOrDefault((x) => x.Id == new NodeTag((ushort)i)) as IMADeviceInfo;
                            if (deviceInfoNested != null)
                            {
                                deviceInfoNested.StartLines.Clear();
                                deviceInfoNested.EndLines.Clear();
                                IMALine line = new IMALine(Layout, new NodeTag(ApplicationModel.Controller.Id), new NodeTag(i), deviceInfoNested.Id);
                                deviceInfo.StartLines.Add(line);
                                deviceInfoNested.EndLines.Add(line);
                                Items.Add(line);
                            }
                        }
                    }
                }
            }
        });
        public CommandBase SetBackgroundImageCommand => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            string fileName = string.Empty;
            ApplicationModel.OpenFileDialogModel.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            if (((IDialog)ApplicationModel.OpenFileDialogModel).ShowDialog() &&
                !string.IsNullOrEmpty(ApplicationModel.OpenFileDialogModel.FileName))
            {
                ApplicationModel.ConfigurationItem.ViewSettings.IMAView.NetworkBackgroundImagePath = ApplicationModel.OpenFileDialogModel.FileName;
                ApplicationModel.IMAFullNetworkModel.BackgroundImagePath = ApplicationModel.OpenFileDialogModel.FileName;
                ApplicationModel.IMAFullNetworkModel.UseBackgroundColor = false;
            }
        });
        public PingNodeCommand PingNodeCommand => CommandsFactory.CommandControllerSessionGet<PingNodeCommand>();
        public RequestNodeInfoIMACommand RequestNodeInfoCommand => CommandsFactory.CommandControllerSessionGet<RequestNodeInfoIMACommand>();
        public GetVersionCommand GetVersionCommand => CommandsFactory.CommandControllerSessionGet<GetVersionCommand>();
        public RediscoveryCommand RediscoveryCommand => CommandsFactory.CommandControllerSessionGet<RediscoveryCommand>();
        public PowerLevelTestCommand PowerLevelTestCommand => CommandsFactory.CommandControllerSessionGet<PowerLevelTestCommand>();
        public AreNeighborsCommand AreNeighborsCommand => CommandsFactory.CommandControllerSessionGet<AreNeighborsCommand>();
        public GetRoutingInfoCommand GetRoutingInfoCommand => CommandsFactory.CommandControllerSessionGet<GetRoutingInfoCommand>();
        public FullNetworkCommand FullNetworkCommand => CommandsFactory.CommandControllerSessionGet<FullNetworkCommand>();


        public byte ControllerNetworkHealth { get; set; }
        public NetworkCanvasLayout Layout { get; private set; }

        private NodeTag _sourceNode;
        public NodeTag SourceNode
        {
            get { return _sourceNode; }
            set
            {
                _sourceNode = value;
                Notify("SourceNode");
            }
        }

        private NodeTag _destinationNode;
        public NodeTag DestinationNode
        {
            get { return _destinationNode; }
            set
            {
                _destinationNode = value;
                Notify("DestinationNode");
            }
        }

        private string _backgroundImagePath;
        public string BackgroundImagePath
        {
            get { return _backgroundImagePath; }
            set
            {
                _backgroundImagePath = value;
                Notify("BackgroundImagePath");
            }
        }

        private byte[] _backgroundColor;
        public byte[] BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                ApplicationModel.ConfigurationItem.ViewSettings.IMAView.NetworkBackgroundColor = value;
                Notify("BackgroundColor");
            }
        }

        private bool _useBackgroundColor;
        public bool UseBackgroundColor
        {
            get { return _useBackgroundColor; }
            set
            {
                _useBackgroundColor = value;
                ApplicationModel.ConfigurationItem.ViewSettings.IMAView.UseNetworkBackgroundColor = value;
                Notify("UseBackgroundColor");
            }
        }

        private ISubscribeCollection<IIMAEntity> _items;
        public ISubscribeCollection<IIMAEntity> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                Notify("Items");
            }
        }

        private ISubscribeCollection<IIMAEntity> _selectedDevices;
        public ISubscribeCollection<IIMAEntity> SelectedDevices
        {
            get { return _selectedDevices; }
            set
            {
                _selectedDevices = value;
                Notify("SelectedDevices");
            }
        }

        private readonly object _lockObject = new object();
        public IMAFullNetworkViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            ApplicationModel.NodesChanged += new NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            Title = "IMA";
            Items = ApplicationModel.SubscribeCollectionFactory.Create<IIMAEntity>();
            SelectedDevices = ApplicationModel.SubscribeCollectionFactory.Create<IIMAEntity>();
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0 && e.NewItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag di = (e.NewItems[0] as SelectableItem<NodeTag>).Item;
                        if (di.EndPointId == 0)
                        {
                            lock (_lockObject)
                            {
                                IMADeviceInfo imaItem = new IMADeviceInfo(ApplicationModel, Layout, di);
                                if (ApplicationModel.Controller != null && di.Id == ApplicationModel.Controller.Id)
                                    imaItem.IsTestController = true;
                                imaItem.IMATestResults = ApplicationModel.SubscribeCollectionFactory.Create<IIMATestResult>();
                                Items.Add(imaItem);

                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0 && e.OldItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag di = (e.OldItems[0] as SelectableItem<NodeTag>).Item;
                        lock (_lockObject)
                        {
                            for (int i = Items.Count - 1; i >= 0; i--)
                            {
                                int index = -1;
                                if (Items[i] is IMADeviceInfo)
                                {
                                    IMADeviceInfo item = (IMADeviceInfo)Items[i];
                                    if (item.Device.Id == di.Id && item.Device.EndPointId == di.EndPointId)
                                    {
                                        index = i;
                                    }
                                }
                                else if (Items[i] is IMALine)
                                {
                                    IMALine item = (IMALine)Items[i];
                                    if ((item.FromId.Id == di.Id && di.EndPointId == 0) || (item.ToId.Id == di.Id && di.EndPointId == 0))
                                    {
                                        item.IsValid = false;
                                        index = i;
                                    }
                                }
                                if (index > -1)
                                {
                                    Items.RemoveAt(index);
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    lock (_lockObject)
                    {
                        Items.Clear();
                    }
                    break;
                default:
                    break;
            }
        }

        public bool HasSelectedItems
        {
            get
            {
                bool ret = false;
                lock (_lockObject)
                {
                    if (Items != null && Items.Count > 0)
                    {
                        foreach (var item in Items)
                        {
                            if (item is IMADeviceInfo && ((IMADeviceInfo)item).IsSelected)
                            {
                                ret = true;
                                break;
                            }
                        }
                    }
                }
                return ret;
            }
        }

        public List<IIMAEntity> GetItems(bool isIncludeNotSelected, bool isIncludeNonListeningNodes, bool isIncludeController)
        {
            List<IIMAEntity> ret = null;
            lock (_lockObject)
            {
                if (Items != null && Items.Count > 0)
                {
                    foreach (var item in Items)
                    {
                        if (item.Type == IMAEntityTypes.Item && item is IMADeviceInfo)
                        {
                            IMADeviceInfo idev = (IMADeviceInfo)item;

                            if (!isIncludeNotSelected && !idev.IsSelected)
                                continue;
                            if (!isIncludeController && idev.Id == ApplicationModel.Controller.Network.NodeTag)
                                continue;
                            if (!isIncludeNonListeningNodes && !ApplicationModel.Controller.Network.IsDeviceListening(idev.Device))
                                continue;
                            if (ApplicationModel.Controller.Network.IsVirtual(idev.Device))
                                continue;

                            if (ret == null)
                                ret = new List<IIMAEntity>();
                            ret.Add(idev);
                        }
                    }
                }
            }
            return ret;
        }

        public List<IIMAEntity> GetSelectedItems()
        {
            return GetItems(false, true, false);
        }

        public List<IIMAEntity> GetItems()
        {
            return GetItems(true, true, false);
        }

        public void ClearRoutingLines()
        {
            ApplicationModel.Invoke(() =>
            {
                for (int i = Items.Count - 1; i >= 0; i--)
                {
                    if (Items[i].Type == IMAEntityTypes.RoutingLine)
                        Items.RemoveAt(i);
                    else if (Items[i].Type == IMAEntityTypes.Item)
                    {
                        ((IMADeviceInfo)Items[i]).StartLines.RemoveAll(x => x.RouteId.Id == 0);
                        ((IMADeviceInfo)Items[i]).EndLines.RemoveAll(x => x.RouteId.Id == 0);
                    }
                }
            });
        }

        public void SetLayout(NetworkCanvasLayout networkCanvasLayout)
        {
            Layout = networkCanvasLayout;
        }

        public IIMALine CreateIMALine(NetworkCanvasLayout layout, NodeTag fromId, NodeTag told, NodeTag routeId)
        {
            return new IMALine(layout, fromId, told, routeId);
        }
    }

    public class IMAEntity : EntityBase, IIMAEntity
    {
        public NetworkCanvasLayout Layout { get; set; }
        public IMAEntity(NetworkCanvasLayout layout)
        {
            Layout = layout;
        }
        public virtual NodeTag Id
        {
            get;
            set;
        }
        public IMAEntityTypes Type { get; set; }

        private bool _isItemSelected;
        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set
            {
                _isItemSelected = value;
                Notify("IsItemSelected");
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                Notify("IsSelected");
            }
        }
    }

    public class IMALine : IMAEntity, IIMALine
    {
        #region Properties
        public NodeTag FromId { get; set; }
        public NodeTag ToId { get; set; }
        public NodeTag RouteId { get; set; }
        public IIMALine PreviousLine { get; set; }

        private bool _isValid;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                Notify("IsValid");
            }
        }

        public int X1
        {
            get
            {
                return (ushort)(Layout != null ? Layout.Get(FromId) : 0) + 20;
            }
        }

        public int X2
        {
            get
            {
                return (ushort)(Layout != null ? Layout.Get(ToId) : 0) + 20;
            }
        }

        public int Y1
        {
            get
            {
                return ((Layout != null ? Layout.Get(FromId) : 0) >> 16) + 20;
            }
        }

        public int Y2
        {
            get
            {
                return ((Layout != null ? Layout.Get(ToId) : 0) >> 16) + 20;
            }
        }

        public Point StartPoint
        {
            get
            {
                return new Point(X1, Y1);
            }
        }

        public Point Point
        {
            get
            {
                return new Point(X2, Y2);
            }
        }

        public Size Size
        {
            get
            {
                int delta = Math.Max(Math.Abs(X2 - X1), Math.Abs(Y2 - Y1));
                return new Size(delta * 4, delta * 4);
            }
        }
        #endregion

        public void UpdateFrom()
        {
            Notify("X1");
            Notify("Y1");
            Notify("StartPoint");
            Notify("Size");
        }

        public void UpdateTo()
        {
            Notify("X2");
            Notify("Y2");
            Notify("Point");
            Notify("Size");
        }

        public IMALine(NetworkCanvasLayout layout, NodeTag fromId, NodeTag toId, NodeTag routeId)
            : base(layout)
        {
            Type = IMAEntityTypes.Line;
            FromId = fromId;
            ToId = toId;
            IsValid = true;
            RouteId = routeId;
        }
    }

    public class IMARoutingLine : IMALine
    {
        public IMARoutingLine(NetworkCanvasLayout layout, NodeTag fromId, NodeTag toId)
            : base(layout, fromId, toId, NodeTag.Empty)
        {
            Type = IMAEntityTypes.RoutingLine;
        }
    }

    public class IMADeviceInfo : IMAEntity, IIMADeviceInfo
    {
        private IApplicationModel _applicationModel;
        private IIMAFullNetworkModel _imaFullNetworkModel;
        public List<IIMALine> StartLines { get; set; } = new List<IIMALine>();
        public List<IIMALine> EndLines { get; set; } = new List<IIMALine>();
        public IMADeviceInfo(IApplicationModel applicationModel, NetworkCanvasLayout layout, NodeTag deviceInfo)
            : base(layout)
        {
            _applicationModel = applicationModel;
            _imaFullNetworkModel = _applicationModel.IMAFullNetworkModel;
            Type = IMAEntityTypes.Item;
            Device = deviceInfo;
        }

        #region Properties
        public override NodeTag Id
        {
            get
            {
                return Device;
            }
        }

        public bool IsTestController { get; set; }

        public bool IsOk
        {
            get
            {
                return NHV > IMAFullNetworkViewModel.YELLOW && NHV <= IMAFullNetworkViewModel.GREEN;
            }
        }

        public bool IsWarning
        {
            get
            {
                return NHV > IMAFullNetworkViewModel.RED && NHV <= IMAFullNetworkViewModel.YELLOW;
            }
        }

        public bool IsError
        {
            get
            {
                return NHV > IMAFullNetworkViewModel.NA && NHV <= IMAFullNetworkViewModel.RED;
            }
        }

        public bool IsNA
        {
            get
            {
                return NHV == IMAFullNetworkViewModel.NA;
            }
        }

        public int TopLeft
        {
            get
            {
                return Layout != null ? Layout.Get(Id) : 0;
            }
            set
            {
                if (Layout != null)
                {
                    Layout.Set(Id, value);
                    UpdateLinesLayout();
                }
                Notify("TopLeft");
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

        private bool _isInProgress;
        public bool IsInProgress
        {
            get { return _isInProgress; }
            set
            {
                _isInProgress = value;
                Notify("IsInProgress");
            }
        }

        private int? _rc;
        public int? RC
        {
            get { return _rc; }
            private set
            {
                _rc = value;
                Notify("RC");
            }
        }

        private int? _per;
        public int? PER
        {
            get { return _per; }
            private set
            {
                _per = value;
                Notify("PER");
            }
        }

        private byte? _nb;
        public byte? NB
        {
            get { return _nb; }
            private set
            {
                _nb = value;
                Notify("NB");
            }
        }

        private bool? _lwrdb;
        public bool? LWRdB
        {
            get { return _lwrdb; }
            private set
            {
                _lwrdb = value;
                Notify("LWRdB");
            }
        }

        private sbyte? _lwrrssi;
        public sbyte? LWRRSSI
        {
            get { return _lwrrssi; }
            private set
            {
                _lwrrssi = value;
                Notify("LWRRSSI");
            }
        }

        private bool _isSelectedAsSource;
        public bool IsSelectedAsSource
        {
            get { return _isSelectedAsSource; }
            set
            {
                _isSelectedAsSource = value;
                UpdateLinesItemSelected(value);
                Notify("IsSelectedAsSource");
            }
        }

        private byte? _nhv;
        public byte? NHV
        {
            get { return _nhv; }
            set
            {
                _nhv = value;
                Notify("NHV");
                Notify("IsOk");
                Notify("IsWarning");
                Notify("IsError");
                Notify("IsNA");
            }
        }

        private ISubscribeCollection<IIMATestResult> _imaTestResults;
        public ISubscribeCollection<IIMATestResult> IMATestResults
        {
            get { return _imaTestResults; }
            set
            {
                _imaTestResults = value;
                Notify("IMATestResults");
            }
        }

        private NodeTag[] _neighbors;
        public NodeTag[] Neighbors
        {
            get { return _neighbors; }
            set
            {
                _neighbors = value;
                Notify("Neighbors");
            }
        }

        #endregion

        private void UpdateLinesLayout()
        {
            foreach (var item in StartLines)
            {
                if (item.IsValid)
                    item.UpdateFrom();
            }
            foreach (var item in EndLines)
            {
                if (item.IsValid)
                    item.UpdateTo();
            }
        }

        private void UpdateLinesItemSelected(bool value)
        {
            foreach (var item in EndLines)
            {
                IIMALine prevLine = item.PreviousLine;
                while (prevLine != null)
                {
                    if (prevLine.RouteId == Id)
                    {
                        prevLine.IsItemSelected = value;
                    }
                    prevLine = prevLine.PreviousLine;
                }
                item.IsItemSelected = value;
            }

            if (_imaFullNetworkModel != null && Neighbors != null)
            {
                if (value)
                {
                    _applicationModel.Invoke(() =>
                    {
                        foreach (var item in Neighbors)
                        {
                            IMADeviceInfo dest = (IMADeviceInfo)_imaFullNetworkModel.Items.FirstOrDefault(x => x.Id == item && x.Type == IMAEntityTypes.Item);
                            if (dest != null)
                            {
                                IMARoutingLine line = new IMARoutingLine(Layout, Id, item);
                                _imaFullNetworkModel.Items.Add(line);
                                this.StartLines.Add(line);
                                dest.EndLines.Add(line);
                            }
                        }
                    });
                }
                else
                {
                    _imaFullNetworkModel.ClearRoutingLines();
                }
            }
        }

        public void ClearIMATestResult()
        {
            IMATestResults.Clear();
            RC = null;
            PER = null;
            NB = null;
            LWRdB = null;
            LWRRSSI = null;
            NHV = null;
        }

        public void AddIMATestResult(IIMATestResult res)
        {
            IMATestResults.Add(res);
            // Update average values
            int n = IMATestResults.Count;
            if (res.RC != null)
                RC = n <= 1 ? res.RC : (((RC ?? 0) * (n - 1)) + res.RC) / n;
            PER = n <= 1 ? res.PER : (((PER ?? 0) * (n - 1)) + res.PER) / n;
            if (res.NB != null)
                NB = n <= 1 ? res.NB : (byte)((((NB ?? 0) * (n - 1)) + res.NB) / n);
            if (res.LWRdB != null)
                LWRdB = LWRdB ?? false | res.LWRdB;
            if (res.LWRRSSI != null)
                LWRRSSI = n <= 1 ? res.LWRRSSI : (sbyte)((((LWRRSSI ?? 0) * (n - 1)) + res.LWRRSSI) / n);
            NHV = n <= 1 ? res.NHV : (byte)((((NHV ?? 0) * (n - 1)) + res.NHV) / n);
        }

        public IIMATestResult CreateTestResult(int testRound)
        {
            return new IMATestResult(testRound);
        }

        public COMMAND_CLASS_POWERLEVEL.POWERLEVEL_TEST_NODE_REPORT PowerLevelReport { get; set; }
    }

    public class IMATestResult : EntityBase, IIMATestResult
    {
        public IMATestResult(int iterationNo)
        {
            IterationNo = iterationNo;
            RC = null;
            PER = 0;
            NB = null;
            LWRdB = null;
            LWRRSSI = null;
            NHV = 0;
        }

        private int _iterationNo;
        public int IterationNo
        {
            get { return _iterationNo; }
            set
            {
                _iterationNo = value;
                Notify("IterationNo");
            }
        }

        private int? _rc;
        public int? RC
        {
            get { return _rc; }
            set
            {
                _rc = value;
                Notify("RC");
            }
        }

        private int _per;
        public int PER
        {
            get { return _per; }
            set
            {
                _per = value;
                Notify("PER");
            }
        }

        private byte? _nb;
        public byte? NB
        {
            get { return _nb; }
            set
            {
                _nb = value;
                Notify("NB");
            }
        }

        private bool? _lwrdb;
        public bool? LWRdB
        {
            get { return _lwrdb; }
            set
            {
                _lwrdb = value;
                Notify("LWRdB");
            }
        }

        private sbyte? _lwrrssi;
        public sbyte? LWRRSSI
        {
            get { return _lwrrssi; }
            set
            {
                _lwrrssi = value;
                Notify("LWRRSSI");
            }
        }

        private byte _nhv;
        public byte NHV
        {
            get { return _nhv; }
            set
            {
                _nhv = value;
                Notify("NHV");
            }
        }
    }
}
