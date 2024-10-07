/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using ZWave.BasicApplication.Enums;
using ZWave.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class ShiftControllerCommand : NetworkManagamentCommandBase
    {
        public ShiftControllerCommand(IControllerSession controllerSession)
            : base(controllerSession)
        {
            Text = "Shift Controller (Controller Change)";
            IsCancelAtController = true;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CmdZWaveControllerChange; }
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession.Controller is ZWave.Devices.IController controller &&
                controller.NetworkRole.HasFlag(ControllerRoles.RealPrimary);
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ControllerChange(out _token);
        }
    }
}