/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZWaveController.Commands;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Services
{
    public class PredefinedPayloadsService : IPredefinedPayloadsService
    {
        private readonly IApplicationModel _applicationModel;
        private readonly IPredefinedCommandsModel _predefinesModel;

        public PredefinedPayloadsService(IApplicationModel applicationModel)
        {
            _applicationModel = applicationModel;
            _predefinesModel = _applicationModel.PredefinedCommandsModel;
        }

        /// <summary>
        /// Get unique group name from param <paramref name="name"/>
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>New Name if <paramref name="name"/> is exists.</returns>
        private string GetGroupName(string name)
        {
            return _predefinesModel.PredefinedGroups.Any(i => i.Name == name) ? GetGroupName(name + "(1)") : name;
        }

        /// <summary>
        /// Execute Sequently Send Data commands.
        /// </summary>
        /// <param name="commands">Commands to send<see cref="PredefinedPayload"/>.</param>
        private void RunCommands(IEnumerable<PredefinedPayload> commands)
        {
            if (commands == null || !commands.Any())
                return;

            //var commandClass = _applicationModel.CommandClassesModel.Clone();
            _applicationModel.CommandClassesModel.IsExpectCommand = false;
            foreach (var command in commands.OrderBy(i => i.Id))
            {
                //if (command.PayloadItem.SecureType != SecureType.SerialApi)  // ATTENTION!
                _applicationModel.CommandClassesModel = _applicationModel.CommandClassesModel.Clone(command.PayloadItem);
                var sendCommand = CommandsFactory.CommandControllerSessionGet<SendCommand>();
                CommandsFactory.CommandRunner.ExecuteAsync(sendCommand, null);
                Thread.Sleep(_applicationModel.SendDataSettingsModel.DelayResponseMs == 0 ? 200 : _applicationModel.SendDataSettingsModel.DelayResponseMs);
            }
            //_applicationModel.CommandClassesModel = commandClass; // DOES NOT WORK - review!
        }

        public CommandExecutionResult Initialize()
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            _applicationModel.Invoke(() =>
            {
                _predefinesModel.PredefinedGroups.Clear();
                _predefinesModel.SelectedGroup = null;
                _predefinesModel.SelectedItem = null;
            });

            ret = AddGroup("Temporary");
            return ret;
        }

        public CommandExecutionResult LoadGroup(string groupName, string json)
        {
            var list = JsonConvert.DeserializeObject<IList<PayloadItem>>(json).ToArray();
            CommandExecutionResult ret = AddGroup(groupName);
            if (ret == CommandExecutionResult.OK)
            {
                _predefinesModel.SelectedGroup = _predefinesModel.PredefinedGroups.LastOrDefault();
                foreach (var item in list)
                {
                    ret = AddItemToGroup(groupName, item);

                    if (ret == CommandExecutionResult.OK)
                    {
                        _predefinesModel.SelectedItem = _predefinesModel.SelectedGroup.Items.FirstOrDefault();
                    }
                }
            }
            return ret;
        }

        public CommandExecutionResult SaveGroup(string name, string path)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == name);
            if (group != null)
            {
                try
                {
                    var payloadItems = group.Items.Select(i => i.Item.PayloadItem).ToList();
                    var json = JsonConvert.SerializeObject(payloadItems);
                    File.WriteAllText(path, json);
                    ret = CommandExecutionResult.OK;
                }
                catch (Exception ex)
                {
                    // Return error to Command. If needed.
                }
            }
            return ret;
        }

        public CommandExecutionResult AddGroup(string name)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            _applicationModel.Invoke(() =>
            {
                var group = _predefinesModel.CreateFavoriteGroup(GetGroupName(name));
                _predefinesModel.PredefinedGroups.Add(group);
                _predefinesModel.SelectedGroup = _predefinesModel.PredefinedGroups.LastOrDefault();
                ret = CommandExecutionResult.OK;
            });
            return ret;
        }

        public CommandExecutionResult RenameGroup(string name, string newName)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(newName) && !_predefinesModel.PredefinedGroups.Any(x => x.Name == newName))
            {
                var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == name);
                if (group != null)
                {
                    _applicationModel.Invoke(() =>
                    {
                        group.Name = newName;
                        _predefinesModel.SelectedGroup = _predefinesModel.PredefinedGroups.FirstOrDefault(x => x == group);
                        _predefinesModel.PredefinedGroups = _predefinesModel.PredefinedGroups; // can't update LIST!!! + selected in list
                        ret = CommandExecutionResult.OK;
                    });
                }
            }
            return ret;
        }

        public CommandExecutionResult CloneGroup(string originalName, string newName = null)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            if (!string.IsNullOrEmpty(originalName) && _predefinesModel.PredefinedGroups.Any(x => x.Name == originalName))
            {
                var cloneName = string.IsNullOrEmpty(newName) || newName == originalName ? GetGroupName(originalName) : newName;
                ret = AddGroup(cloneName);
                var originalGroup = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == originalName);
                foreach (var payloadItem in originalGroup?.Items?.Select(x => x.Item.PayloadItem).ToList())
                {
                    AddItemToGroup(cloneName, payloadItem);
                }
            }
            return ret;
        }

        public CommandExecutionResult DeleteGroup(string name)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            if (!string.IsNullOrEmpty(name))
            {
                var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == name);
                if (group != null)
                {
                    _applicationModel.Invoke(() =>
                    {
                        if (_predefinesModel.PredefinedGroups.Remove(group))
                        {
                            _predefinesModel.SelectedGroup = _predefinesModel.PredefinedGroups.LastOrDefault();
                            _predefinesModel.SelectedItem = null;
                            ret = CommandExecutionResult.OK;
                        }
                    });
                }
            }
            return ret;
        }

        public CommandExecutionResult DeleteGroups()
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            _applicationModel.Invoke(() =>
            {
                _predefinesModel.PredefinedGroups.Clear();
                _predefinesModel.SelectedGroup = null;
                ret = CommandExecutionResult.OK;
            });
            return ret;
        }

        public CommandExecutionResult AddItemToGroup(string groupName, PayloadItem payloadItem)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            IPredefinedCommandGroup group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
            if (group != null)
            {
                var favoriteItem = _predefinesModel.GetItem(payloadItem);
                favoriteItem.Item.Id = group.GetFreeFavoriteItemId();
                _applicationModel.Invoke(() =>
                {
                    group.Items.Add(favoriteItem);
                    _predefinesModel.SelectedGroup = _predefinesModel.SelectedGroup;
                    _predefinesModel.SelectedItem = _predefinesModel.SelectedGroup.Items.LastOrDefault();
                    ret = CommandExecutionResult.OK;
                });
            }
            return ret;
        }

        public CommandExecutionResult DeleteItem(string groupName, uint id)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
            if (group != null)
            {
                var favoriteItem = group.Items.FirstOrDefault(i => i.Item.Id == id);
                if (favoriteItem != null)
                {
                    _applicationModel.Invoke(() =>
                        {
                            group.Items.Remove(favoriteItem);
                            uint idx = 0;
                            foreach (var item in group.Items)
                            {
                                item.Item.Id = ++idx;
                            }
                            ret = CommandExecutionResult.OK;
                            _predefinesModel.SelectedGroup = _predefinesModel.SelectedGroup;
                            if (id - 1 > 0)
                            {
                                _predefinesModel.SelectedItem = _predefinesModel.SelectedGroup.Items.FirstOrDefault(x => x.Item.Id == id - 1);
                            }
                            else
                            {
                                _predefinesModel.SelectedItem = null;
                            }
                        });
                }
            }
            return ret;
        }

        public CommandExecutionResult MoveUp(string groupName, uint id)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
            if (group != null)
            {
                var currItem = group.Items.FirstOrDefault(i => i.Item.Id == id);
                var prevItem = group.Items.FirstOrDefault(i => i.Item.Id == id - 1);
                if (currItem != null && prevItem != null)
                {
                    _applicationModel.Invoke(() =>
                    {
                        var srcIx = group.Items.IndexOf(currItem);
                        var dstIx = group.Items.IndexOf(prevItem);
                        var temp = group.Items[srcIx];
                        group.Items[srcIx] = group.Items[dstIx];
                        group.Items[dstIx] = temp;
                        currItem.Item.Id = id - 1;
                        prevItem.Item.Id = id;
                        _predefinesModel.SelectedItem = temp;
                        ret = CommandExecutionResult.OK;
                    });
                }
            }
            return ret;
        }

        public CommandExecutionResult MoveDown(string groupName, uint id)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
            if (group != null)
            {

                var currItem = group.Items.FirstOrDefault(i => i.Item.Id == id);
                var nextItem = group.Items.FirstOrDefault(i => i.Item.Id == id + 1);
                if (currItem != null && nextItem != null)
                {
                    _applicationModel.Invoke(() =>
                    {
                        var srcIx = group.Items.IndexOf(currItem);
                        var dstIx = group.Items.IndexOf(nextItem);
                        var temp = group.Items[srcIx];
                        group.Items[srcIx] = group.Items[dstIx];
                        group.Items[dstIx] = temp;
                        currItem.Item.Id = id + 1;
                        nextItem.Item.Id = id;
                        _predefinesModel.SelectedItem = temp;
                        ret = CommandExecutionResult.OK;
                    });
                }
            }
            return ret;
        }


        public CommandExecutionResult CopyItem(string groupName, uint id)
        {
            CommandExecutionResult ret = CommandExecutionResult.Failed;
            var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
            if (group != null)
            {
                var favoriteItem = group.Items.FirstOrDefault(i => i.Item.Id == id);
                if (favoriteItem != null)
                {
                    AddItemToGroup(groupName, favoriteItem.Item.PayloadItem);
                }
            }
            return ret;
        }

        public void RunGroup(string name)
        {
            RunCommands(_predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == name).Items.Select(i => i.Item));
        }

        public void RunCommands(string groupName, List<uint> ids)
        {
            RunCommands(_predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName).Items.Where(i => ids.Contains(i.Item.Id)).Select(i => i.Item));
        }

    }
}
/* Review is it still needed: *
        //public void RunSelectedCommands(string groupName) == public void RunGroup(string name)
        //{
        //    RunCommands(_predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName).Items.Where(i => i.IsSelected).Select(i => i.Item));
        //}


 
        //public ISelectableItem<PredefinedPayload> CloneItem(string groupName, uint id)
        //{
        //    var group = _applicationModel.FavoritePayloadsModel.FavoriteGroups.FirstOrDefault(i => i.Item.Name == groupName);
        //    if (group == null)
        //        return null;

        //    var favoriteItem = group.Item.PayloadItems.FirstOrDefault(i => i.Item.Id == id);
        //    if (favoriteItem == null)
        //        return null;

        //    var item = _mapper.Map<FavoriteItem>(favoriteItem);
        //    item.Id = group.Item.GetFavoriteItemId();
        //    var cloneItem = _applicationModel.CreateSelectableItem<FavoriteItem>(item);
        //    group.Item.PayloadItems.Add(cloneItem);

        //    SaveGroup(groupName);
        //    return cloneItem;
        //}

        //public IPredefinedCommandGroup UploadGroup(string name, List<ISelectableItem<PredefinedPayload>> favoriteItems)
        //{
        //    var group = _predefinesModel.CreateFavoriteGroup(GetGroupName(name));
        //    group.Items = favoriteItems.OrderBy(i => i.Item.Id).ToList();
        //    _predefinesModel.PredefinedGroups.Add(group);
        //    SaveGroup(name);
        //    return group;
        //}

        //public void AddItemsToGroup(string groupName, List<ISelectableItem<PredefinedPayload>> favoriteItems)
        //{
        //    var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
        //    if (group == null)
        //        return;
        //    group.Items = new List<ISelectableItem<PredefinedPayload>>(); // fix nullable fail ?!
        //    favoriteItems.ForEach(i =>
        //    {
        //        i.Item.Id = group.GetFavoriteItemId();
        //        group.Items.Add(i);
        //    });

        //    SaveGroup(groupName);
        //}

        //public void UpdateItem(string groupName, ISelectableItem<PredefinedPayload> favoriteItem)
        //{
        //    var group = _predefinesModel.PredefinedGroups.FirstOrDefault(i => i.Name == groupName);
        //    if (group == null)
        //        return;
        //    var item = group.Items.FirstOrDefault(i => i.Item.Id == favoriteItem.Item.Id);

        //    if (item == null)
        //        return;

        //    group.Items.Remove(item);
        //    group.Items.Add(favoriteItem);

        //    group.Items = group.Items.OrderBy(i => i.Item.Id).ToList();
        //    SaveGroup(groupName);
        //}
 */