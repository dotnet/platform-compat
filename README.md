# Platform Compatibility Analyzer

This project has been replaced by analzyers that are built into the .NET SDK:

* [Spec: Platform compatibility analzyer](https://github.com/dotnet/designs/blob/main/accepted/2020/platform-checks/platform-checks.md)
* [Spec: Better obsoletion](https://github.com/dotnet/designs/blob/main/accepted/2020/better-obsoletion/better-obsoletion.md)
* [Announcement: Platform compatibility analzyer](https://devblogs.microsoft.com/dotnet/the-future-of-net-standard/#dealing-with-windows-specific-apis)

As such, it's archived.

----

[![Build Status](https://img.shields.io/azure-devops/build/dnceng/public/449/master.svg)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=449&branchName=master&view=logs) [![Build Status](https://img.shields.io/azure-devops/tests/dnceng/public/449/master.svg)](https://dev.azure.com/dnceng/public/_build/latest?definitionId=449&branchName=master&view=logs)

This tool provides [Roslyn](https://github.com/dotnet/roslyn) analyzers that
find usages of .NET Core & .NET Standard APIs that are problematic on specific
platforms or are deprecated.

You can find out more in our [blog post](https://blogs.msdn.microsoft.com/dotnet/2017/10/31/introducing-api-analyzer/)!

## Usage

In order to use it, install the NuGet package [Microsoft.DotNet.Analyzers.Compatibility](https://www.nuget.org/packages/Microsoft.DotNet.Analyzers.Compatibility).

## Experience

### Usage of .NET Core and .NET Standard APIs that throw `PlatformNotSupportedException`

![](docs/screenshot1.png)

See [PC001](docs/PC001.md) for more details.

### Usage of .NET Standard 2.0 APIs missing from .NET Framework 4.6.1

![](docs/screenshot2.png)

See [PC002](docs/PC002.md) for more details.

### Usage of deprecated APIs

![](docs/screenshot3.png)

See [DEXXX files in the docs folder](docs) for more details.
