/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Utils;
using Utils.UI.Interfaces;
using ZWave.Xml.Application;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Services
{
    public class SenderHistoryService : ISenderHistoryService
    {
        const int MAX_HISTORY_LENGTH = 25;
        const string STORE_EXTENSION = ".json";
        const string SEARCH_PATTERN = "_history";
        private readonly string _folder;
        private IApplicationModel _applicationModel;

        public SenderHistoryService(IApplicationModel applicationModel)
        {
            _applicationModel = applicationModel;
            _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Silicon Labs", "Z-Wave PC Controller 5", "History");
        }

        public void Load()
        {
            //var filePath = GetFileFullPath();
            //if (File.Exists(filePath))
            //{
            //    var historyItems = JsonConvert.DeserializeObject<List<PayloadHistoryItem>>(File.ReadAllText(filePath));
            //    _applicationModel.Invoke(() => {
            //        _applicationModel.SenderHistoryModel.History.AddRange(historyItems.Select(x => _applicationModel.CreateSelectableItem<PayloadHistoryItem>(x)));
            //        _applicationModel.SenderHistoryModel.UpdatedFilters();
            //    });
            //}
        }

        public void Save()
        {
            //if (!Directory.Exists(_folder))
            //    Directory.CreateDirectory(_folder);

            //File.WriteAllText(GetFileFullPath(), JsonConvert.SerializeObject(_applicationModel.SenderHistoryModel.History.Select(x => x.Item)));
        }

        public IList<ISelectableItem<PayloadHistoryItem>> GetFilteredHistory(DateTime? date = null)
        {
            return date.HasValue
                ? _applicationModel.SenderHistoryModel.History.Where(i => i.Item.Timestamp.Date == date.Value.Date).ToList()
                : _applicationModel.SenderHistoryModel.History;
        }

        public void Add(PayloadItem itemToStore)
        {
            var newRecord = _applicationModel.SenderHistoryModel.GetPayloadHistoryItem(itemToStore);
            _applicationModel.Invoke(() =>
            {
                _applicationModel.SenderHistoryModel.History.Add(newRecord);
                var historyCount = _applicationModel.SenderHistoryModel.History.Count;
                if (historyCount > MAX_HISTORY_LENGTH)
                {
                    var temp = _applicationModel.SenderHistoryModel.History.Skip(historyCount - MAX_HISTORY_LENGTH).ToList();
                    _applicationModel.SenderHistoryModel.History.Clear();
                    _applicationModel.SenderHistoryModel.History.AddRange(temp);
                }
                _applicationModel.SenderHistoryModel.UpdatedFilters();
            });
            Save();
        }

        public void Add(byte[] data) //move to CC view model?
        {
            var commandClass = _applicationModel.ZWaveDefinition.FindCommandClasses(data[0]);
            var payloadItem = new PayloadItem
            {
                Payload = data,
                CommandName = "Unknown - no cmd",
                ClassId = data[0],
                SecureType = Enums.SecureType.DefaultSecurity,
                Version = commandClass.Max(x => x.Version)
            };

            if (commandClass != null && data.Length > 1)
            {
                var command = _applicationModel.ZWaveDefinition.FindCommand(commandClass.OrderByDescending(i => i.Version).Last(), data[1]);
                if (command != null)
                {
                    payloadItem.CommandName = command.Text;
                    payloadItem.ClassId = command.Parent.KeyId;
                    payloadItem.CommandId = command.KeyId;
                }
            }

            Add(payloadItem);
        }

        public void Delete(uint id)
        {
            _applicationModel.Invoke(() =>
            {
                var item = _applicationModel.SenderHistoryModel.History.FirstOrDefault(i => i.Item.Id == id);
                if (item != null)
                    _applicationModel.SenderHistoryModel.History.Remove(item);
            });
            Save();
        }

        public void Delete(List<uint> ids)
        {
            _applicationModel.Invoke(() =>
            {
                ids.ForEach(id =>
                {
                    var item = _applicationModel.SenderHistoryModel.History.FirstOrDefault(i => i.Item.Id == id);
                    if (item != null)
                        _applicationModel.SenderHistoryModel.History.Remove(item);
                });
            });
            Save();
        }

        public void Clear()
        {
            _applicationModel.Invoke(() => _applicationModel.SenderHistoryModel.History.Clear());
            Save();
        }

        private string GetFileFullPath()
        {
            return Path.Combine(_folder, $"{_applicationModel.DataSource.SourceName}{SEARCH_PATTERN}{STORE_EXTENSION}");
        }

        //public PayloadItem GetPayloadItem(ICommandClassesModel model)
        //{
        //    var commandClass = ApplicationModel.ZWaveDefinition.FindCommandClasses(model.Payload[0]);
        //    Command command = null;
        //    if (commandClass != null && commandClass.Count > 0 && model.Payload.Length > 1)
        //    {
        //        command = ApplicationModel.ZWaveDefinition.FindCommand(commandClass.OrderByDescending(x => x.Version).First(), model.Payload[1]);
        //    }
        //    var name = "Unknown - no cmd";
        //    var classId = model.Payload[0];
        //    byte commId = 0;
        //    byte ccVersion = 0;
        //    if (command != null)
        //    {
        //        name = command.Text;
        //        classId = command.Parent.KeyId;
        //        commId = command.KeyId;
        //        ccVersion = commandClass.Max(x => x.Version);
        //    }
        //    var ret = new PayloadItem {
        //        Payload = model.Payload,
        //        CommandName = name,
        //        ClassId = classId,
        //        CommandId = commId,
        //        Version = ccVersion,

        //        IsCrc16Enabled = model.IsCrc16Enabled,
        //        IsSuppressMulticastFollowUp = model.IsSuppressMulticastFollowUp,
        //        IsForceMulticastEnabled = model.IsForceMulticastEnabled,
        //        IsSupervisionGetEnabled = model.IsSupervisionGetEnabled,
        //        SupervisionSessionId = model.SupervisionSessionId,
        //        IsSupervisionGetStatusUpdatesEnabled = model.IsSupervisionGetStatusUpdatesEnabled,
        //        IsAutoIncSupervisionSessionId = model.IsAutoIncSupervisionSessionId,
        //        SecureType = model.SecureType,
        //        IsMultiChannelEnabled = model.IsMultiChannelEnabled,
        //        IsBitAddress = model.IsBitAddress,
        //    };
        //    return ret;
        //}
    }
}
