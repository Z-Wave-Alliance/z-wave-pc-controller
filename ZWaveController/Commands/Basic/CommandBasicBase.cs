/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWave;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Enums;
using ZWave.Devices;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class CommandBasicBase : ControllerSessionCommandBase
    {
        protected NodeTag Device { get; set; }
        public CommandBasicBase(IControllerSession controllerSession)
            : base(controllerSession)
        {
        }

        protected ActionToken _token = null;

        public Device SessionDevice
        {
            get { return (ControllerSession as BasicControllerSession)?._device; }
        }

        public virtual CommandTypes CommandType { get { return CommandTypes.None; } }

        public virtual void UpdateTargetDevice()
        {
            if (ControllerSession.Controller != null && !_isTargetDeviceFrozen)
            {
                if (ControllerSession.ApplicationModel.SelectedNode != null)
                {
                    TargetDevice = ControllerSession.ApplicationModel.SelectedNode.Item;
                    IsSleepingTarget = !ControllerSession.Controller.Network.IsDeviceListening(TargetDevice);
                }
            }
        }

        bool _isTargetDeviceFrozen = false;
        public void FreezeTargetDevice()
        {
            _isTargetDeviceFrozen = true;
        }

        public bool IsSleepingTarget { get; set; }
        public NodeTag TargetDevice { get; set; }
        public bool IsTargetSensitive { get; set; }
        public bool IsCancelAtController { get; set; }

        protected override void CancelAction(object param)
        {
            if (!IsCancelAtController)
            {
                if (TargetDevice.Id > 0)
                    ControllerSession.Cancel(TargetDevice, _token);
            }
            else
                ControllerSession.Cancel(NodeTag.Empty, _token);
        }

        public virtual void PrepareData()
        {
            Device = (NodeTag)TargetDevice.Clone();
        }

        public override bool IsSupportedAndReady()
        {
            bool ret = true;
            UpdateTargetDevice();
            PrepareData();
            if (ControllerSession is BasicControllerSession)
            {
                if (CommandType != CommandTypes.None && ControllerSession.Controller != null && SessionDevice.SupportedSerialApiCommands != null)
                {
                    ret = SessionDevice.SupportedSerialApiCommands.Contains((byte)CommandType);
                    if (!ret)
                    {
                        ControllerSession.LogError("{0} is not implemented in the Z-Wave Device", CommandType);
                    }
                }
                if (ret && IsTargetSensitive && TargetDevice != null)
                {
                    if (TargetDevice.Id > 0 && IsSleepingTarget)
                    {
                        ret = false;
                        IsSleepingTarget = false;
                        ControllerSession.Logger.LogOk($"Command: {Text} is queued for Node: {TargetDevice}");
                        (ControllerSession as BasicControllerSession).EnqueueCommand(TargetDevice, (CommandBase)MemberwiseClone());
                    }
                }
            }
            return ret;
        }
    }
}
