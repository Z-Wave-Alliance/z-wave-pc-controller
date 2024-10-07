/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI;
using Utils.UI.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class SelectableItem<T> : EntityBase, ISelectableItem<T>
    {
        public Action OnSelectedCallback { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (OnSelectedCallback != null && value)
                    OnSelectedCallback();
                Notify("IsSelected");
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                Notify("IsEnabled");
            }
        }

        private T _item;
        public T Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Notify("Item");
            }
        }

        public SelectableItem(T item)
        {
            Item = item;
        }

        public override string ToString()
        {
            var obj = Item as object;
            if (obj != null)
                return obj.ToString();

            return base.ToString();
        }

        public void RefreshBinding()
        {
            Notify("Item");
        }
    }

    public class SelectableItemWithComment<T> : EntityBase, ISelectableItem<T>
    {
        public SelectableItemWithComment()
        {
            Item = default(T);
        }

        public SelectableItemWithComment(T item)
        {
            Item = item;
        }

        public Action OnSelectedCallback { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (OnSelectedCallback != null && value)
                    OnSelectedCallback();
                Notify("IsSelected");
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                Notify("IsEnabled");
            }
        }

        private T _item;
        public T Item
        {
            get { return _item; }
            set
            {
                _item = value;
                Notify("Item");
            }
        }

        public void RefreshBinding()
        {
            Notify("Item");
        }

        private string _mComment = "";
        public string Comment
        {
            get { return _mComment; }
            set
            {
                _mComment = value;
                Notify("Comment");
            }
        }

        public override string ToString()
        {
            string str;
            var obj = Item as object;
            if (obj != null)
                str = obj.ToString();

            str = base.ToString();

            if (string.IsNullOrEmpty(Comment))
            {
                return str;
            }
            return string.Format("{0} ({1})", str, Comment);
        }
    }
}
