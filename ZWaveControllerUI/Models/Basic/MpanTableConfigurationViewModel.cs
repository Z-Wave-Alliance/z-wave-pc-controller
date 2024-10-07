/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.ComponentModel;
using Utils.UI;
using ZWave.Security;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController;
using System.Collections.Generic;
using ZWave.Devices;

namespace ZWaveControllerUI.Models
{
    public class MpanTableConfigurationViewModel : DialogVMBase, IMpanTableConfigurationModel
    {
        #region Properties

        private IList<IMpanItem> _fullMpanTableBind;
        public IList<IMpanItem> FullMpanTableBind
        {
            get { return _fullMpanTableBind; }
            set
            {
                _fullMpanTableBind = value;
                Notify("FullMpanTableBind");
            }
        }

        private IMpanItem _selectedMpanItem;
        public IMpanItem SelectedMpanItem
        {
            get { return _selectedMpanItem; }
            set
            {
                _selectedMpanItem = value;
                Notify("SelectedMpanItem");
            }
        }

        private byte _testMpanGroupId = 0x01;
        public byte TestMpanGroupId
        {
            get { return _testMpanGroupId; }
            set
            {
                _testMpanGroupId = value;
                Notify("TestMpanGroupId");
            }
        }

        private NodeTag _testMpanOwner = NodeTag.Empty;
        public NodeTag TestMpanOwner
        {
            get { return _testMpanOwner; }
            set
            {
                _testMpanOwner = value;
                Notify("TestMpanOwner");
            }
        }

        private bool _testMpanIsMos;
        public bool TestMpanIsMos
        {
            get { return _testMpanIsMos; }
            set
            {
                _testMpanIsMos = value;
                Notify("TestMpanIsMos");
            }
        }

        private byte[] _testMpanState;
        public byte[] TestMpanState
        {
            get { return _testMpanState; }
            set
            {
                _testMpanState = value;
                Notify("TestMpanState");
            }
        }

        private byte[] _testMpanValue;
        public byte[] TestMpanValue
        {
            get { return _testMpanValue; }
            set
            {
                _testMpanValue = value;
                Notify("TestMpanValue");
            }
        }

        private NodeTag[] _testMpanNodes;
        public NodeTag[] TestMpanNodes
        {
            get { return _testMpanNodes; }
            set
            {
                _testMpanNodes = value;
                Notify("TestMpanNodes");
            }
        }

        private byte _testMpanSequenceNumber;
        public byte TestMpanSequenceNumber
        {
            get { return _testMpanSequenceNumber; }
            set
            {
                _testMpanSequenceNumber = value;
                Notify("TestMpanSequenceNumber");
            }
        }

        public NodeGroupId TestMpanNodeGroupId
        {
            get
            {
                return new NodeGroupId(TestMpanOwner, TestMpanGroupId);
            }
        }
        #endregion

        #region Commands
        public LoadMpanTableCommand LoadMpanTableCommand => CommandsFactory.CommandControllerSessionGet<LoadMpanTableCommand>();
        public RemoveSelectedMpanItemCommand RemoveSelectedMpanItemCommand => CommandsFactory.CommandControllerSessionGet<RemoveSelectedMpanItemCommand>();
        public NextSelectedMpanItemCommand NextSelectedMpanItemCommand => CommandsFactory.CommandControllerSessionGet<NextSelectedMpanItemCommand>();
        public ClearMpanTableCommand ClearMpanTableCommand => CommandsFactory.CommandControllerSessionGet<ClearMpanTableCommand>();
        public AddOrUpdateSelectedMpanItemCommand AddOrUpdateSelectedMpanItemCommand => CommandsFactory.CommandControllerSessionGet<AddOrUpdateSelectedMpanItemCommand>();
        #endregion

        public MpanTableConfigurationViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = true;
            Description = "Modify existing Mpan table";
            Title = "Mpan table configurations";
            FullMpanTableBind = ApplicationModel.SubscribeCollectionFactory.Create<IMpanItem>();
            this.PropertyChanged += new PropertyChangedEventHandler(MpanTableConfigurationViewModel_PropertyChanged);
        }

        void MpanTableConfigurationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedMpanItem")
            {
                if (SelectedMpanItem != null)
                {
                    TestMpanGroupId = SelectedMpanItem.GroupId;
                    TestMpanIsMos = SelectedMpanItem.IsMos;
                    TestMpanNodes = SelectedMpanItem.Nodes;
                    TestMpanOwner = SelectedMpanItem.Owner;
                    TestMpanValue = SelectedMpanItem.MpanValue;
                    TestMpanState = SelectedMpanItem.MpanState;
                    TestMpanSequenceNumber = SelectedMpanItem.SequenceNumber;
                }
            }
        }

        private IMpanItem CreateItemFromMpanContainer(MpanContainer mpanContainer)
        {
            return new MpanItem
            {
                GroupId = mpanContainer.NodeGroupId.GroupId,
                Owner = mpanContainer.NodeGroupId.Node,
                IsMos = mpanContainer.IsMosState,
                MpanState = mpanContainer.MpanState,
                Nodes = mpanContainer.ReceiverGroupHandle,
                SequenceNumber = mpanContainer.SequenceNumber
            };
        }

        public void AddMpanItem(MpanContainer mpanContainer)
        {
            var mpanItem = CreateItemFromMpanContainer(mpanContainer);
            ApplicationModel.Invoke(() => FullMpanTableBind.Add(mpanItem));
        }

        public void ClearMpanTable()
        {
            ApplicationModel.Invoke(() => FullMpanTableBind.Clear());
            TestMpanState = new byte[] { };
            TestMpanValue = null;
        }
    }

    public class MpanItem : EntityBase, IMpanItem
    {
        private byte _groupId;
        public byte GroupId
        {
            get { return _groupId; }
            set
            {
                _groupId = value;
                Notify("GroupId");
            }
        }

        private NodeTag _owner;
        public NodeTag Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                Notify("Owner");
            }
        }

        private bool _isMos;
        public bool IsMos
        {
            get { return _isMos; }
            set
            {
                _isMos = value;
                Notify("IsMos");
            }
        }

        public byte[] MpanValue { get; set; }

        private byte[] _mpanState;
        public byte[] MpanState
        {
            get { return _mpanState; }
            set
            {
                _mpanState = value;
                Notify("MpanState");
            }
        }

        private NodeTag[] _nodes;
        public NodeTag[] Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                Notify("Nodes");
            }
        }

        private byte _sequenceNumber;
        public byte SequenceNumber
        {
            get { return _sequenceNumber; }
            set
            {
                _sequenceNumber = value;
                Notify("SequenceNumber");
            }
        }

        public NodeGroupId NodeGroupId
        {
            get
            {
                return new NodeGroupId(Owner, GroupId);
            }
        }
    }
}

