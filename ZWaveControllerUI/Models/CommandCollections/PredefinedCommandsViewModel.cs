/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using Utils.UI.Interfaces;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveControllerUI.Models
{
    /// <summary>
    /// ViewModel to manage Predefined Command Groups.
    /// </summary>
    public class PredefinedCommandsViewModel : DialogVMBase, IPredefinedCommandsModel
    {
        public PredefinedCommandsViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Predefined Commands Manager";

            //DialogSettings.IsResizable = true;
            //DialogSettings.Width = 450;
            //DialogSettings.Height = 350;
            //this.PropertyChanged += new PropertyChangedEventHandler(SelectedGroup_Changed);
        }

        //private void SelectedGroup_Changed(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "SelectedGroup")
        //    {
        //        TempGroupName = SelectedGroup.Name;
        //    }
        //}

        #region Properies IPredefinedCommandsModel

        public IPredefinedCommandGroup CreateFavoriteGroup(string name)
        {
            var ret = new PredefinedCommandGroup();
            ret.Name = name;
            ret.Items = ApplicationModel.SubscribeCollectionFactory.Create<ISelectableItem<PredefinedPayload>>();
            return ret;
        }

        private IList<IPredefinedCommandGroup> _predefinedGroups;
        public IList<IPredefinedCommandGroup> PredefinedGroups
        {
            get
            {
                return _predefinedGroups ?? (_predefinedGroups = ApplicationModel.SubscribeCollectionFactory.Create<IPredefinedCommandGroup>());
            }
            set
            {
                _predefinedGroups = value;
                Notify("Favorites");
            }
        }

        private IPredefinedCommandGroup _selectedGroup;
        public IPredefinedCommandGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                _selectedGroup = value;
                Notify("SelectedGroup");
                if (SelectedGroup != null) //or SelectedGroup_Changed
                {
                    TempGroupName = SelectedGroup.Name;
                }
                else
                {
                    TempGroupName = string.Empty;
                }
            }
        }

        private ISelectableItem<PredefinedPayload> _selectedItem { get; set; }
        public ISelectableItem<PredefinedPayload> SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                if (SelectedItem?.Item?.PayloadItem != null)
                {
                    var dt = SelectedItem.Item.PayloadItem.Payload;
                    var res = ((CommandClassesViewModel)ApplicationModel.CommandClassesModel).
                        FillSelectedCommandDetails(dt);
                    SelectedItemDetails = res;
                }
                else
                {
                    SelectedItemDetails = string.Empty;
                }
                Notify("SelectedItem");
            }
        }

        private string _tempGroupName;
        public string TempGroupName
        {
            get { return _tempGroupName; }
            set
            {
                _tempGroupName = value;
                Notify("TempGroupName");
            }
        }

        public ISelectableItem<PredefinedPayload> GetItem(PayloadItem payloadItem)
        {
            var id = (SelectedGroup != null && SelectedGroup.Items.Any()) ?
                SelectedGroup.Items.Max(i => i.Item.Id) + 1 : 1;
            var item = new PredefinedPayload(id)
            {
                PayloadItem = payloadItem
            };
            return new SelectableItem<PredefinedPayload>(item);
        }

        #endregion

        #region Commands
        public SendPredefinedGroupCommand SendPredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<SendPredefinedGroupCommand>();
        public LoadPredefinedGroupCommand LoadPredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<LoadPredefinedGroupCommand>();
        public SavePredefinedGroupCommand SavePredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<SavePredefinedGroupCommand>();

        public CommandBase EditSelectedItemCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => EditSelectedItem(param), c => SelectedItem != null);
        private void EditSelectedItem(object param)
        {
            var payloadItem = SelectedItem.Item.PayloadItem;
            ApplicationModel.Invoke(() =>
            {

                ((CommandClassesViewModel)ApplicationModel.CommandClassesModel).FillCommandClassesViewModel(payloadItem);
            });
        }

        //Groups:
        public DeletePredefinedGroupsCommand DeletePredefinedGroupsCommand => CommandsFactory.CommandControllerSessionGet<DeletePredefinedGroupsCommand>(); //clear.
        public AddPredefinedGroupCommand AddPredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<AddPredefinedGroupCommand>();
        public DeletePredefinedGroupCommand DeletePredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<DeletePredefinedGroupCommand>();
        public CopyPredefinedGroupCommand CopyPredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<CopyPredefinedGroupCommand>();
        public RenamePredefinedGroupCommand RenamePredefinedGroupCommand => CommandsFactory.CommandControllerSessionGet<RenamePredefinedGroupCommand>();
        //Items:
        public AddPredefinedItemCommand AddPredefinedItemCommand => CommandsFactory.CommandControllerSessionGet<AddPredefinedItemCommand>();
        public DeletePredefinedCommandCommand DeletePredefinedCommandCommand => CommandsFactory.CommandControllerSessionGet<DeletePredefinedCommandCommand>();
        public MoveUpPredefinedItemCommand MoveUpPredefinedItemCommand => CommandsFactory.CommandControllerSessionGet<MoveUpPredefinedItemCommand>();
        public MoveDownPredefinedItemCommand MoveDownPredefinedItemCommand => CommandsFactory.CommandControllerSessionGet<MoveDownPredefinedItemCommand>();
        public CopyPredefinedItemCommand CopyPredefinedItemCommand => CommandsFactory.CommandControllerSessionGet<CopyPredefinedItemCommand>();
        #endregion

        private string _selectedItemDetails;
        public string SelectedItemDetails
        {
            get
            {
                return _selectedItemDetails;
            }
            set
            {
                _selectedItemDetails = value;
                Notify("SelectedItemDetails");
            }
        }
    }

    /// <summary>
    /// Group of predefined commands <see cref="PredefinedPayload"/>
    /// </summary>
    public class PredefinedCommandGroup : IPredefinedCommandGroup
    {
        public string Name { get; set; }
        public IList<ISelectableItem<PredefinedPayload>> Items { get; set; }
        public uint GetFreeFavoriteItemId()
        {
            return Items.Any() ? (Items.Max(i => i.Item.Id) + 1) : 1;
        }
    }
}