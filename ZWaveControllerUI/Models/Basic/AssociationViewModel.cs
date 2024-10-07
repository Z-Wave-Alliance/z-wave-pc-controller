/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using System.Collections.Specialized;
using Utils.UI.Wrappers;
using ZWave.Devices;
using ZWaveController.Commands;
using ZWave.CommandClasses;
using Utils;
using ZWaveController;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class AssociationViewModel : AssociationModel
    {
        #region Commands
        public AssociationGetGroupsCommand AssociationGetGroupsCommand => CommandsFactory.CommandControllerSessionGet<AssociationGetGroupsCommand>();
        public AssociationCreateCommand AssociationCreateCommand => CommandsFactory.CommandControllerSessionGet<AssociationCreateCommand>();
        public AssociationGetCommand AssociationGetCommand => CommandsFactory.CommandControllerSessionGet<AssociationGetCommand>();
        public AssociationRemoveCommand AssociationRemoveCommand => CommandsFactory.CommandControllerSessionGet<AssociationRemoveCommand>();
        public AssociationGetCommandListCommand AssociationGetCommandListCommand => CommandsFactory.CommandControllerSessionGet<AssociationGetCommandListCommand>();
        public AssociationGetGroupInfoCommand AssociationGetGroupInfoCommand => CommandsFactory.CommandControllerSessionGet<AssociationGetGroupInfoCommand>();
        #endregion

        public AssociationViewModel(IApplicationModel applicationModel)
            : base(applicationModel)
        {
            ApplicationModel.NodesChanged += new NotifyCollectionChangedEventHandler(Nodes_CollectionChanged);
            ApplicationModel.SelectedNodesChanged += new System.Action(ApplicationModel_SelectedNodesChanged);
        }

        void ApplicationModel_SelectedNodesChanged()
        {
            SelectedNodeIds.Clear();
            var addedNodes = ApplicationModel.ConfigurationItem.Nodes.ToArray();
            foreach (var item in addedNodes)
            {
                if (item.IsSelected && item.IsEnabled)
                {
                    SelectedNodeIds.Add(item.Item);
                }
            }
            //SelectedNodeIds.AddRange(MainVM.SelectedNodeItems.Select(x => x.Item.Id));
        }

        void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0 && e.NewItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag node = (e.NewItems[0] as SelectableItem<NodeTag>).Item;
                        if (node.Id != ApplicationModel.Controller?.Id &&
                            //TODO: Replace 'di.IsCommandClassSupported' with 'Controller.Network.IsSpportSecure'
                            (ApplicationModel.Controller?.Network.HasCommandClass(node, COMMAND_CLASS_ASSOCIATION.ID) == true 
                             || AssociativeApplications.Select(x => x.RootDevice.Id).Contains(node.Id))
                            )
                        {
                            var ad = new AssociativeDevice(node);
                            if (node.EndPointId == 0)
                            {
                                var newRoot = new AssociativeApplication();
                                newRoot.Devices.Add(new AssociativeDevice(node) { ParentApplication = newRoot });
                                AssociativeApplications.Add(newRoot);
                            }
                            else
                            {
                                var root = AssociativeApplications.FirstOrDefault(x => x.RootDevice.Id == node.Id);
                                if (root != null)
                                {
                                    ad.ParentApplication = root;
                                    int idx = root.Devices.FindIndex(bb => bb.Device == node);
                                    if (idx > -1)
                                    {
                                        root.Devices[idx] = ad;
                                    }
                                    else
                                    {
                                        root.Devices.Add(ad);
                                    }
                                }
                                else
                                {
                                    "Assosiations: Root Device id:{0} does not exist for end point:{1}"._EXLOG(node.Id, node.EndPointId);
                                }
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0 && e.OldItems[0] is SelectableItem<NodeTag>)
                    {
                        NodeTag node = (e.OldItems[0] as SelectableItem<NodeTag>).Item;
                        int index = -1;
                        for (int i = 0; i < AssociativeApplications.Count; i++)
                        {
                            if (AssociativeApplications[i].RootDevice == node)
                            {
                                index = i;
                                break;
                            }
                        }
                        if (index > -1)
                        {
                            AssociativeApplications.RemoveAt(index);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    AssociativeApplications.Clear();
                    break;
                default:
                    break;
            }
        }

        public override void UpdateAssociativeDevice(NodeTag node)
        {
            if (node.Id != ApplicationModel.Controller?.Id && ApplicationModel.Controller?.Network.HasCommandClass(node, COMMAND_CLASS_ASSOCIATION.ID) == true)
            {
                var root = AssociativeApplications.FirstOrDefault(x => x.RootDevice != null && x.RootDevice.Id == node.Id);
                if (root != null)
                {
                    var ad = root.Devices.Find(x => x.Device == node);
                    if (ad == null)
                    {
                        ApplicationModel.Invoke(() => root.Devices.Add(new AssociativeDevice(node) { ParentApplication = root }));
                    }
                }
                else
                {
                    ApplicationModel.Invoke(() =>
                    {
                        var newRoot = new AssociativeApplication();
                        newRoot.Devices.Add(new AssociativeDevice(node) { ParentApplication = newRoot });
                        AssociativeApplications.Add(newRoot);
                        foreach (var item in ApplicationModel.ConfigurationItem.Nodes.Where(x => x.Item.Id == node.Id && x.Item.EndPointId != 0))
                        {
                            var ad = new AssociativeDevice(item.Item);
                            ad.ParentApplication = newRoot;
                            newRoot.Devices.Add(ad);
                        }
                    });
                }
            }
        }
    }
}
