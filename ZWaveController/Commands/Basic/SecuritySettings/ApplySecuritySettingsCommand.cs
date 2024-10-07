/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.UI;
using ZWave.BasicApplication.Security;
using ZWave.CommandClasses;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Security;
using ZWaveController.Configuration;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ApplySecuritySettingsCommand : SecuritySettingsCommandBase
    {
        public ApplySecuritySettingsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return !ApplicationModel.IsBusy;
        }

        protected override void ExecuteInner(object param)
        {
            Apply();
        }

        private void Apply()
        {
            for (int i = 0; i < SecurityManagerInfo.NETWORK_KEYS_COUNT; i++)
            {
                if (_securitySettings.TestNetworkKeys[i] != null)
                {
                    if (_securitySettings.TestNetworkKeys[i].Value != null)
                    {
                        int len = _securitySettings.TestNetworkKeys[i].Value.Length;
                        if (len < 16)
                        {
                            _securitySettings.TestNetworkKeys[i].Value = _securitySettings.TestNetworkKeys[i].Value.Concat(new byte[16 - len]).ToArray();
                        }
                    }
                    else
                    {
                        _securitySettings.TestNetworkKeys[i].Value = new byte[16];
                    }
                }
            }

            ApplicationModel.ConfigurationItem.SecuritySettings.TestS0Settings = new TestS0Settings();
            ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings = new TestS2Settings();

            SetTestKeyByScheme(SecuritySchemes.S0, false);

            ApplicationModel.ConfigurationItem.SecuritySettings.IsCsaEnabled = ApplicationModel.Controller.Network.IsCsaEnabled = _securitySettings.IsClientSideAuthS2Enabled;
            ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS0 = ApplicationModel.Controller.Network.IsEnabledS0 = _securitySettings.IsEnabledSecurityS0;

            var s2Schemes = new List<SecuritySchemes>
            {
                SecuritySchemes.S2_UNAUTHENTICATED,
                SecuritySchemes.S2_AUTHENTICATED,
                SecuritySchemes.S2_ACCESS
            };
            bool isLR = false;
            for (int i = 0; i < 2; i++)
            {
                foreach (var schemeS2 in s2Schemes)
                {
                    bool isKeyChanged = SetTestKeyByScheme(schemeS2, isLR);
                    bool isSchemeEnabledChanged = false;
                    if (schemeS2 == SecuritySchemes.S2_UNAUTHENTICATED)
                    {
                        isSchemeEnabledChanged = _securitySettings.IsEnabledSecurityS2_UNAUTHENTICATED != ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED;
                        ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_UNAUTHENTICATED =
                            ApplicationModel.Controller.Network.IsEnabledS2_UNAUTHENTICATED = _securitySettings.IsEnabledSecurityS2_UNAUTHENTICATED;
                    }
                    else if (schemeS2 == SecuritySchemes.S2_AUTHENTICATED)
                    {
                        isSchemeEnabledChanged = _securitySettings.IsEnabledSecurityS2_AUTHENTICATED !=
                                                 ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED;
                        ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_AUTHENTICATED =
                            ApplicationModel.Controller.Network.IsEnabledS2_AUTHENTICATED = _securitySettings.IsEnabledSecurityS2_AUTHENTICATED;
                    }
                    else if (schemeS2 == SecuritySchemes.S2_ACCESS)
                    {
                        isSchemeEnabledChanged = _securitySettings.IsEnabledSecurityS2_ACCESS !=
                                                 ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_ACCESS;
                        ApplicationModel.ConfigurationItem.SecuritySettings.IsEnabledS2_ACCESS =
                            ApplicationModel.Controller.Network.IsEnabledS2_ACCESS = _securitySettings.IsEnabledSecurityS2_ACCESS;
                    }

                    if (isKeyChanged || isSchemeEnabledChanged)
                    {
                        ControllerSession.SecurityManager.SecurityManagerInfo.ActivateNetworkKeyS2ForNode(new InvariantPeerNodeId(ApplicationModel.Controller.Network.NodeTag, new NodeTag(255)), schemeS2, false);
                        ControllerSession.Controller.Network.SetEnabledSecuritySchemes();
                        ControllerSession.Controller.Network.ResetCurrentSecurityScheme();
                    }
                }
                isLR = true;
            }

            ApplicationModel.ConfigurationItem.SecuritySettings.TestS0Settings = XmlUtility.XmlStr2Obj<TestS0Settings>(XmlUtility.Obj2XmlStr(_securitySettings.TestS0Settings));
            ApplicationModel.ConfigurationItem.SecuritySettings.TestS0Settings.IsActive = _securitySettings.TestS0Settings.IsActive;
            ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings = XmlUtility.XmlStr2Obj<TestS2Settings>(XmlUtility.Obj2XmlStr(_securitySettings.TestS2Settings));
            ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.IsActive = _securitySettings.TestS2Settings.IsActive;
            if (_securitySettings.TestS2Settings != null &&
                _securitySettings.TestS2Settings.Extensions != null &&
                ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings != null &&
                ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions != null)
            {
                for (int i = 0; i < _securitySettings.TestS2Settings.Extensions.Count; i++)
                {
                    _securitySettings.TestS2Settings.Extensions[i].Counter = new ValueEntity<int>();
                    if (i < ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions.Count)
                    {
                        ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions[i].Counter = _securitySettings.TestS2Settings.Extensions[i].Counter;
                    }
                }
            }

            //ApplicationModel.ConfigurationItem.ViewSettings.SecurityView.IsTabS0Selected = _securitySettings.IsS0TabSelected;

            UpdateControllerConfiguration();

            ControllerSessionsContainer.Config.ControllerConfiguration.IsSaveKeys = _securitySettings.IsSaveKeys;
            if(!string.IsNullOrEmpty(_securitySettings.KeysStorageFolder))
                ControllerSessionsContainer.Config.ControllerConfiguration.KeysStorageFolder = _securitySettings.KeysStorageFolder;
            ControllerSession.SaveSecurityKeys(true);
            ControllerSession.SetupSecurityManagerInfo(ApplicationModel.ConfigurationItem.SecuritySettings);
            if (_securitySettings.IsPauseSecurity)
            {
                ControllerSession.SecurityManager.Suspend();
            }
            else
            {
                ControllerSession.SecurityManager.Resume();
            }
            ApplicationModel.ConfigurationItem.RefreshNodes();
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }

        private bool SetTestKeyByScheme(SecuritySchemes scheme, bool isLongRange)
        {
            bool isValueChanged = false;
            var keyIndex = SecurityManagerInfo.GetNetworkKeyIndex(scheme, isLongRange);
            var prevValue = ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys[keyIndex].TestValue;

            ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys[keyIndex].TestValue = 
                _securitySettings.TestNetworkKeys[keyIndex].IsSet ?
                _securitySettings.TestNetworkKeys[keyIndex].Value : 
                null;

            if ((prevValue != null && ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys[keyIndex].TestValue != null
                && !prevValue.SequenceEqual(ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys[keyIndex].TestValue))
                || prevValue != ApplicationModel.ConfigurationItem.SecuritySettings.NetworkKeys[keyIndex].TestValue)
            {
                isValueChanged = true;
            }

            return isValueChanged;
        }

        private void UpdateControllerConfiguration()
        {
            try
            {
                var commandClasses = ApplicationModel.Controller.Network.GetCommandClasses(ApplicationModel.Controller.Network.NodeTag);
                var sett = ApplicationModel.ConfigurationItem.SecuritySettings;
                commandClasses = sett.IsEnabledS0 ?
                    commandClasses.Concat(new byte[] { COMMAND_CLASS_SECURITY.ID }).Distinct().ToArray() :
                    commandClasses.Where(x => x != COMMAND_CLASS_SECURITY.ID).ToArray();

                commandClasses = (sett.IsEnabledS2_UNAUTHENTICATED || sett.IsEnabledS2_AUTHENTICATED || sett.IsEnabledS2_ACCESS) ?
                    commandClasses.Concat(new byte[] { COMMAND_CLASS_SECURITY_2.ID }).Distinct().ToArray() :
                    commandClasses.Where(x => x != COMMAND_CLASS_SECURITY_2.ID).ToArray();
                ApplicationModel.NotifyControllerChanged(NotifyProperty.ControllerInfo | NotifyProperty.NodesList);

                ApplicationModel.Controller.Network.SetCommandClasses(commandClasses);
                ControllerSession.SetNodeInformation(out var token);
            }
            catch (Exception ex)
            {
                ex.Message._DLOG();
#if DEBUG
                throw ex;
#endif
            }
        }
    }
}
