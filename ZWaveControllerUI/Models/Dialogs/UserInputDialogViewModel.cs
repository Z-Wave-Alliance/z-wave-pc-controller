/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI;
using Utils.UI.Interfaces;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class UserInputDialogViewModel : DialogVMBase, IUserInputDialog
    {
        public IUserInput State { get; set; }
        public bool HasCancel { get; set; }

        public UserInputDialogViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            State = new UserInput();
            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;
        }
    }

    public class UserInput : EntityBase, IUserInput
    {
        public string InputData { get; set; }
        public bool IsInputDataVisible { get; set; } = false;
        public object InputOptions { get; set; }
        public bool IsInputOptionsVisible { get; set; } = false;
        public int SelectedInputOptionIndex { get; set; }
        public object SelectedInputOption { get; set; }
        private string _additionalText;
        public string AdditionalText { get { return _additionalText; } set { _additionalText = value; Notify("AdditionalText"); } }
        public bool IsAdditionalTextVisible { get; set; } = false;
        public bool IsCancelButtonVisible { get; set; }
    }
}
