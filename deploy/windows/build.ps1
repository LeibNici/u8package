param(
    [string]$Configuration = "Release",
    [string]$NuGetPath = "",
    [string]$MSBuildPath = "",
    [switch]$SkipNuGetDownload
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$src = Join-Path $root "src"
$solution = Join-Path $src "Xinchuan.U8Bridge.sln"
$packages = Join-Path $src "packages"
$tools = Join-Path $PSScriptRoot ".tools"
$localNuGet = Join-Path $tools "nuget.exe"

function Resolve-NuGet {
    if ($NuGetPath -and (Test-Path $NuGetPath)) {
        return $NuGetPath
    }

    $command = Get-Command "nuget.exe" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    if (Test-Path $localNuGet) {
        return $localNuGet
    }

    if ($SkipNuGetDownload) {
        throw "nuget.exe was not found. Install NuGet CLI or put nuget.exe at $localNuGet."
    }

    New-Item -ItemType Directory -Force -Path $tools | Out-Null
    Write-Host "nuget.exe was not found. Downloading to $localNuGet ..."
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest `
        -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" `
        -OutFile $localNuGet
    return $localNuGet
}

function Resolve-MSBuild {
    if ($MSBuildPath -and (Test-Path $MSBuildPath)) {
        return $MSBuildPath
    }

    $command = Get-Command "msbuild.exe" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $installPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($installPath) {
            $candidate = Join-Path $installPath "MSBuild\Current\Bin\MSBuild.exe"
            if (Test-Path $candidate) {
                return $candidate
            }
        }
    }

    throw "MSBuild was not found. Install Visual Studio Build Tools 2022 with .NET Framework build tools."
}

$resolvedNuGet = Resolve-NuGet
$resolvedMSBuild = Resolve-MSBuild

Write-Host "NuGet: $resolvedNuGet"
Write-Host "MSBuild: $resolvedMSBuild"

& $resolvedNuGet restore $solution -PackagesDirectory $packages
& $resolvedMSBuild $solution /p:Configuration=$Configuration /p:Platform="Any CPU"
