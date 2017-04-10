# Platform Compatibility Analyzer

[![Build status](https://ci.appveyor.com/api/projects/status/evecxgd6lnsg20lb/branch/master?svg=true)](https://ci.appveyor.com/project/terrajobst/platform-compat/branch/master)

This tool provides [Roslyn](https://github.com/dotnet/roslyn) analyzers that
find usages of .NET Core & .NET Standard APIs that are problematic on specific
platforms:

* **[PC001](docs/PC001.md)**: Usage of .NET Core and .NET Standard APIs that throw `PlatformNotSupportedException`
* **[PC002](docs/PC002.md)**: Usage of .NET Standard 2.0 APIs missing from .NET Framework 4.6.1

The experience looks like this:

![](docs/screenshot.png)

## Usage

In order to use it, install the NuGet package [Terrajobst.PlatformCompat.Analyzers](https://www.nuget.org/packages/terrajobst.platformcompat.analyzers).

## Nightlies

The feed with nightly builds can be found here:

```
https://ci.appveyor.com/nuget/platform-compat
```
