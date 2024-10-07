/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils.UI.Interfaces;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveControllerUI.Models
{
    public class SenderHistoryViewModel : DialogVMBase, ISenderHistoryModel
    {
        public SenderHistoryViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Recents Commands";
            //DialogSettings.IsResizable = true;
            //DialogSettings.Width = 400;
            //DialogSettings.Height = 300;
        }

        #region Commands
        public DeleteHistoryItemsCommand DeleteHistoryItemsCommand => CommandsFactory.CommandControllerSessionGet<DeleteHistoryItemsCommand>();

        public CommandBase EditHistoryItemCommand => CommandsFactory.CommandBaseGet<CommandBase>(param => EditHistoryItem(param), c => SelectedHistoryRecord != null);
        private void EditHistoryItem(object param)
        {
            var payloadItem = SelectedHistoryRecord.Item.PayloadItem;
            ApplicationModel.Invoke(() =>
            {
                ((CommandClassesViewModel)ApplicationModel.CommandClassesModel).FillCommandClassesViewModel(payloadItem);
            });
        }
        #endregion

        #region ISenderHistoryModel
        private IList<ISelectableItem<PayloadHistoryItem>> _history;
        public IList<ISelectableItem<PayloadHistoryItem>> History
        {
            get
            {
                return _history ?? (_history = ApplicationModel.SubscribeCollectionFactory.Create<ISelectableItem<PayloadHistoryItem>>());
            }
            set
            {
                _history = value;
                Notify("History");
                Notify("AvailableDates");
            }
        }

        public ISelectableItem<PayloadHistoryItem> GetPayloadHistoryItem(PayloadItem payloadItem)
        {
            var item = new PayloadHistoryItem(History.Any() ? History.Max(i => i.Item.Id) + 1 : 1, DateTime.Now)
            {
                PayloadItem = payloadItem
            };
            return new SelectableItem<PayloadHistoryItem>(item);
        }

        public void UpdatedFilters()
        {
            Notify("AvailableDates");
            SelectedDate = AvailableDates.Count() > 0 ? AvailableDates.First() : DateTime.Today;
        }
        #endregion

        private ISelectableItem<PayloadHistoryItem> _selectedHistoryRecord { get; set; }
        public ISelectableItem<PayloadHistoryItem> SelectedHistoryRecord
        {
            get
            {
                return _selectedHistoryRecord;
            }
            set
            {
                _selectedHistoryRecord = value;
                Notify("SelectedHistoryRecord");
                if (SelectedHistoryRecord?.Item?.PayloadItem != null)
                {
                    var dt = SelectedHistoryRecord.Item.PayloadItem.Payload;
                    var res = ((CommandClassesViewModel)ApplicationModel.CommandClassesModel).
                        FillSelectedCommandDetails(dt);
                    SelectedItemDetails = res;
                }
                else
                {
                    SelectedItemDetails = string.Empty;
                }
            }
        }

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

        public DateTime[] AvailableDates =>
            History.OrderByDescending(x => x.Item.Timestamp).Select(x => x.Item.Timestamp.Date).Distinct().ToArray();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                Notify("SelectedDate");
                Notify("FilteredHistory");
            }
        }
        public IList<ISelectableItem<PayloadHistoryItem>> FilteredHistory =>
            ApplicationModel.SubscribeCollectionFactory.Create(History.Where(x => x.Item.Timestamp.Date == _selectedDate.Date).OrderByDescending(x => x.Item.Timestamp));
    }
}