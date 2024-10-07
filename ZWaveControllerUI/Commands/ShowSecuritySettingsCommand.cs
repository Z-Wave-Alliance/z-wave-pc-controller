/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils;
using Utils.UI.Interfaces;
using ZWave.BasicApplication.Devices;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Commands
{
    public class ShowSecuritySettingsCommand : ControllerSessionCommandBase
    {
        public ShowSecuritySettingsCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Show Security Settings command";
            IsModelBusy = false;
        }

        protected override void ExecuteInner(object param)
        {
            ApplicationModel.SecuritySettingsModel.IsClientSideAuthS2Enabled = ControllerSession.Controller.Network.IsCsaEnabled;
            ApplicationModel.SecuritySettingsModel.IsEnabledSecurityS0 = ControllerSession.Controller.Network.IsEnabledS0;
            ApplicationModel.SecuritySettingsModel.IsEnabledSecurityS2_ACCESS = ControllerSession.Controller.Network.IsEnabledS2_ACCESS;
            ApplicationModel.SecuritySettingsModel.IsEnabledSecurityS2_AUTHENTICATED = ControllerSession.Controller.Network.IsEnabledS2_AUTHENTICATED;
            ApplicationModel.SecuritySettingsModel.IsEnabledSecurityS2_UNAUTHENTICATED = ControllerSession.Controller.Network.IsEnabledS2_UNAUTHENTICATED;

            var tmp = XmlUtility.XmlStr2Obj<TestS2Settings>(XmlUtility.Obj2XmlStr(ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings));
            ApplicationModel.Invoke(() =>
            {
                ApplicationModel.SecuritySettingsModel.TestS2Settings.IsActive = ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.IsActive;
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Parameters.Clear();
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Parameters.AddRange(tmp.Parameters);
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Frames.Clear();
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Frames.AddRange(tmp.Frames);
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Extensions.Clear();
                ApplicationModel.SecuritySettingsModel.TestS2Settings.Extensions.AddRange(tmp.Extensions);
                if (ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings != null &&
                    ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions != null &&
                    ApplicationModel.SecuritySettingsModel.TestS2Settings != null &&
                    ApplicationModel.SecuritySettingsModel.TestS2Settings.Extensions != null)
                {
                    for (int i = 0; i < ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions.Count; i++)
                    {
                        if (i < ApplicationModel.SecuritySettingsModel.TestS2Settings.Extensions.Count)
                        {
                            ApplicationModel.SecuritySettingsModel.TestS2Settings.Extensions[i].Counter = ApplicationModel.ConfigurationItem.SecuritySettings.TestS2Settings.Extensions[i].Counter;
                        }
                    }
                }
            });

            ApplicationModel.SecuritySettingsModel.IsSaveKeys = ControllerSessionsContainer.Config.ControllerConfiguration.IsSaveKeys;
            ApplicationModel.SecuritySettingsModel.KeysStorageFolder = ControllerSessionsContainer.Config.ControllerConfiguration.KeysStorageFolder;
            ApplicationModel.SecuritySettingsModel.NetworkKeyTemp = ControllerSession.SecurityManager.SecurityManagerInfo.GetActualNetworkKeyS2Temp();
            ((IDialog)ApplicationModel.SecuritySettingsModel).ShowDialog();
        }

        public override bool CanExecuteModelDependent(object param)
        {
            var supportedDevice = (ControllerSession.Controller is Controller) ||
                !ControllerSession.IsEndDeviceLibrary || 
                (ControllerSession.Controller as Device).Library == ZWave.Enums.Libraries.EndDeviceSysTestLib;
            return !ApplicationModel.IsActiveSessionZip && supportedDevice;
        }
    }
}
