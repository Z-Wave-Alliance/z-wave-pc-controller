/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Threading;

namespace ZWaveController.Commands
{
    public class CommandRunner : ICommandRunner
    {
        public void ExecuteAsync(CommandBase cmd, object param)
        {
            if (cmd == null || !cmd.IsSupportedAndReady())
            {
                return;
            }
            if (cmd.UseBackgroundThread)
            {
                ThreadPool.QueueUserWorkItem(state =>
                    {
                        var arg = (CommandExecuteArgs)state;
                        arg.Command.Execute(arg.Parameter);
                        ProcessInferiorCommands(arg.Command, arg.Parameter);
                        // arg.Command.OnExecuteCompleted();
                    },
                    new CommandExecuteArgs { Command = cmd, Parameter = param });
            }
            else
            {
                cmd.Execute(param);
                ProcessInferiorCommands(cmd, param);
            }
        }

        public void Execute(CommandBase cmd, object param, bool isSleepingCommand = false)
        {
            if (cmd != null)
            {
                if(!isSleepingCommand)
                    (cmd as CommandBasicBase)?.PrepareData();
                cmd.Execute(param);
                ProcessInferiorCommands(cmd, param);
            }
        }

        private void ProcessInferiorCommands(CommandBase command, object param)
        {
            if (command.InferiorCommands == null)
            {
                return;
            }
            foreach (var cmd in command.InferiorCommands)
            {
                if (!cmd.IsSupportedAndReady())
                {
                    continue;
                }
                cmd.Execute(param);
            }
        }

        internal class CommandExecuteArgs
        {
            public CommandBase Command { get; set; }
            public object Parameter { get; set; }
        }
    }
}
