/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController.Services;

namespace ZWaveController
{

    public static class CommandsFactory
    {
        public static string CurrentSourceId { get; set; }

        public static ICommandRunner CommandRunner { get; set; }

        public static T CommandControllerSessionGet<T>(string sourceId = null) where T : ControllerSessionCommandBase
        {
            return string.IsNullOrEmpty(sourceId ?? CurrentSourceId) ? null :
                (T)Activator.CreateInstance(typeof(T), ControllerSessionsContainer.ControllerSessions[sourceId ?? CurrentSourceId]);
        }

        public static T CommandControllerSessionGet<T>(string sourceId, params object[] ctorParams) where T : ControllerSessionCommandBase
        {
            return string.IsNullOrEmpty(sourceId ?? CurrentSourceId) ? null :
                (T)Activator.CreateInstance(typeof(T), (new object[] { ControllerSessionsContainer.ControllerSessions[sourceId ?? CurrentSourceId] }).Concat(ctorParams).ToArray());
        }

        public static T CommandBaseGet<T>(Action<object> execute, Func<object, bool> canExecute = null, bool UseBackgroundThread = true) where T : CommandBase
        {
            var command = (T)(Activator.CreateInstance(typeof(T), execute, canExecute));
            command.UseBackgroundThread = UseBackgroundThread;
            return command;
        }

        public static T CommandBaseGet<T>(Action<object> execute, Func<object, bool> canExecute, Action<object> cancel, Func<object, bool> canCancel, bool UseBackgroundThread = true) where T : CommandBase
        {
            var command = (T)(Activator.CreateInstance(typeof(T), execute, canExecute, cancel, canCancel));
            command.UseBackgroundThread = UseBackgroundThread;
            return command;
        }

        public static T CommandSourcesGet<T>(IApplicationModel applicationModel) where T : SourcesCommandBase
        {
            return (T)(Activator.CreateInstance(typeof(T), applicationModel));
        }

        public static T CommandSourcesGet<T>(IApplicationModel applicationModel, List<IDataSource> dataSources, LogBaseService logger) where T : SourcesCommandBase
        {
            return (T)(Activator.CreateInstance(typeof(T), applicationModel, dataSources, logger));
        }

        public static T CommandSourcesGet<T>(IApplicationModel applicationModel, List<IDataSource> dataSources) where T : SourcesCommandBase
        {
            return (T)(Activator.CreateInstance(typeof(T), applicationModel, dataSources));
        }
    }
}