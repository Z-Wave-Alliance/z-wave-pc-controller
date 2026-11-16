# SPDX-License-Identifier: BSD-3-Clause
# SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
<#
.SYNOPSIS
Publishes ZWaveControllerUI for Windows as a runtime folder or managed application bundle.

.DESCRIPTION
Supports self-contained and framework-dependent folders, plus a self-contained managed bundle for win-x64 and win-x86. Required XML and native DLL sidecars are checked/copied. Publish output is cleaned before each run. Optional Authenticode signing signs the main executable.

.PARAMETER RuntimeIdentifier
Windows target architecture: win-x64 or win-x86.

.PARAMETER Mode
SelfContained, FrameworkDependent, or ManagedBundle. ManagedBundle still needs XML and native DLL sidecars.

.PARAMETER Configuration
MSBuild configuration, normally Release.

.PARAMETER OutputPath
Publish output directory. Relative paths are resolved from the current directory. Existing default output is replaced; an existing custom path requires -Force.

.PARAMETER Force
Allow the script to remove and replace an existing custom output directory.

.PARAMETER NoRestore
Skip NuGet restore; use only when dependencies for this publish target are already restored.

.PARAMETER Sign
Sign and verify ZWaveControllerUI.exe after publishing.

.PARAMETER CertificateThumbprint
Thumbprint of the signing certificate in Cert:\CurrentUser\My. Required with -Sign.

.PARAMETER TimestampUrl
RFC 3161 timestamp service URL.

.EXAMPLE
.\publish\publish.ps1

.EXAMPLE
.\publish\publish.ps1 -RuntimeIdentifier win-x86 -Mode ManagedBundle

.EXAMPLE
.\publish\publish.ps1 -Sign -CertificateThumbprint 'CERTIFICATE_THUMBPRINT'
#>
[CmdletBinding()]
param(
	[ValidateSet('win-x64', 'win-x86')]
	[string]$RuntimeIdentifier = 'win-x64',

	[ValidateSet('SelfContained', 'FrameworkDependent', 'ManagedBundle')]
	[string]$Mode = 'SelfContained',

	[string]$Configuration = 'Release',
	[string]$OutputPath,
	[switch]$Force,
	[switch]$NoRestore,
	[switch]$Sign,
	[string]$CertificateThumbprint,
	[string]$TimestampUrl = 'https://timestamp.digicert.com'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repositoryRoot 'ZWaveControllerUI\ZWaveControllerUI.csproj'

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
	throw "UI project was not found: $projectPath"
}

$thumbprint = $null
$signTool = $null
if ($Sign) {
	if ([string]::IsNullOrWhiteSpace($CertificateThumbprint)) {
		throw 'Signing requires -CertificateThumbprint for a code-signing certificate in the CurrentUser\My certificate store.'
	}
	$thumbprint = $CertificateThumbprint -replace '\s', ''
	$certificate = Get-ChildItem -Path 'Cert:\CurrentUser\My' | Where-Object {
		($_.Thumbprint -replace '\s', '') -eq $thumbprint
	} | Select-Object -First 1
	if ($null -eq $certificate) {
		throw "Certificate $thumbprint was not found in Cert:\CurrentUser\My."
	}
	if (-not $certificate.HasPrivateKey) {
		throw "Certificate $thumbprint does not have an accessible private key."
	}
	$signTool = Get-Command 'signtool.exe' -ErrorAction SilentlyContinue
	if ($null -eq $signTool) {
		throw 'signtool.exe was not found on PATH. Install the Windows SDK signing tools or add signtool.exe to PATH.'
	}
}

$usesDefaultOutputPath = [string]::IsNullOrWhiteSpace($OutputPath)
if ($usesDefaultOutputPath) {
	$modeDirectory = switch ($Mode) {
		'SelfContained' { 'self-contained' }
		'FrameworkDependent' { 'framework-dependent' }
		'ManagedBundle' { 'managed-bundle' }
	}
	$OutputPath = Join-Path $PSScriptRoot "artifacts\$RuntimeIdentifier\$modeDirectory"
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$repositoryRoot = [System.IO.Path]::GetFullPath($repositoryRoot).TrimEnd('\')
$rootPath = [System.IO.Path]::GetPathRoot($OutputPath).TrimEnd('\')
$normalizedOutputPath = $OutputPath.TrimEnd('\')
$outputPathPrefix = $normalizedOutputPath + '\'
if ($normalizedOutputPath.Equals($rootPath, [System.StringComparison]::OrdinalIgnoreCase) -or
	$normalizedOutputPath.Equals($repositoryRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
	$repositoryRoot.StartsWith($outputPathPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
	throw "Refusing to publish to a filesystem root or a directory containing the repository: $OutputPath"
}
if (Test-Path -LiteralPath $OutputPath) {
	if (-not (Test-Path -LiteralPath $OutputPath -PathType Container)) {
		throw "Output path exists and is not a directory: $OutputPath"
	}
	if (-not $usesDefaultOutputPath -and -not $Force) {
		throw "Custom output directory already exists. Choose a new path or specify -Force to replace it: $OutputPath"
	}
	Write-Host "Cleaning previous publish output: $OutputPath"
	Remove-Item -LiteralPath $OutputPath -Recurse -Force
}
$null = New-Item -ItemType Directory -Path $OutputPath -Force

$publishArguments = @(
	'publish', $projectPath,
	'--configuration', $Configuration,
	'--runtime', $RuntimeIdentifier,
	'--self-contained', ($Mode -ne 'FrameworkDependent').ToString().ToLowerInvariant(),
	'--output', $OutputPath,
	'-p:DebugType=None',
	'-p:DebugSymbols=false'
)

if ($Mode -eq 'ManagedBundle') {
	$publishArguments += @(
		'-p:PublishSingleFile=true',
		'-p:IncludeNativeLibrariesForSelfExtract=true'
	)
}
if ($NoRestore) {
	$publishArguments += '--no-restore'
}

Write-Host "Publishing $projectPath"
Write-Host "  Mode: $Mode; RID: $RuntimeIdentifier; Configuration: $Configuration"
Write-Host "  Output: $OutputPath"
& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
	throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$licensePath = Join-Path $repositoryRoot 'LICENSE'
if (-not (Test-Path -LiteralPath $licensePath -PathType Leaf)) {
	throw "Required application license file was not found: $licensePath"
}
Copy-Item -LiteralPath $licensePath -Destination (Join-Path $OutputPath 'LICENSE') -Force

$architecture = if ($RuntimeIdentifier -eq 'win-x64') { '64' } else { '32' }
if ($Mode -eq 'ManagedBundle') {
	$nativeFiles = @(
		@{
			Source = Join-Path $repositoryRoot "z-wave-tools-core\z-wave-blobs\z-wave-tools-core\CryptoLibraryS2\win\s2crypto$architecture.dll"
			Name = "s2crypto$architecture.dll"
		},
		@{
			Source = Join-Path $repositoryRoot "z-wave-tools-core\z-wave-blobs\z-wave-tools-core\ZipTransport\win\ziptrans$architecture.dll"
			Name = "ziptrans$architecture.dll"
		},
		@{
			Source = Join-Path $repositoryRoot "z-wave-tools-core\z-wave-blobs\z-wave-pc-controller\WebCamCapture\win\wcvcap$architecture.dll"
			Name = "wcvcap$architecture.dll"
		}
	)
	foreach ($nativeFile in $nativeFiles) {
		if (-not (Test-Path -LiteralPath $nativeFile.Source -PathType Leaf)) {
			throw "Required native library source was not found: $($nativeFile.Source)"
		}
		Copy-Item -LiteralPath $nativeFile.Source -Destination (Join-Path $OutputPath $nativeFile.Name) -Force
	}
}

$executablePath = Join-Path $OutputPath 'ZWaveControllerUI.exe'
if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
	throw "Publish completed but the expected application executable is missing: $executablePath"
}

$requiredFiles = @(
	(Join-Path $OutputPath 'LICENSE'),
	(Join-Path $OutputPath 'XmlFiles\zwave.xml'),
	(Join-Path $OutputPath "s2crypto$architecture.dll"),
	(Join-Path $OutputPath "ziptrans$architecture.dll"),
	(Join-Path $OutputPath "wcvcap$architecture.dll")
)
$missingFiles = @($requiredFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
if ($missingFiles.Count -gt 0) {
	throw "Publish output is missing files required by this application:`n$($missingFiles -join "`n")"
}

$otherArchitecture = if ($architecture -eq '64') { '32' } else { '64' }
$wrongArchitectureFiles = @(
	(Join-Path $OutputPath "s2crypto$otherArchitecture.dll"),
	(Join-Path $OutputPath "ziptrans$otherArchitecture.dll"),
	(Join-Path $OutputPath "wcvcap$otherArchitecture.dll")
) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf }
foreach ($file in $wrongArchitectureFiles) {
	Remove-Item -LiteralPath $file -Force
	Write-Host "Removed non-target native library: $(Split-Path -Leaf $file)"
}

$pdbFiles = @(Get-ChildItem -LiteralPath $OutputPath -Filter '*.pdb' -File -Recurse)
foreach ($pdbFile in $pdbFiles) {
	Remove-Item -LiteralPath $pdbFile.FullName -Force
}
if ($pdbFiles.Count -gt 0) {
	Write-Host "Removed $($pdbFiles.Count) debug-symbol files from the release output."
}

if ($Sign) {
	$signArguments = @(
		'sign', '/fd', 'SHA256',
		'/tr', $TimestampUrl, '/td', 'SHA256',
		'/sha1', $thumbprint,
		$executablePath
	)
	& $signTool.Source @signArguments
	if ($LASTEXITCODE -ne 0) {
		throw "SignTool signing failed with exit code $LASTEXITCODE."
	}

	& $signTool.Source verify '/pa' '/v' $executablePath
	if ($LASTEXITCODE -ne 0) {
		throw "SignTool verification failed with exit code $LASTEXITCODE."
	}
}

Write-Host "Publish completed: $OutputPath"
if ($Mode -eq 'ManagedBundle') {
	Write-Warning 'ManagedBundle places the managed app/runtime in one EXE; XML and native DLL files remain required sidecars.'
}
