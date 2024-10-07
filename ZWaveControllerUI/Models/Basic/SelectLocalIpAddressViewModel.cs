/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Enums;
using ZWaveController;

namespace ZWaveControllerUI.Models
{
    public class SelectLocalIpAddressViewModel : DialogVMBase, ISelectLocalIpAddressModel
    {
        #region Commands
        public UnsolicitedDestStartStopCommand UnsolicitedDestStartStopCommand => CommandsFactory.CommandControllerSessionGet<UnsolicitedDestStartStopCommand>();
        public UnsolicitedDestSecondaryStartStopCommand UnsolicitedDestSecondaryStartStopCommand => CommandsFactory.CommandControllerSessionGet<UnsolicitedDestSecondaryStartStopCommand>();
        public UnsolicitedDestApplyCommand UnsolicitedDestApplyCommand => CommandsFactory.CommandControllerSessionGet<UnsolicitedDestApplyCommand>();
        #endregion

        #region Properties

        private SelectLocalIpViews _currentViewType;
        public SelectLocalIpViews CurrentViewType
        {
            get => _currentViewType;
            set
            {
                _currentViewType = value;
                if (_currentViewType == SelectLocalIpViews.UnsolicitedDestination)
                {
                    Title = "Z/IP Unsolicited Destination";
                    Description = "Change Unsolicited Destination for Z/IP Gateway";
                }
            }
        }

        private bool _isListening;
        public bool IsListening
        {
            get => _isListening;
            set
            {
                _isListening = value;
                if (_isListening)
                {
                    StatusMessage = "Listening on:";
                    ActionButtonName = "Stop";
                }
                else
                {
                    StatusMessage = "Not listening";
                    ActionButtonName = "Start";
                }
                Notify("IsListening");
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                _statusMessage = value;
                Notify("StatusMessage");
            }
        }

        private string _actionButtonName;
        public string ActionButtonName
        {
            get => _actionButtonName;
            private set
            {
                _actionButtonName = value;
                Notify("ActionButtonName");
            }
        }

        private string _primaryAddress;
        public string PrimaryAddress
        {
            get => _primaryAddress;
            set
            {
                _primaryAddress = value;
                Notify("PrimaryAddress");
            }
        }

        private ushort _primaryPort;
        public ushort PrimaryPort
        {
            get => _primaryPort;
            set
            {
                _primaryPort = value;
                Notify("PrimaryPort");
            }
        }

        private bool _isSecondaryOn;
        public bool IsSecondaryOn
        {
            get => _isSecondaryOn;
            set
            {
                _isSecondaryOn = value;
                Notify("IsSecondaryOn");
            }
        }

        private ushort _secondaryPort;
        public ushort SecondaryPort
        {
            get => _secondaryPort;
            set
            {
                _secondaryPort = value;
                Notify("SecondaryPort");
            }
        }

        private string _customPrimaryAddress;
        public string CustomPrimaryAddress
        {
            get => _customPrimaryAddress;
            set
            {
                _customPrimaryAddress = value;
                Notify("CustomPrimaryAddress");
            }
        }

        private ushort _customPrimaryPort;
        public ushort CustomPrimaryPort
        {
            get => _customPrimaryPort;
            set
            {
                _customPrimaryPort = value;
                Notify("CustomPrimaryPort");
            }
        }

        //public override CommandBase CommandOk => CommandsFactory.CommandBaseGet<CommandBase>(param =>
        //{
        //    if (_currentViewType == SelectLocalIpViews.UnsolicitedDestination)
        //    {
        //        UnsolicitedDestinationApplyCommand?.Execute(param);
        //    }
        //    base.CommandOk.Execute(param);
        //}, param => base.CommandOk.CanExecute(param) &&
        //        UnsolicitedDestinationStartStopCommand != null &&
        //        UnsolicitedDestinationStartStopCommand.CanExecute(param));

        
        #endregion

        public SelectLocalIpAddressViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            DialogSettings.IsResizable = false;
            DialogSettings.IsTopmost = true;
            DialogSettings.CenterOwner = true;
        }
    }
}
