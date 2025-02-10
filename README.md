# Z-Wave-PC-Controller

## Description

The Z-Wave PC Controller is a GUI tool used to setup a Z-Wave network and operate nodes.
It needs a Z-Wave Serial API controller (connected via USB or IP).

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

The PC tools can be built on command line using [Build Tools for Visual Studio 2022](https://visualstudio.microsoft.com/downloads/).

## Build

Run this once:
```
MSBuild.exe z-wave-pc-controller\ZWaveController.sln -restore -verbosity:minimal
```

Build for debug:
```
MSBuild.exe z-wave-pc-controller\ZWaveController.sln -property:Platform="Any CPU" -property:Configuration=Debug -p:OutputPath=artifacts\debug -verbosity:minimal
```

Build for release:
```
MSBuild.exe z-wave-pc-controller\ZWaveController.sln -property:Platform="Any CPU" -property:Configuration=Release -p:OutputPath=artifacts\release -p:DebugType=None -p:DebugSymbols=false -verbosity:minimal
```

## References

- https://github.com/Z-Wave-Alliance/z-wave-pc-controller
- https://docs.silabs.com/z-wave/1.0.1/z-wave-docs/zwave-tools
- https://www.silabs.com/developer-tools/simplicity-studio
- https://community.silabs.com/s/article/How-to-get-Z-Wave-PC-Controller-and-Zniffer-tools


## Legal info

SPDX-License-Identifier: BSD-3-Clause

SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
