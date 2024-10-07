/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using ZWave.CommandClasses;
using ZWaveController.Interfaces;

namespace ZWaveController.Services
{
    public class NodeInformationService : INodeInformationService
    {
        private IApplicationModel _applicationModel;
        private static byte[] _defaultCommandClasses =
        {
            // According SDS11847:
            COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID,
            COMMAND_CLASS_VERSION.ID,
            COMMAND_CLASS_MANUFACTURER_SPECIFIC.ID,
            COMMAND_CLASS_POWERLEVEL.ID,
            COMMAND_CLASS_ASSOCIATION.ID,
            COMMAND_CLASS_ASSOCIATION_GRP_INFO.ID,
            COMMAND_CLASS_CRC_16_ENCAP.ID,
            COMMAND_CLASS_DEVICE_RESET_LOCALLY.ID,
            COMMAND_CLASS_SECURITY.ID,
            COMMAND_CLASS_SECURITY_2.ID,
            COMMAND_CLASS_SUPERVISION.ID,
            COMMAND_CLASS_TRANSPORT_SERVICE_V2.ID,
            COMMAND_CLASS_INCLUSION_CONTROLLER.ID,
            COMMAND_CLASS_TIME_V2.ID, //DT:31.11.0004.1

            // Supported but not mandatory:
            COMMAND_CLASS_APPLICATION_STATUS.ID,
            COMMAND_CLASS_CONFIGURATION_V3.ID,
            COMMAND_CLASS_FIRMWARE_UPDATE_MD_V5.ID,
            COMMAND_CLASS_MULTI_CHANNEL_V4.ID,
            COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID,
            COMMAND_CLASS_WAKE_UP_V2.ID,
            COMMAND_CLASS_TIME_PARAMETERS.ID
        };
        /// <summary>
        /// Mandatory Command Classes defined in document SDS11847.
        /// </summary>
        public static IReadOnlyList<byte> DefaultCommandClasses => Array.AsReadOnly(_defaultCommandClasses);

        public NodeInformationService(IApplicationModel applicationModel)
        {
            _applicationModel = applicationModel;
        }

        public byte[] GetDefaultCommandClasses()
        {
            var notMandatory = new byte[]
            {
                COMMAND_CLASS_MULTI_CHANNEL_V4.ID,
                COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID,
                COMMAND_CLASS_WAKE_UP_V2.ID,
                COMMAND_CLASS_TIME_PARAMETERS.ID,
            };
            var commandClasses = _applicationModel.ZWaveDefinition.CommandClasses.Where(x => x.KeyId >= 0x20);
            var defaultCcs = commandClasses.Where(cc => !notMandatory.Contains(cc.KeyId) && _defaultCommandClasses.Contains(cc.KeyId)).
                    Select(cc => cc.KeyId).
                    ToList();
            if (_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS0)
                defaultCcs.Add(COMMAND_CLASS_SECURITY.ID);
            if (_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED ||
                _applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED ||
                _applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_ACCESS)
                defaultCcs.Add(COMMAND_CLASS_SECURITY_2.ID);
            return defaultCcs.Distinct().OrderBy(x => x != COMMAND_CLASS_ZWAVEPLUS_INFO_V2.ID).ToArray();
        }

        public byte[] GetSelectedCommandClasses()
        {
            var ret = new List<byte>(_defaultCommandClasses);
            if (!_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS0)
            {
                ret.Remove(COMMAND_CLASS_SECURITY.ID);
            }
            if (!_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED &&
                !_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED &&
                !_applicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_ACCESS)
            {
                ret.Remove(COMMAND_CLASS_SECURITY_2.ID);
            }
            ret.Remove(COMMAND_CLASS_MULTI_CHANNEL_V4.ID);
            ret.Remove(COMMAND_CLASS_MULTI_CHANNEL_ASSOCIATION_V3.ID);
            ret.Remove(COMMAND_CLASS_WAKE_UP_V2.ID);
            return ret.ToArray();
        }
    }
}
