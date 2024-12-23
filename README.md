# Z-Wave-PC-Controller

## Description

PC Controller is a GUI tool used to setup a Z-Wave network, and operate nodes.
It needs a Z-Wave Serial API controller (connected to host using USB or IP network).


## Usage

The tool is mainly supporting Windows OS, but can also be used on other win32 runtimes.

## Build prerequisites

- [z-wave-blobs](https://github.com/Z-Wave-Alliance/z-wave-blobs/)
- [z-wave-tools-core](https://github.com/Z-Wave-Alliance/z-wave-tools-core/)
- [z-wave-pc-controller](https://github.com/Z-Wave-Alliance/z-wave-pc-controller)
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks?cid=getdotnetsdk)

[z-wave-blobs](https://github.com/Z-Wave-Alliance/z-wave-blobs/),
[z-wave-tools-core](https://github.com/Z-Wave-Alliance/z-wave-tools-core/),
and
[z-wave-pc-controller](https://github.com/Z-Wave-Alliance/z-wave-pc-controller)
must be at the same level.

## Build

On the Command Prompt:


    MSBuild.exe z-wave-pc-controller\ZWaveController.sln -restore -verbosity:minimal

    MSBuild.exe z-wave-pc-controller\ZWaveController.sln -property:Platform="Any CPU" -property:Configuration=Release -p:OutputPath=artifacts\release -p:DebugType=None -p:DebugSymbols=false -verbosity:minimal

    MSBuild.exe z-wave-pc-controller\ZWaveController.sln -property:Platform="Any CPU" -property:Configuration=Debug -p:OutputPath=artifacts\debug -verbosity:minimal

## References

- https://github.com/Z-Wave-Alliance/z-wave-pc-controller
- https://docs.silabs.com/z-wave/1.0.1/z-wave-docs/zwave-tools
- https://www.silabs.com/developer-tools/simplicity-studio
- https://community.silabs.com/s/article/How-to-get-Z-Wave-PC-Controller-and-Zniffer-tools


## Legal info

SPDX-License-Identifier: BSD-3-Clause

SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
