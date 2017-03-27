# PNS Scan

[![Build status](https://ci.appveyor.com/api/projects/status/7flj8m2ko6jh9ng5/branch/master?svg=true)](https://ci.appveyor.com/project/terrajobst/pns-scan/branch/master)

This tool provides a [Roslyn](https://github.com/dotnet/roslyn) diagnostic analyzer
that finds usages of APIs that will throw `PlatformNotSupportedException`
on certain platforms.

The experience looks like this:

![](docs/screenshot.png)