/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using System.Text;
using ZWave.CommandClasses;
using ZWave;
using ZWave.Devices;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ConfigurationCommandBase : CommandBasicBase
    {
        public IConfigurationModel ConfigurationModel
        {
            get; private set;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public ConfigurationCommandBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
            ConfigurationModel = ControllerSession.ApplicationModel.ConfigurationModel;
            UseBackgroundThread = true;
        }

        public ActionResult ConfigurationSet(NodeTag node, byte[] data, byte parameterNumber, byte size, byte mDefault)
        {
            COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_SET cmd = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_SET();
            cmd.parameterNumber = parameterNumber;
            cmd.configurationValue = data;
            cmd.properties1.size = size;
            cmd.properties1.mdefault = mDefault;
            ActionToken token = null;
            return ControllerSession.SendData(node, cmd, token);
        }

        public string ConfigurationNameGet(NodeTag node, byte[] parameterNumber)
        {
            return ControllerSession.ConfigurationNameGet(node, parameterNumber, _token);
        }

        public string ConfigurationInfoGet(NodeTag node, byte[] parameterNumber)
        {
            return ControllerSession.ConfigurationInfoGet(node, parameterNumber, _token);
        }

        public byte[] ConfigurationGet(NodeTag node, byte[] parameterNumber)
        {
            byte[] ret = null;
            var getCmd = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_GET();
            byte[] rptData = new COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_REPORT();
            getCmd.parameterNumber = parameterNumber.Last(); //TODO!!!!!!!

            var configurationReport = ControllerSession.RequestData(node,
                getCmd, ref rptData, 10000, _token);
            if (configurationReport)
            {
                COMMAND_CLASS_CONFIGURATION_V4.CONFIGURATION_REPORT getRpt = rptData;
                ret = getRpt.configurationValue.ToArray();
            }
            return ret;
        }
    }
}