/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class MessageDialogViewModel : DialogVMBase, IMessageDialogModel
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                Notify("Message");
            }
        }

        private bool _isDoNotShowThis;
        public bool IsDoNotShowThis
        {
            get { return _isDoNotShowThis; }
            set
            {
                _isDoNotShowThis = value;
                Notify("IsDoNotShowThis");
            }
        }

        private bool _isDoNotShowThisVersionUpdateVisible;
        public bool IsDoNotShowThisVersionUpdateVisible
        {
            get { return _isDoNotShowThisVersionUpdateVisible; }
            set
            {
                _isDoNotShowThisVersionUpdateVisible = value;
                Notify("IsDoNotShowThisVersionUpdateVisible");
            }
        }

        public MessageDialogViewModel(IApplicationModel applicationModel) : base (applicationModel)
        {
        }
    }
}
