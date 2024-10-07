/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI;
using Utils.UI.Bind;
using ZWave.Devices;
using System.Collections;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class AssociationModel : VMBase, IAssociationsModel
    {
        #region Properties

        private ISubscribeCollection<IAssociativeApplication> _associativeApplications;
        public ISubscribeCollection<IAssociativeApplication> AssociativeApplications
        {
            get
            {
                return _associativeApplications;
            }
            set
            {
                _associativeApplications = value;
                Notify("AssociativeApplications");
            }
        }

        private IAssociativeDevice _selectedAssociativeDevice;
        public IAssociativeDevice SelectedAssociativeDevice
        {
            get { return _selectedAssociativeDevice; }
            set
            {
                _selectedAssociativeDevice = value;
                Notify("SelectedAssociativeDevice");
            }
        }

        private IAssociativeGroup _selectedGroup;
        public IAssociativeGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                Notify("SelectedGroup");
            }
        }

        private IAssociativeNode _selectedAssociativeNode;
        public IAssociativeNode SelectedAssociativeNode
        {
            get { return _selectedAssociativeNode; }
            set
            {
                _selectedAssociativeNode = value;
                Notify("SelectedAssociativeNode");
            }
        }

        private object _selectedObject;
        public object SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                _selectedObject = value;
                SelectedAssociativeNode = null;
                if (value is AssociativeApplication)
                {
                    SelectedAssociativeDevice = null;
                }
                if (value is AssociativeDevice)
                {
                    SelectedAssociativeDevice = (AssociativeDevice)value;
                    SelectedGroup = null;
                }
                else if (value is AssociativeGroup)
                {
                    SelectedGroup = (AssociativeGroup)value;
                }
                else if (value is AssociativeFolder)
                {
                    SelectedGroup = (AssociativeGroup)((AssociativeFolder)value).Parent;
                }
                else if (value is AssociativeNode)
                {
                    var val = (AssociativeNode)value;
                    SelectedAssociativeNode = val;
                    SelectedGroup = val.ParentGroup;
                }
                Notify("SelectedObject");
            }
        }

        private object _expandObject;
        public object ExpandObject
        {
            get { return _expandObject; }
            set
            {
                _expandObject = value;
                Notify("ExpandObject");
            }
        }

        private bool _isAssignReturnRoutes = true;
        public bool IsAssignReturnRoutes
        {
            get { return _isAssignReturnRoutes; }
            set
            {
                _isAssignReturnRoutes = value;
                Notify("IsAssignReturnRoutes");
            }
        }

        private List<NodeTag> _selectedNodeIds = new List<NodeTag>();
        public List<NodeTag> SelectedNodeIds
        {
            get { return _selectedNodeIds; }
            set
            {
                _selectedNodeIds = value;
                Notify("SelectedNodeIds");
            }
        }

        #endregion

        public AssociationModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Associations";
            AssociativeApplications = ((MainViewModel)applicationModel).SubscribeCollectionFactory.Create<IAssociativeApplication>();
        }

        public override object Clone()
        {
            AssociationModel ret = (AssociationModel)base.Clone();
            ret.SelectedNodeIds = new List<NodeTag>(SelectedNodeIds);
            return ret;
        }

        public virtual void UpdateAssociativeDevice(NodeTag nodeId)
        {
        }
    }

    public class AssociativeApplication : EntityBase, IAssociativeApplication
    {
        private List<IAssociativeDevice> _devices;
        public List<IAssociativeDevice> Devices
        {
            get { return _devices; }
            set
            {
                _devices = value;
                Notify("Devices");
            }
        }

        public NodeTag RootDevice
        {
            get
            {
                NodeTag ret = new NodeTag();
                var ad = Devices.Find(x => x.Device.EndPointId == 0);
                if (ad != null)
                {
                    ret = ad.Device;
                }
                return ret;
            }
        }

        public AssociativeApplication()
        {
            Devices = new List<IAssociativeDevice>();
        }
    }

    public class AssociativeDevice : EntityBase, IAssociativeDevice
    {
        public AssociativeDevice(NodeTag item)
        {
            Device = item;
            Groups = new List<IAssociativeGroup>();
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

        private List<IAssociativeGroup> _groups;
        public List<IAssociativeGroup> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                Notify("Groups");
            }
        }

        public void UpdateGroup(byte groupId, byte maxNodesSupported, IList<NodeTag> nodeIds)
        {
            foreach (var item in Groups)
            {
                if (item.Id == groupId)
                {
                    item.Update(maxNodesSupported, nodeIds);
                }
            }
        }

        private IAssociativeGroup CreateGroup(byte id)
        {
            IAssociativeGroup ret = new AssociativeGroup();
            ret.Id = id;
            ret.ParentDevice = this;
            return ret;
        }

        public void SetGroups(byte groupCount)
        {
            List<IAssociativeGroup> tmp = new List<IAssociativeGroup>();
            for (int i = 0; i < groupCount; i++)
            {
                tmp.Add(CreateGroup((byte)(i + 1)));
            }
            Groups = tmp;
        }

        public IAssociativeApplication ParentApplication { get; set; }

        public NodeTag[] GetNodeIds()
        {
            var res = new List<NodeTag>();
            foreach (var item in Groups)
            {
                var nodes = item.GetNodeIds();
                res.AddRange(nodes);
            }
            return res.ToArray();
        }
    }

    public class AssociativeGroup : EntityBase, IAssociativeGroup
    {
        public AssociativeGroup()
        {
        }

        public IAssociativeDevice ParentDevice { get; set; }

        private IAssociativeNodeCollection _nodes;
        public IAssociativeNodeCollection Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                Notify("Nodes");
            }
        }

        public NodeTag[] GetNodeIds()
        {
            var len = Nodes == null ? 0 : Nodes.NodeIds.Count;
            var ret = new NodeTag[len];
            for (int i = 0; i < len; i++)
            {
                ret[i] = ((SelectableItem<NodeTag>)Nodes.NodeIds[i]).Item;
            }
            return ret;
        }

        private byte _maxNodesSupported;
        public byte MaxNodesSupported
        {
            get { return _maxNodesSupported; }
            set
            {
                _maxNodesSupported = value;
                Notify("MaxNodesSupported");
            }
        }

        private byte _id;
        public byte Id
        {
            get { return _id; }
            set
            {
                _id = value;
                Notify("Id");
            }
        }

        public void Update(byte maxNodesSupported, IList<NodeTag> nodeIds)
        {
            if (nodeIds != null && nodeIds.Count > 0)
            {
                List<IAssociativeNode> tmp = new List<IAssociativeNode>();
                if (nodeIds != null)
                {
                    foreach (var item in nodeIds)
                    {
                        tmp.Add(new AssociativeNode(item, this));
                    }
                }
                if (Nodes == null)
                {
                    Nodes = new AssociativeNodeCollection();
                    Nodes.ParentGroup = this;
                }
                Nodes.NodeIds = tmp;
            }
            else
                Nodes = null;
            Nodes = Nodes;
            MaxNodesSupported = maxNodesSupported;
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                Notify("GroupName");
            }
        }

        private IAssociativeGroupInfo _groupInfo;
        public IAssociativeGroupInfo GroupInfo
        {
            get { return _groupInfo; }
            set
            {
                _groupInfo = value;
                Notify("GroupInfo");
            }
        }

        private List<string> _groupCommandClasses;
        public List<string> GroupCommandClasses
        {
            get { return _groupCommandClasses; }
            set
            {
                _groupCommandClasses = value;
                Notify("GroupCommandClasses");
            }
        }

        public void SetGroupInfo(string profile1, string profile2)
        {
            GroupInfo = new AssociativeGroupInfo
            {
                ParentGroup = this,
                Profile = new List<string> { profile1, profile2 }
            };
        }

    }

    public class AssociativeNodeCollection : EntityBase, IAssociativeNodeCollection
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                Notify("Title");
            }
        }

        private List<IAssociativeNode> _nodeIds;
        public List<IAssociativeNode> NodeIds
        {
            get { return _nodeIds; }
            set
            {
                _nodeIds = value;
                Notify("NodeIds");
                Notify("Nodes");
            }
        }

        public IAssociativeGroup ParentGroup { get; set; }
    }

    public class AssociativeNode : SelectableItem<NodeTag>, IAssociativeNode
    {
        public AssociativeNode(NodeTag nodeId, AssociativeGroup parent)
            : base(nodeId)
        {
            ParentGroup = parent;
        }

        public IAssociativeGroup ParentGroup { get; private set; }
    }

    public class AssociativeGroupInfo : EntityBase, IAssociativeGroupInfo
    {
        public AssociativeGroupInfo()
        {
            Profile = new List<string>();
        }

        private List<string> _profile;
        public List<string> Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
                Notify("Profile");
            }
        }

        public IAssociativeGroup ParentGroup { get; set; }
    }

    public class AssociativeFolder : EntityBase
    {
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Notify("Name");
            }
        }

        private IEnumerable _items;
        public IEnumerable Items
        {
            get { return _items; }
            set
            {
                _items = value;
                Notify("Items");
            }
        }

        private object _parent;
        public object Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                Notify("Parent");
            }
        }
    }
}
