/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class SetTestSecurityParameterCommand : SecuritySettingsCommandBase
    {
        public SetTestSecurityParameterCommand(IControllerSession controllerSession) : base(controllerSession)
        {
        }

        protected override void ExecuteInner(object param)
        {
            var testParam = _securitySettings.TestS2Settings.Parameters.FirstOrDefault(val => val.ParameterTypeV == _securitySettings.SelectedParameterS2Type);
            if (testParam == null)
            {
                testParam = new TestParametersS2Settings { ParameterTypeV = _securitySettings.SelectedParameterS2Type };
                _securitySettings.TestS2Settings.Parameters.Add(testParam);
            }
            byte[] res = null;
            if (_securitySettings.TestParameterS2Value != null && _securitySettings.TestParameterS2Value.Length > 0)
            {
                if ((testParam.ParameterTypeV == ParameterS2Type.Span ||
                     testParam.ParameterTypeV == ParameterS2Type.Sender_EI) &&
                    _securitySettings.TestParameterS2Value.Length < 16
                )
                {
                    res = new byte[16];
                    for (int i = 0; i < _securitySettings.TestParameterS2Value.Length; i++)
                    {
                        res[i] = _securitySettings.TestParameterS2Value[i];
                    }
                }
                else if (testParam.ParameterTypeV == ParameterS2Type.SecretKey &&
                         _securitySettings.TestParameterS2Value.Length < 32)
                {
                    res = new byte[32];
                    for (int i = 0; i < _securitySettings.TestParameterS2Value.Length; i++)
                    {
                        res[i] = _securitySettings.TestParameterS2Value[i];
                    }
                }
                else if (testParam.ParameterTypeV == ParameterS2Type.ReservedField || testParam.ParameterTypeV == ParameterS2Type.SequenceNo)
                {
                    res = new byte[] { _securitySettings.TestParameterS2Value[0] };
                }
                else
                {
                    res = _securitySettings.TestParameterS2Value;
                }
                testParam.Value = res;
            }

        }
    }
}
