<!---
SPDX-License-Identifier: BSD-3-Clause
SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
-->
# Windows publishing

This folder contains a PowerShell publishing script for the `ZWaveControllerUI` .NET 10 WPF/WinForms application. It leaves the normal Visual Studio build unchanged.

## Quick start: portable x64 folder

1. Install the .NET 10 SDK on the build machine. The recipient does **not** need the .NET runtime for a self-contained publish.
2. Open PowerShell at the repository root (the folder containing `ZWaveController.sln`).
3. Run:

   ```powershell
   .\publish\publish.ps1
   ```

   If Windows blocks script execution, review the script and run it with a process-scoped policy override:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\publish\publish.ps1
   ```

4. Wait for `Publish completed`. The output is `publish\artifacts\win-x64\self-contained`. The script cleans this dedicated output folder before each run; generated output is ignored by Git and can be recreated. If a publish fails, discard the partial folder and rerun.
5. Copy the **whole output folder** to the target Windows x64 computer; run `ZWaveControllerUI.exe` from that folder. Do not copy just the executable.
6. Test on a clean Windows x64 computer and exercise the hardware/features you use, especially serial connections, Z-Wave S2 security, ZIP/IP connections, and webcam capture.

The script fails if the published output is missing `XmlFiles\zwave.xml` or the architecture-specific native DLLs used by this application.

## Available publish variants

The script also has comment-based PowerShell help (`.\publish\publish.ps1 -?`). Common commands:

```powershell
# Portable self-contained x64 folder (default)
.\publish\publish.ps1

# Smaller framework-dependent x64 folder; target computer needs .NET 10 Desktop Runtime
.\publish\publish.ps1 -Mode FrameworkDependent

# Managed app/runtime bundle EXE plus required XML/native sidecar files
.\publish\publish.ps1 -Mode ManagedBundle

# 32-bit Windows variants
.\publish\publish.ps1 -RuntimeIdentifier win-x86
.\publish\publish.ps1 -RuntimeIdentifier win-x86 -Mode ManagedBundle

# Choose configuration/output directory; existing custom directories require -Force to replace
.\publish\publish.ps1 -Configuration Release -OutputPath .\custom-publish
.\publish\publish.ps1 -Configuration Release -OutputPath .\custom-publish -Force

# Skip restore when packages have already been restored
.\publish\publish.ps1 -NoRestore
```

| Mode | Runtime required on target | Output shape | Recommendation |
|---|---|---|---|
| `SelfContained` | None for .NET | Runtime and app files in a folder | Recommended portable release. |
| `FrameworkDependent` | .NET 10 Desktop Runtime | App and dependencies in a folder | Use when the runtime is already installed. |
| `ManagedBundle` | None for .NET | Managed app/runtime in one EXE, with XML/native sidecars | Optional; not a one-file distribution. |

`ManagedBundle` is deliberately not called `SingleFile`: this application still needs `XmlFiles\zwave.xml` and native DLL sidecars. In validation, its x64 EXE was about 192 MiB, and the complete distribution was about 199 MiB. It reduces the number of managed files, not total size; the self-contained folder is the default recommendation.

The repository layout keeps publishing tools and their generated artifacts together: `publish\publish.ps1`, `publish\README.md`, and `publish\artifacts\<RID>\<flavor>` (for example, `publish\artifacts\win-x64\self-contained`). This RID-then-flavor naming keeps target architecture and runtime choice clear. Both `win-x64` and `win-x86` are supported; publish each architecture separately. Default variant folders are cleaned before each run, so stale files cannot remain. Existing custom output folders are not removed unless `-Force` is specified; choose a fresh custom path or use `-Force` only when you intend to replace that exact folder.

Release output omits `.pdb` debug-symbol files. Each variant includes the repository-root `LICENSE` for binary redistribution. Keep runtime/application DLLs, `.deps.json`/`.runtimeconfig.json` when present, `.dll.config` settings files, `XmlFiles\zwave.xml`, and the target-architecture native DLLs. The script removes native DLLs for the other architecture. The WPF icon is embedded in the application, so the redundant root `App.ico` is not copied to the publish output. Third-party components may have additional license and notice requirements; review and include those separately when distributing.

## Signing a published application

Authenticode signing gives Windows a verifiable publisher identity and lets recipients check whether the signed executable was changed after signing. It does not make an application trusted by itself: the certificate must chain to a trusted authority, be valid for code signing, and be protected appropriately.

1. Obtain a code-signing certificate from an appropriate certificate authority or your organization's PKI. For public distribution, use a certificate whose chain is trusted by Windows. Self-signed certificates are suitable only for controlled testing unless separately trusted on each recipient computer.
2. Install/import the certificate and its private key into the signing user's `CurrentUser\Personal` (`Cert:\CurrentUser\My`) store. The private key must be usable by that user. Do not commit or distribute private keys/passwords with this repository.
3. Install the Windows SDK SignTool and make `signtool.exe` available on `PATH`.
4. Find the certificate thumbprint in the certificate details (remove spaces), then publish and sign in one step:

   ```powershell
   .\publish\publish.ps1 -Sign -CertificateThumbprint 'YOUR_CERTIFICATE_THUMBPRINT'
   ```

   You can combine these parameters with `-Mode`, `-RuntimeIdentifier`, `-Configuration`, and `-OutputPath`. The script signs and verifies `ZWaveControllerUI.exe`, using SHA-256 and an RFC 3161 timestamp. An internet connection to the timestamp service is needed. Change `-TimestampUrl` if your organization uses another timestamp service.

5. Confirm the script's SignTool verification succeeded. Before release, test the signed output and publish the certificate/publisher identity through your normal release channel. This signs only `ZWaveControllerUI.exe`; it does not sign the managed dependency DLLs or native DLL sidecars.

For production, keep certificate private keys in an approved secure signing service, HSM, or protected key store; avoid exporting private keys where possible.

## Existing CI workflow

The existing `.github/workflows/build.yml` is unchanged. Its Windows job already restores and builds the solution in Release and Debug, then performs a Release MSBuild output into `artifacts\<repository-name>`, packages that output, uploads it as a workflow artifact, and attaches the package to tagged GitHub releases. That workflow does **not** call this manual publishing script or produce the six RID/mode-specific folders under `publish\artifacts`. Use the commands above for those variants; their CI integration is intentionally left unchanged.

## Current limitations and remaining work

- **This is not currently a one-EXE-only application.** `ZWaveController\Configuration\Config.cs` reads `XmlFiles\zwave.xml` relative to the app directory. The controller and referenced projects also use native `s2crypto`, `ziptrans`, and `wcvcap` DLLs through P/Invoke. `ManagedBundle` places the managed app/runtime in one EXE but keeps those XML/native assets beside it. Embedding/extracting those assets and changing their lookup/loading would require separate implementation and testing.
- **Native features need real-device testing.** The projects include native blobs for both architectures in some build outputs; the publish script retains only the selected architecture. It checks required files, not successful loading or behavior. Test each supported architecture and feature on a clean machine.
- No signing certificate or secure key-storage arrangement is included. Signing is optional and requires the publisher to provide these.
- The current `ZWaveControllerUI\app.config` still declares a .NET Framework 4.8 startup runtime. It predates the .NET 10 SDK-style project; inspect the generated publish configuration and confirm it is not relied on or misleading before shipping.

## Troubleshooting

- If restore fails, ensure the .NET 10 SDK and NuGet access are available, then rerun without `-NoRestore`.
- If the output-file check fails, inspect the build output and the native/content file conditions in the referenced projects. Do not distribute an incomplete folder.
- If signing fails, confirm that SignTool is on `PATH`, the certificate thumbprint is correct, its private key is accessible, the certificate permits code signing, and the timestamp endpoint is reachable.
