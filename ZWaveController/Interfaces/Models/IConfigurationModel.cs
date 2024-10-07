/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Bind;

namespace ZWaveController.Interfaces
{
    public interface IConfigurationModel
    {
        ISubscribeCollection<IConfigurationParamModel> ConfigurationParameters { get; set; }

        void TestFill();
        void ClearParameters();
        IConfigurationParamModel GetConfigurationParam();
        IBitFieldConfigurationParam GetBitFieldConfigParam();
        IUnsignedIntegerConfigurationParam GetUintConfigParam(int minValue, int maxValue, uint value);
        ISignedIntegerConfigurationParam GetSignedIntConfigParam(int minValue, int maxValue, int value);
        IEnumeratedConfigurationParam GetEnumConfigParam();
    }
}
