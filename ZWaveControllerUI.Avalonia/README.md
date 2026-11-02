<!--
SPDX-License-Identifier: BSD-3-Clause
SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
-->
# Z-Wave PC Controller for GNU/Linux

A native GNU/Linux front end of the PC Controller, without Wine or Mono. It is built with
[Avalonia](https://avaloniaui.net/) on .NET 10.

The view models are compiled directly from `../ZWaveControllerUI/Models`, so they stay identical to the
WPF build. The controller logic comes from `../ZWaveController` and `z-wave-tools-core` through their
`*_netcore` projects. Only the views, converters and controls are specific to this project.

## Build and run

Building needs the .NET 10 SDK. A user-local install needs no root:

```sh
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
```

Then build:

```sh
./build-linux.sh              # self-contained build in bin/publish/linux-x64
./build-linux.sh --install    # and install to ~/.local/opt, with an application menu entry
bin/publish/linux-x64/ZWaveControllerUI
```

The build is self-contained. It includes the .NET runtime, and every package version is pinned in
`ZWaveControllerUI.Avalonia.csproj`, so running it does not depend on what is installed on the system.
The script fetches the `z-wave-tools-core` submodules if they are missing.

## Serial ports

Controllers show up as `/dev/ttyACM*` (or `/dev/ttyUSB*`) in Settings. If opening one fails with a
permission error, add yourself to the group that owns the port, then log out and back in:

```sh
sudo usermod -aG dialout "$USER"
```

The native S2 crypto (`libs2crypto64.so`) and Z/IP transport (`libziptrans64.so`) libraries ship with
the build.

## Differences from the Windows build

- Scanning a DSK QR code with a webcam is not available, because that uses a Windows-only capture
  library. A QR code typed by a barcode scanner, or pasted, is read instead.
- The log view copies the selected lines with Ctrl+C.
- Settings of the UI are kept in `~/.config/Silicon Labs/Z-Wave PC Controller 5/ui-settings.json`,
  next to the controller configuration. The crash log is `Controller_curr.log` in the temp folder.
