/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class AddNodeWithCustomSettingsViewModel : DialogVMBase, IAddNodeWithCustomSettingsModel
    {
        private bool _isDeleteReturnRoute = true;
        public bool IsDeleteReturnRoute
        {
            get => _isDeleteReturnRoute;
            set
            {
                _isDeleteReturnRoute = value;
                Notify("IsDeleteReturnRoute");
            }
        }

        private bool _isAssignReturnRoute = true;
        public bool IsAssignReturnRoute
        {
            get => _isAssignReturnRoute;
            set
            {
                _isAssignReturnRoute = value;
                Notify("IsAssignReturnRoute");
            }
        }

        private bool _isAssociationCreate = true;
        public bool IsAssociationCreate
        {
            get => _isAssociationCreate;
            set
            {
                _isAssociationCreate = value;
                Notify("IsAssociationCreate");
            }
        }

        private bool _isMultichannelAssociationCreate = true;
        public bool IsMultichannelAssociationCreate
        {
            get => _isMultichannelAssociationCreate;
            set
            {
                _isMultichannelAssociationCreate = value;
                Notify("IsMultichannelAssociationCreate");
            }
        }

        private bool _isWakeUpCapabilities = true;
        public bool IsWakeUpCapabilities
        {
            get => _isWakeUpCapabilities;
            set
            {
                _isWakeUpCapabilities = value;
                Notify("IsWakeUpCapabilities");
            }
        }

        private bool _isWakeUpInterval = true;
        public bool IsWakeUpInterval
        {
            get => _isWakeUpInterval;
            set
            {
                _isWakeUpInterval = value;
                Notify("IsWakeUpInterval");
            }
        }

        private bool _isSetAsSisAutomatically = true;
        public bool IsSetAsSisAutomatically
        {
            get => _isSetAsSisAutomatically;
            set
            {
                _isSetAsSisAutomatically = value;
                Notify("IsSetAsSisAutomatically");
            }
        }

        private bool _isBasedOnZwpRoleType;
        public bool IsBasedOnZwpRoleType
        {
            get => _isBasedOnZwpRoleType;
            set
            {
                _isBasedOnZwpRoleType = value;
                Notify("IsBasedOnZwpRoleType");
            }
        }

        public SetupNodeLifelineSettings GetSetupNodeLifelineSettings()
        {
            var setupNodeLifelineSettings = SetupNodeLifelineSettings.Default;
            setupNodeLifelineSettings |= _isDeleteReturnRoute ? SetupNodeLifelineSettings.IsDeleteReturnRoute : 0;
            setupNodeLifelineSettings |= _isAssignReturnRoute ? SetupNodeLifelineSettings.IsAssignReturnRoute : 0;
            setupNodeLifelineSettings |= _isAssociationCreate ? SetupNodeLifelineSettings.IsAssociationCreate : 0;
            setupNodeLifelineSettings |= _isMultichannelAssociationCreate ? SetupNodeLifelineSettings.IsMultichannelAssociationCreate : 0;
            setupNodeLifelineSettings |= _isWakeUpCapabilities ? SetupNodeLifelineSettings.IsWakeUpCapabilities : 0;
            setupNodeLifelineSettings |= _isWakeUpInterval ? SetupNodeLifelineSettings.IsWakeUpInterval : 0;
            setupNodeLifelineSettings |= _isSetAsSisAutomatically ? SetupNodeLifelineSettings.IsSetAsSisAutomatically : 0;
            setupNodeLifelineSettings |= _isSetAsSisAutomatically && _isBasedOnZwpRoleType ? SetupNodeLifelineSettings.IsBasedOnZwpRoleType : 0;
            // If nothing is checked then SetupNodeLifelineSettings.SkipAllSteps will be applied
            if (setupNodeLifelineSettings == SetupNodeLifelineSettings.Default)
            {
                setupNodeLifelineSettings = SetupNodeLifelineSettings.SkipAllSteps;
            }
            return setupNodeLifelineSettings;
        }

        public AddNodeWithCustomSettingsViewModel(IApplicationModel applicationModel)
            : base(applicationModel)
        {
            Title = "Add node with custom settings";
            Description = "Configure automatic node setup.";
            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;

            _isDeleteReturnRoute = true;
            _isAssignReturnRoute = true;
            _isAssociationCreate = true;
            _isMultichannelAssociationCreate = true;
            _isWakeUpCapabilities = true;
            _isWakeUpInterval = true;
            _isSetAsSisAutomatically = true;
            _isBasedOnZwpRoleType = true;
        }
    }
}
