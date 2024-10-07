/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class VMBase : EntityBase, ICloneable, IDialog
    {
        public MainViewModel ApplicationModel { get; set; }
        public bool IsOk { get; set; }
        public Action CloseCallback { get; set; }
        public static Action<IDialog> ShowDialogAction { get; set; }

        public VMBase(IApplicationModel applicationModel)
        {
            ApplicationModel = applicationModel as MainViewModel;
        }

        public void Close()
        {
            CloseCallback?.Invoke();
        }

        public virtual bool ShowDialog()
        {
            return IsOk;
        }

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

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                Notify("Description");
            }
        }

        private int _tag;
        public int Tag
        {
            get { return _tag; }
            set
            {
                _tag = value;
                Notify("Tag");
            }
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
