/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Commands
{
    public class SelectDefaultCommandClassesCommand : CommandBasicBase
    {
        public SelectDefaultCommandClassesCommand(IControllerSession controllerSession) : base(controllerSession)
        {
            Text = "Select Controller Default Command Classes";
            UseBackgroundThread = true;
        }

        protected override bool IsExecuteOnEndDeviceAlloved => true;

        public override bool CanExecuteModelDependent(object param)
        {
            return ControllerSession is BasicControllerSession;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            var defaultCcs = ControllerSession.NodeInformationService.GetDefaultCommandClasses();
            foreach (var item in ApplicationModel.SetNodeInformationModel.CommandClasses.Where(cc => defaultCcs.Contains(cc.Item.KeyId)))
                item.IsSelected = true;
        }
    }
}
