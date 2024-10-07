/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.Layers;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class AddSocketSourceViewModel : DialogVMBase, IAddSocketSourceModel
    {
        private SoketSourceTypes _selectedSourceType = SoketSourceTypes.ZIP;
        public SoketSourceTypes SelectedSourceType
        {
            get { return _selectedSourceType; }
            set
            {
                _selectedSourceType = value;
                switch (_selectedSourceType)
                {
                    case SoketSourceTypes.ZIP:
                        Args = ZWave.ZipApplication.Constants.DefaultPsk;
                        break;
                    case SoketSourceTypes.TCP:
                        Args = "";
                        break;
                    default:
                        break;
                }
                Notify("SelectedSourceType");
            }
        }

        private string _ipAddress;
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                Notify("IPAddress");
            }
        }

        private int _portNo;
        public int PortNo
        {
            get { return _portNo; }
            set
            {
                _portNo = value;
                Notify("PortNo");
            }
        }

        private string _args = ZWave.ZipApplication.Constants.DefaultPsk;
        public string Args
        {
            get { return _args; }
            set
            {
                _args = value;
                Notify("Args");
            }
        }

        #region Commands

        public new CommandBase CommandCancel => CommandsFactory.CommandBaseGet<CommandBase>(param => CancelCommandExecute(param));
        public AddSocketSourceCommand AddSocketSourceCommand => CommandsFactory.CommandSourcesGet<AddSocketSourceCommand>(ApplicationModel, ApplicationModel.ConnectModel.DataSources);
        public override CommandBase CommandOk => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        {
            AddSocketSourceCommand.Execute(null);
            PortNo = 0;
            IPAddress = null;
            base.CommandOk.Execute(param);
        }, obj => base.CommandOk.CanExecute(obj) && AddSocketSourceCommand.CanExecute(obj) && !string.IsNullOrEmpty(IPAddress) && PortNo > 0);

        #endregion

        public AddSocketSourceViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            SelectedSourceType = SoketSourceTypes.ZIP;
        }

        private void CancelCommandExecute(object obj)
        {
            IsOk = false;
            PortNo = 0;
            IPAddress = null;
            Close();
        }
    }
}
