/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
/// SPDX-FileCopyrightText: Z-Wave Alliance https://z-wavealliance.org
using System.Linq;
using ZWave.Xml.Application;
using System;
using ZWave.CommandClasses;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController;

namespace ZWaveControllerUI.Models
{
    public class SelectCommandViewModel : DialogVMBase
    {
        private ICommandClassesModel Model { get; set; }
        public override CommandBase CommandOk
        {
            get
            {
                return CommandsFactory.CommandBaseGet<CommandBase>(param =>
                {
                    if (SelectedItem is Command)
                    {
                        base.CommandOk.Execute(null);
                        ApplicationModel.Invoke(() => ((CommandClassesViewModel)Model).SelectedCommand = SelectedItem as Command);
                    }
                },
                param => (SelectedItem is Command));
            }
        }

        public SelectCommandViewModel(IApplicationModel applicationModel, ICommandClassesModel model) : base(applicationModel)
        {
            DialogSettings.IsResizable = true;
            Title = "Select Command";
            Model = model;
        }

        private bool _isShowAllCommandClasses;
        public bool IsShowAllCommandClasses
        {
            get { return _isShowAllCommandClasses; }
            set
            {
                _isShowAllCommandClasses = value;
                UpdateDescription();
                Notify("IsShowAllCommandClasses");
                Notify("CommandClasses");
            }
        }

        private string _filterCC;
        public string FilterCC
        {
            get { return _filterCC; }
            set
            {
                _filterCC = value;
                Notify("FilterCC");
            }
        }
        private void UpdateDescription()
        {
            if (_isShowAllCommandClasses)
            {
                Description = "List of all Command Classes";
            }
            else if (CommandClasses != null && CommandClasses.Length > 0)
            {
                Description = "List of supported Command Classes";
            }
            else
            {
                Description = "Supported Command Classes list is empty for Selected Node. Execute 'Request Node Info' command or select 'All Command Classes'";
            }
        }

        private byte[] _commandClasses;
        public byte[] CommandClasses
        {
            get { return IsShowAllCommandClasses ? null : _commandClasses; }
            set
            {
                if (value == null)
                {
                    _commandClasses = new byte[] { COMMAND_CLASS_BASIC.ID };
                }
                else
                {
                    if (value.Contains(COMMAND_CLASS_BASIC.ID))
                    {
                        _commandClasses = new byte[value.Length];
                        Array.Copy(value, _commandClasses, value.Length);
                    }
                    else
                    {
                        // Adding Basic CC as it is not advertised by supporting nodes
                        _commandClasses = new byte[value.Length + 1];
                        _commandClasses[0] = COMMAND_CLASS_BASIC.ID;
                        Array.Copy(value, 0, _commandClasses, 1, value.Length);
                    }
                }
                UpdateDescription();
                Notify("CommandClasses");
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                Notify("SelectedItem");
            }
        }
    }
}
