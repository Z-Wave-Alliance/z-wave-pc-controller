/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Specialized;
using System.Linq;
using Utils.UI;
using Utils.UI.Bind;
using Utils.UI.Interfaces;
using Utils.UI.Wrappers;
using ZWave.Devices;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SetupRouteViewModel : VMBase, ISetupRouteModel
    {
        #region Commands

        public AssignReturnRouteCommand AssignReturnRouteCommand => CommandsFactory.CommandControllerSessionGet<AssignReturnRouteCommand>();
        public RemoveReturnRouteCommand RemoveReturnRouteCommand => CommandsFactory.CommandControllerSessionGet<RemoveReturnRouteCommand>();
        public GetPriorityRouteCommand GetPriorityRouteCommand => CommandsFactory.CommandControllerSessionGet<GetPriorityRouteCommand>();
        public SetPriorityRouteCommand SetPriorityRouteCommand => CommandsFactory.CommandControllerSessionGet<SetPriorityRouteCommand>();

        #endregion

        #region properties

        private IRouteCollection _sourceRouteCollection;
        public IRouteCollection SourceRouteCollection
        {
            get { return _sourceRouteCollection; }
            set
            {
                _sourceRouteCollection = value;
                Notify("SourceRouteCollection");
            }
        }
        private IRouteCollection _destinationRouteCollection;
        public IRouteCollection DestinationRouteCollection
        {
            get { return _destinationRouteCollection; }
            set
            {
                _destinationRouteCollection = value;
                Notify("SourceRouteCollection");
            }
        }

        private bool _useAssignReturnRoute = true;
        public bool UseAssignReturnRoute
        {
            get { return _useAssignReturnRoute; }
            set
            {
                ResetAllCheckBoxes();
                _useAssignReturnRoute = value;
                Notify("UseAssignReturnRoute");
                AvailabilityNotify();
            }
        }

        private bool _useAssignSUCRetrunRoute;
        public bool UseAssignSUCRetrunRoute
        {
            get { return _useAssignSUCRetrunRoute; }
            set
            {
                ResetAllCheckBoxes();
                _useAssignSUCRetrunRoute = value;
                Notify("UseAssignSUCRetrunRoute");
                AvailabilityNotify();
            }
        }

        private bool _useAssignPriorityReturnRoute;
        public bool UseAssignPriorityReturnRoute
        {
            get { return _useAssignPriorityReturnRoute; }
            set
            {
                ResetAllCheckBoxes();
                _useAssignPriorityReturnRoute = value;
                Notify("UseAssignPriorityReturnRoute");
                AvailabilityNotify();
            }
        }

        private bool _useAssignPrioritySUCReturnRoute;
        public bool UseAssignPrioritySUCReturnRoute
        {
            get { return _useAssignPrioritySUCReturnRoute; }
            set
            {
                ResetAllCheckBoxes();
                _useAssignPrioritySUCReturnRoute = value;
                Notify("UseAssignPrioritySUCReturnRoute");
                AvailabilityNotify();
            }
        }

        private bool _usePriorityRoute;
        public bool UsePriorityRoute
        {
            get { return _usePriorityRoute; }
            set
            {
                ResetAllCheckBoxes();
                _usePriorityRoute = value;
                Notify("UsePriorityRoute");
                AvailabilityNotify();
            }
        }

        private NodeTag[] _priorutyRoute = new NodeTag[4];
        public NodeTag[] PriorityRoute
        {
            get { return _priorutyRoute; }
            set
            {
                _priorutyRoute = value;
                Notify("PriorityRoute");
            }
        }

        private byte _routeSpeed = 3;
        public byte RouteSpeed
        {
            get { return _routeSpeed; }
            set
            {
                _routeSpeed = value;
                Notify("RouteSpeed");
            }
        }

        private bool _isSourceListEnabled;
        public bool IsSourceListEnabled
        {
            get { return _isSourceListEnabled; }
            private set
            {
                _isSourceListEnabled = value;
                Notify("IsSourceListEnabled");
            }
        }

        private bool _isDestListEnabled;
        public bool IsDestListEnabled
        {
            get { return _isDestListEnabled; }
            set
            {
                _isDestListEnabled = value;
                Notify("IsDestListEnabled");
            }
        }

        private bool _isPriorityEnabled;
        public bool IsPriorityEnabled
        {
            get { return _isPriorityEnabled; }
            private set
            {
                _isPriorityEnabled = value;
                Notify("IsPriorityEnabled");
            }
        }

        public NodeTag Source { get; set; }
        public NodeTag Destionation { get; set; }

        #endregion

        byte GENERIC_TYPE_SENSOR_BINARY_ID = 0x00;
        byte GENERIC_TYPE_SENSOR_MULTILEVEL_ID = 0x00;
        byte GENERIC_TYPE_SWITCH_BINARY_ID = 0x00;
        byte GENERIC_TYPE_SWITCH_MULTILEVEL_ID = 0x00;
        byte GENERIC_TYPE_STATIC_CONTROLLER_ID = 0x00;
        byte GENERIC_TYPE_GENERIC_CONTROLLER_ID = 0x00;
        byte GENERIC_TYPE_REPEATER_END_NODE_ID = 0x00;
        byte GENERIC_TYPE_ENTRY_CONTROL_ID = 0x00;
        byte GENERIC_TYPE_THERMOSTAT_ID = 0x00;

        private readonly object mLockObject = new object();
        public SetupRouteViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Setup Route";

            AvailabilityNotify();
            ApplicationModel.NodesChanged += new NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            SourceRouteCollection = new RouteCollection(ApplicationModel.ZWaveDefinition);
            SourceRouteCollection.Nodes = ApplicationModel.SubscribeCollectionFactory.Create<ISelectableItem<NodeTag>>();
            DestinationRouteCollection = new RouteCollection(ApplicationModel.ZWaveDefinition);
            DestinationRouteCollection.Nodes = ApplicationModel.SubscribeCollectionFactory.Create<ISelectableItem<NodeTag>>();

            GenericDevice genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_SENSOR_BINARY");

            if (genDevice != null)
                GENERIC_TYPE_SENSOR_BINARY_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_SENSOR_MULTILEVEL");
            if (genDevice != null)
                GENERIC_TYPE_SENSOR_MULTILEVEL_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_SWITCH_BINARY");
            if (genDevice != null)
                GENERIC_TYPE_SWITCH_BINARY_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_SWITCH_MULTILEVEL");
            if (genDevice != null)
                GENERIC_TYPE_SWITCH_MULTILEVEL_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_STATIC_CONTROLLER");
            if (genDevice != null)
                GENERIC_TYPE_STATIC_CONTROLLER_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_GENERIC_CONTROLLER");
            if (genDevice != null)
                GENERIC_TYPE_GENERIC_CONTROLLER_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_REPEATER_END_NODE");
            if (genDevice != null)
                GENERIC_TYPE_REPEATER_END_NODE_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_ENTRY_CONTROL");
            if (genDevice != null)
                GENERIC_TYPE_ENTRY_CONTROL_ID = genDevice.KeyId;

            genDevice = ApplicationModel.ZWaveDefinition.GenericDevices.FirstOrDefault(p =>
                p.Name == "GENERIC_TYPE_THERMOSTAT");

            if (genDevice != null)
                GENERIC_TYPE_THERMOSTAT_ID = genDevice.KeyId;
        }

        void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0 && e.NewItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag di = (e.NewItems[0] as SelectableItem<NodeTag>).Item;
                        if (di.Id < 0xFF && di.EndPointId == 0)
                        {
                            SourceRouteCollection.Nodes.Add(new SelectableItem<NodeTag>(di));
                            DestinationRouteCollection.Nodes.Add(new SelectableItem<NodeTag>(di));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0 && e.OldItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag di = (e.OldItems[0] as SelectableItem<NodeTag>).Item;
                        lock (mLockObject)
                        {
                            int index = -1;
                            for (int i = 0; i < SourceRouteCollection.Nodes.Count; i++)
                            {
                                if (SourceRouteCollection.Nodes[i].Item.Id == di.Id)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index > -1)
                                SourceRouteCollection.Nodes.RemoveAt(index);


                            index = -1;
                            for (int i = 0; i < DestinationRouteCollection.Nodes.Count; i++)
                            {
                                if (DestinationRouteCollection.Nodes[i].Item.Id == di.Id)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            if (index > -1)
                                DestinationRouteCollection.Nodes.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    lock (mLockObject)
                    {
                        SourceRouteCollection.Nodes.Clear();
                        DestinationRouteCollection.Nodes.Clear();
                    }
                    break;
                default:
                    break;
            }

        }

        private void ResetAllCheckBoxes()
        {
            _useAssignPriorityReturnRoute = false;
            _useAssignPrioritySUCReturnRoute = false;
            _useAssignReturnRoute = false;
            _useAssignSUCRetrunRoute = false;
            _usePriorityRoute = false;

            Notify("UseAssignPriorityReturnRoute");
            Notify("UseAssignPrioritySUCReturnRoute");
            Notify("UseAssignReturnRoute");
            Notify("UseAssignSUCRetrunRoute");
            Notify("UsePriorityRoute");
        }

        private void AvailabilityNotify()
        {
            IsSourceListEnabled = !UsePriorityRoute;
            IsDestListEnabled = UsePriorityRoute || UseAssignReturnRoute || UseAssignPriorityReturnRoute;
            IsPriorityEnabled = UsePriorityRoute || UseAssignPriorityReturnRoute || UseAssignPrioritySUCReturnRoute;
        }
    }

    public class RouteCollection : EntityBase, IRouteCollection
    {
        public RouteCollection(ZWaveDefinition zwaveDefinition)
        {
            ZWaveDefinition = zwaveDefinition;
        }

        public ZWaveDefinition ZWaveDefinition { get; private set; }

        private ISubscribeCollection<ISelectableItem<NodeTag>> _nodes;
        public ISubscribeCollection<ISelectableItem<NodeTag>> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                Notify("SourceRouteCollection");
            }
        }

        private ISelectableItem<NodeTag> _selectedNode;

        public ISelectableItem<NodeTag> SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                Notify("SelectedNode");
            }
        }
    }
}
