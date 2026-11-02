#!/usr/bin/env bash
# SPDX-License-Identifier: BSD-3-Clause
# SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
#
# Builds the native GNU/Linux Z-Wave PC Controller as a self-contained folder: the .NET runtime and all
# libraries are pinned and copied in, so the result does not depend on what is installed on the system.
#
#   ./build-linux.sh             build into bin/publish/linux-x64
#   ./build-linux.sh --install   also copy it to ~/.local/opt/zwave-pc-controller, add the
#                                zwave-pc-controller command and an application menu entry
#
# RID=linux-arm64 ./build-linux.sh builds for another architecture.
set -euo pipefail

here="$(cd "$(dirname "$0")" && pwd)"
repo="$(dirname "$here")"
rid="${RID:-linux-x64}"
out="$here/bin/publish/$rid"

dotnet="$(command -v dotnet || true)"
if [ -z "$dotnet" ] && [ -x "$HOME/.dotnet/dotnet" ]; then
    dotnet="$HOME/.dotnet/dotnet"
    export DOTNET_ROOT="$HOME/.dotnet"
fi
if [ -z "$dotnet" ]; then
    echo "The .NET 10 SDK is needed to build. Install it for your user, without root, with:" >&2
    echo "  curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0" >&2
    exit 1
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

if [ ! -f "$repo/z-wave-tools-core/ZWave/ZWave_netcore.csproj" ]; then
    git -C "$repo" submodule update --init --recursive
fi

"$dotnet" publish "$here/ZWaveControllerUI.Avalonia.csproj" -c Release -r "$rid" --self-contained true \
    -p:DebugType=None -p:DebugSymbols=false -o "$out"
echo "Built: $out/ZWaveControllerUI"

if [ "${1:-}" = "--install" ]; then
    dest="$HOME/.local/opt/zwave-pc-controller"
    rm -rf "$dest"
    mkdir -p "$dest" "$HOME/.local/bin" "$HOME/.local/share/applications" "$HOME/.local/share/icons/hicolor/256x256/apps"
    cp -a "$out/." "$dest/"
    cp "$repo/ZWaveControllerUI/Images/z-wave_logo256.png" "$HOME/.local/share/icons/hicolor/256x256/apps/zwave-pc-controller.png"
    cat > "$HOME/.local/bin/zwave-pc-controller" <<EOF
#!/bin/sh
exec "$dest/ZWaveControllerUI" "\$@"
EOF
    chmod +x "$HOME/.local/bin/zwave-pc-controller"
    cat > "$HOME/.local/share/applications/zwave-pc-controller.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Z-Wave PC Controller
Comment=Set up a Z-Wave network and operate nodes
Exec=$dest/ZWaveControllerUI
Icon=zwave-pc-controller
Terminal=false
Categories=Development;Utility;
EOF
    echo "Installed: $dest (run zwave-pc-controller, or Z-Wave PC Controller from the application menu)"
fi
