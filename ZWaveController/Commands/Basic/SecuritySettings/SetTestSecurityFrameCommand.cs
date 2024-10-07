/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetTestSecurityFrameCommand : SecuritySettingsCommandBase
    {
        public SetTestSecurityFrameCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        protected override void ExecuteInner(object param)
        {
            var frame = _securitySettings.TestS2Settings.Frames.FirstOrDefault(x => x.FrameTypeV == _securitySettings.ActiveTestFrameIndex);

            if (_securitySettings.TestFrameCommand.IsSet && _securitySettings.TestFrameCommand.Value != null && _securitySettings.TestFrameCommand.Value.Length > 0)
            {
                if (frame == null)
                {
                    frame = new TestFrameS2Settings
                    {
                        FrameTypeV =_securitySettings.ActiveTestFrameIndex,
                        Command = _securitySettings.TestFrameCommand.Value
                    };
                    _securitySettings.TestS2Settings.Frames.Add(frame);
                }
                else
                {
                    frame.Command = _securitySettings.TestFrameCommand.Value;
                }
            }
            else
            {
                if (frame != null)
                {
                    frame.Command = null;
                }
            }

            if (_securitySettings.TestFrameDelay.IsSet && _securitySettings.TestFrameDelay.Value > 0)
            {
                if (frame == null)
                {
                    frame = new TestFrameS2Settings
                    {
                        FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                        Delay = _securitySettings.TestFrameDelay.Value,
                        DelaySpecified = true
                    };
                    _securitySettings.TestS2Settings.Frames.Add(frame);
                }
                else
                {
                    frame.Delay = _securitySettings.TestFrameDelay.Value;
                    frame.DelaySpecified = true;
                }
            }
            else
            {
                if (frame != null)
                {
                    frame.Delay = 0;
                    frame.DelaySpecified = false;
                }
            }

            if (_securitySettings.TestFrameIsMulticast.IsSet)
            {
                if (frame == null)
                {
                    frame = new TestFrameS2Settings
                    {
                        FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                        IsMulticast = _securitySettings.TestFrameIsMulticast.Value,
                        IsMulticastSpecified = true
                    };
                    _securitySettings.TestS2Settings.Frames.Add(frame);
                }
                else
                {
                    frame.IsMulticast = _securitySettings.TestFrameIsMulticast.Value;
                    frame.IsMulticastSpecified = true;
                }
            }
            else
            {
                if (frame != null)
                {
                    frame.IsMulticast = false;
                    frame.IsMulticastSpecified = false;
                }
            }

            if (_securitySettings.TestFrameIsBroadcast.IsSet)
            {
                if (frame == null)
                {
                    frame = new TestFrameS2Settings
                    {
                        FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                        IsBroadcast = _securitySettings.TestFrameIsBroadcast.Value,
                        IsBroadcastSpecified = true
                    };
                    _securitySettings.TestS2Settings.Frames.Add(frame);
                }
                else
                {
                    frame.IsBroadcast = _securitySettings.TestFrameIsBroadcast.Value;
                    frame.IsBroadcastSpecified = true;
                }
            }
            else
            {
                if (frame != null)
                {
                    frame.IsBroadcast = false;
                    frame.IsBroadcastSpecified = false;
                }
            }

            if (_securitySettings.ActiveTestFrameIndex != SecurityS2TestFrames.NonceGet)
            {
                if (_securitySettings.TestFrameIsEncrypted.IsSet)
                {
                    if (frame == null)
                    {
                        frame = new TestFrameS2Settings
                        {
                            FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                            IsEncrypted = _securitySettings.TestFrameIsEncrypted.Value,
                            IsEncryptedSpecified = true
                        };
                        _securitySettings.TestS2Settings.Frames.Add(frame);
                    }
                    else
                    {
                        frame.IsEncrypted = _securitySettings.TestFrameIsEncrypted.Value;
                        frame.IsEncryptedSpecified = true;
                    }
                }
                else
                {
                    if (frame != null)
                    {
                        frame.IsEncrypted = false;
                        frame.IsEncryptedSpecified = false;
                    }
                }

                if (_securitySettings.TestFrameNetworkKey.IsSet && _securitySettings.TestFrameNetworkKey.Value != null && _securitySettings.TestFrameNetworkKey.Value.Length > 0)
                {
                    if (frame == null)
                    {
                        frame = new TestFrameS2Settings
                        {
                            FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                            NetworkKey = _securitySettings.TestFrameNetworkKey.Value
                        };
                        _securitySettings.TestS2Settings.Frames.Add(frame);
                    }
                    else
                    {
                        frame.NetworkKey = _securitySettings.TestFrameNetworkKey.Value;
                    }
                }
                else
                {
                    if (frame != null)
                    {
                        frame.NetworkKey = null;
                    }
                }

                if (_securitySettings.TestFrameIsTemp.IsSet)
                {
                    bool isTemp = _securitySettings.TestFrameIsTemp.Value;
                    if (frame == null)
                    {
                        frame = new TestFrameS2Settings
                        {
                            FrameTypeV = _securitySettings.ActiveTestFrameIndex,
                            IsTemp = isTemp,
                            IsTempSpecified = true
                        };
                        _securitySettings.TestS2Settings.Frames.Add(frame);
                    }
                    else
                    {
                        frame.IsTemp = isTemp;
                        frame.IsTempSpecified = true;
                    }
                }
                else
                {
                    if (frame != null)
                    {
                        frame.IsTemp = false;
                        frame.IsTempSpecified = false;
                    }
                }
            }
            ApplicationModel.LastCommandExecutionResult = CommandExecutionResult.OK;
        }

    }
}
