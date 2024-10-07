/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;
using Utils.UI.Interfaces;
using ZWaveController.Models;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Utils;
using System.Collections.Generic;
using ZWaveController.Commands;
using ZWaveController;

namespace ZWaveControllerUI.Models
{
    public class RepeatListManagerViewModel : DialogVMBase //, ICommandClassesModel
    {
        public CommandClassesViewModel Parent { get; set; }

        public IRepeatListModel SelectedRepeatList { get { return Parent.SelectedRepeatList; } }
        public RepeatListManagerViewModel(IApplicationModel applicationModel, CommandClassesViewModel commandClassesViewModel) : base(applicationModel)
        {
            Title = "Repeat List Manager";
            Parent = commandClassesViewModel;
        }

        public string TemplateName { get; set; } = "New List";

        #region Commands
        public CreateRepeatListCommand CreateRepeatListCommand => CommandsFactory.CommandControllerSessionGet<CreateRepeatListCommand>();
        public CommandBase DeleteRepeatListCommand => CommandsFactory.CommandBaseGet<CommandBase>(DeleteRepeatList, c => !ApplicationModel.IsBusy && SelectedRepeatList != null);
        private void DeleteRepeatList(object obj)
        {
            ApplicationModel.Invoke(() =>
            {
                TemplateName = string.Empty;
                SelectedRepeatList.Name = string.Empty;
                SelectedRepeatList.PayloadItems.Clear();
                var i = Parent.RepeatLists.First(x => x == SelectedRepeatList);
                Parent.RepeatLists.Remove(i);
                Parent.SelectedRepeatList = null;
            });
        }

        public CommandBase SaveRepeatListCommand => CommandsFactory.CommandBaseGet<CommandBase>(SaveRepeatList, c => !ApplicationModel.IsBusy && SelectedRepeatList != null);
        private void SaveRepeatList(object obj)
        {
            ApplicationModel.FolderBrowserDialogViewModel.Title = "Choose folder";
            if (((IDialog)ApplicationModel.FolderBrowserDialogViewModel).ShowDialog() &&
                !string.IsNullOrEmpty(ApplicationModel.FolderBrowserDialogViewModel.FolderPath) && Tools.FolderHasAccess(ApplicationModel.FolderBrowserDialogViewModel.FolderPath))
            {
                var list = SelectedRepeatList.PayloadItems.Select(i => i.Item);
                string fileName;
                string fullPath;
                var index = 0;
                do
                {
                    fileName = (index == 0 ? SelectedRepeatList.Name : $"{SelectedRepeatList.Name} ({index})");
                    fullPath = Path.Combine(ApplicationModel.FolderBrowserDialogViewModel.FolderPath, fileName + ".json");
                    if (!File.Exists(fullPath))
                        break;
                    index++;
                } while (true);
                File.WriteAllText(fullPath, JsonConvert.SerializeObject(list));
            }
        }

        public CommandBase UploadRepeatListCommand => CommandsFactory.CommandBaseGet<CommandBase>(UploadRepeatList, c => !ApplicationModel.IsBusy && SelectedRepeatList != null);
        private void UploadRepeatList(object obj)
        {
            ApplicationModel.OpenFileDialogModel.Filter = "Json files (*.json)|*.json";
            ((IDialog)ApplicationModel.OpenFileDialogModel).ShowDialog();
            if (ApplicationModel.OpenFileDialogModel.IsOk && !string.IsNullOrEmpty(ApplicationModel.OpenFileDialogModel.FileName))
            {
                var json = File.ReadAllText(ApplicationModel.OpenFileDialogModel.FileName);
                var list = JsonConvert.DeserializeObject<IEnumerable<PayloadItem>>(json).Select(i => new SelectableItem<PayloadItem>(i)).ToArray();

                if (list == null && !list.Any())
                    return;

                var index = 0;
                var fileName = Path.GetFileNameWithoutExtension(ApplicationModel.OpenFileDialogModel.FileName);
                string name;
                do
                {
                    name = (index == 0 ? fileName : $"{fileName} ({index})");
                    if (!Parent.RepeatLists.Any(i => i.Name == name))
                        break;
                    index++;
                } while (true);
                ApplicationModel.Invoke(() =>
                {
                    var newItem = ApplicationModel.SubscribeCollectionFactory.Create<ISelectableItem<PayloadItem>>(list);
                    Parent.RepeatLists.Add(new RepeatListModel { Name = name, PayloadItems = newItem });
                });
            }
        }

        public CommandBase DeleteCommandFromRlCommand => CommandsFactory.CommandBaseGet<CommandBase>(DeleteFromRepeatList,
                c => !ApplicationModel.IsBusy &&
                    SelectedRepeatList != null &&
                    SelectedRepeatList.PayloadItems != null &&
                    SelectedRepeatList.PayloadItems.Any(x => x.IsSelected));
        private void DeleteFromRepeatList(object obj)
        {
            for (int i = SelectedRepeatList.PayloadItems.Count - 1; i >= 0; i--)
            {
                if (SelectedRepeatList.PayloadItems[i].IsSelected)
                {
                    ApplicationModel.Invoke(() => SelectedRepeatList.PayloadItems.RemoveAt(i));
                }
            }
        }

        public CommandBase MoveUpCommand => CommandsFactory.CommandBaseGet<CommandBase>(MoveUpInList,
                c => !ApplicationModel.IsBusy &&
                    SelectedRepeatList != null &&
                    SelectedRepeatList.PayloadItems != null &&
                    SelectedRepeatList.PayloadItems.Count() > 1 &&
                    SelectedRepeatList.PayloadItems.Where(x => x.IsSelected).Count() == 1);
        private void MoveUpInList(object obj)
        {
            ApplicationModel.Invoke(() =>
            {
                var selected = SelectedRepeatList.PayloadItems.First(x => x.IsSelected);
                var ix = SelectedRepeatList.PayloadItems.IndexOf(selected);
                if (ix > 0)
                {
                    var prev = SelectedRepeatList.PayloadItems[ix - 1];
                    SelectedRepeatList.PayloadItems[ix - 1] = SelectedRepeatList.PayloadItems[ix];
                    SelectedRepeatList.PayloadItems[ix] = prev;
                    SelectedRepeatList.PayloadItems[ix - 1].IsSelected = true;
                }
            });
        }

        public CommandBase MoveDownCommand => CommandsFactory.CommandBaseGet<CommandBase>(MoveDownInList,
                c => !ApplicationModel.IsBusy &&
                    SelectedRepeatList != null &&
                    SelectedRepeatList.PayloadItems != null &&
                    SelectedRepeatList.PayloadItems.Count() > 1 &&
                    SelectedRepeatList.PayloadItems.Where(x => x.IsSelected).Count() == 1);
        private void MoveDownInList(object obj)
        {
            ApplicationModel.Invoke(() =>
            {
                var selected = SelectedRepeatList.PayloadItems.First(x => x.IsSelected);
                var ix = SelectedRepeatList.PayloadItems.IndexOf(selected);
                if (ix < SelectedRepeatList.PayloadItems.Count - 1)
                {
                    var next = SelectedRepeatList.PayloadItems[ix + 1];
                    SelectedRepeatList.PayloadItems[ix + 1] = SelectedRepeatList.PayloadItems[ix];
                    SelectedRepeatList.PayloadItems[ix] = next;
                    SelectedRepeatList.PayloadItems[ix + 1].IsSelected = true;
                }
            });
        }

        #endregion

    }
}
