# Xunit.Isolation

[![GitHub Actions status](https://github.com/harnel-tngn/Xunit.Isolation/actions/workflows/build.yml/badge.svg)](https://github.com/harnel-tngn/Xunit.Isolation/actions/workflows/build.yml)
[![NuGet package](https://img.shields.io/nuget/v/Xunit.Isolation.svg)](https://nuget.org/packages/Xunit.Isolation)
[![NuGet package](https://img.shields.io/nuget/v/Xunit.Isolation.SkippableFact.svg)](https://nuget.org/packages/Xunit.Isolation.SkippableFact)

Xunit.Isolation is a library to run Xunit tests in isolated `AssemblyLoadContext`, means every static variables referenced in tests are isolated and does not affect each other.

## Getting Started

### Installation

You can install Xunit.Isolation through NuGet.

If you are using `SkippableFact` or `SkippableTheory` from Xunit.SkippableFact, install Xunit.Isolation.SkippableFact together.

### Usage

To introduce Xunit.Isolation, set `TestFrameworkAttribute` in code to set test framework to `IsolationFramework`.

```C#
using Xunit;
using Xunit.Isolation;

[assembly: TestFramework(IsolationFramework.TypeName, IsolationFramework.AssemblyName)]
```

This will make Xunit tests runs on isolated `AssemblyLoadContext`.

### Reusing `AssemblyLoadContext`

To reuse `AssemblyLoadContext`, use `IsolationContextIdAttribute` or `IsolationContextPoolIdAttribute` attribute. This will let created `AssemblyLoadContext` shared between multiple tests, or let `AssemblyLoadContext`s pooled.

### Limitation

Since we can't load System.Private.CoreLib assembly, every static members in System.Private.CoreLib cannot be isolated between tests. This includes some static properties like:

```C#
int System.Environment.ExitCode
string System.Environment.CurrentDirectory

System.Globalization.CultureInfo System.Globalization.CultureInfo.CurrentCulture
System.Globalization.CultureInfo System.Globalization.CultureInfo.CurrentUICulture
System.Globalization.CultureInfo System.Globalization.CultureInfo.DefaultThreadCurrentCulture
System.Globalization.CultureInfo System.Globalization.CultureInfo.DefaultThreadCurrentUICulture

System.Security.Principal.IPrincipal System.Threading.Thread.CurrentPrincipal

System.Runtime.GCLatencyMode System.Runtime.GCSettings.LatencyMode
System.Runtime.GCLargeObjectHeapCompactionMode System.Runtime.GCSettings.LargeObjectHeapCompactionMode

bool System.Diagnostics.Debug.AutoFlush
int System.Diagnostics.Debug.IndentLevel
int System.Diagnostics.Debug.IndentSize
```