---
outputFileName: index.html
---

# Subatomix.Logging

Additions to [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging)
in three NuGet packages:

- [**Subatomix.Logging**](https://www.nuget.org/packages/Subatomix.Logging)
  - [`PrettyConsoleFormatter`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging/Subatomix.Logging.Console.html)
    – A terse, colorful formatter for Microsoft's
      [console logger](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#console).
  - [`DebuggerLogger`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging/Subatomix.Logging.Debugger.html)
    – A logger that sends messages to an attached debugger. In Visual Studio,
      logged messages will appear in the debug output window.
  - [`OperationScope`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging/Subatomix.Logging.OperationScope.html)
    – A scope representing a logical operation.  Automatically logs the start,
      end, duration, and (optionally) the exception thrown from an operation.
  - [`ActivityScope`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging/Subatomix.Logging.ActivityScope.html)
    – An operation scope that also starts and stops an
      [`Activity`](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs).
      The activity carries tags understood by
      [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview),
      which can present the activity as a dependency telemetry item.
- [**Subatomix.Logging.Legacy**](https://www.nuget.org/packages/Subatomix.Logging.Legacy)
  - [`LoggingTraceListener`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging.Legacy/Subatomix.Logging.Legacy.LoggingTraceListener.html)
    – A trace listener plugin for the legacy
      [`TraceSource`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracesource)
      API.  This listener forwards trace events to
      [`ILogger`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger)
      loggers.
  - [`CorrelationManagerActivityListener`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging.Legacy/Subatomix.Logging.Legacy.CorrelationManagerActivityListener.html)
    – A listener that flows
      [`Activity`](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs)
      start and stop events to the legacy
      [`CorrelationManager`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.correlationmanager)
      facility.
  - [Types to assist migration](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging.Legacy/Sharp.Diagnostics.Logging.html)
    from the [Sharp.Diagnostics.Logging](https://github.com/sharpjs/Sharp.Diagnostics.Logging)
    package.
- [**Subatomix.Logging.Sql**](https://www.nuget.org/packages/Subatomix.Logging.Sql)
  - [`SqlLogger`](https://sharpjs.github.io/Subatomix.Logging/api/Subatomix.Logging.Sql/Subatomix.Logging.Sql.html)
    – A logger that writes to a table in SQL Server or Azure SQL Database.

## Status

[![Build](https://github.com/sharpjs/Subatomix.Logging/workflows/Build/badge.svg)](https://github.com/sharpjs/Subatomix.Logging/actions)

Nearing release.

- **New-ish:** New implementation, but based on code with extensive private use in production.
- **Tested:**  100% coverage by automated tests.
- **Documented**<sup>ish</sup>**:**
  - :white_check_mark: IntelliSense on everything.
  - :white_check_mark: [External reference documentation](https://sharpjs.github.io/Subatomix.Logging/).
  - :x:                No tutorial content yet.

## Installation

NuGet packages are available.

Package | Status
--------|-------
[Subatomix.Logging](https://www.nuget.org/packages/Subatomix.Logging)               | [![NuGet](https://img.shields.io/nuget/v/Subatomix.Logging.svg)](https://www.nuget.org/packages/Subatomix.Logging) [![NuGet](https://img.shields.io/nuget/dt/Subatomix.Logging.svg)](https://www.nuget.org/packages/Subatomix.Logging)
[Subatomix.Logging.Legacy](https://www.nuget.org/packages/Subatomix.Logging.Legacy) | [![NuGet](https://img.shields.io/nuget/v/Subatomix.Logging.Legacy.svg)](https://www.nuget.org/packages/Subatomix.Logging.Legacy) [![NuGet](https://img.shields.io/nuget/dt/Subatomix.Logging.Legacy.svg)](https://www.nuget.org/packages/Subatomix.Logging.Legacy)
[Subatomix.Logging.Sql](https://www.nuget.org/packages/Subatomix.Logging.Sql)       | [![NuGet](https://img.shields.io/nuget/v/Subatomix.Logging.Sql.svg)](https://www.nuget.org/packages/Subatomix.Logging.Sql) [![NuGet](https://img.shields.io/nuget/dt/Subatomix.Logging.Sql.svg)](https://www.nuget.org/packages/Subatomix.Logging.Sql)

## Building From Source

Requirements:
- Appropriate .NET SDKs — see the target framework(s) specified in each `.csproj` file.
  - Download [.NET SDKs](https://dotnet.microsoft.com/download/dotnet)
  - Download [.NET Framework Developer Packs](https://dotnet.microsoft.com/download/dotnet-framework)
- Visual Studio 2022 or later (if using Visual Studio).

```powershell
# The default: build and run tests
.\Make.ps1 [-Test] [-Configuration <String>]

# Just build; don't run tests
.\Make.ps1 -Build [-Configuration <String>]

# Build and run tests w/coverage
.\Make.ps1 -Coverage [-Configuration <String>]
```

<!--
  Copyright 2022 Jeffrey Sharp

  Permission to use, copy, modify, and distribute this software for any
  purpose with or without fee is hereby granted, provided that the above
  copyright notice and this permission notice appear in all copies.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
  WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
  MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
  ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
  WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
  ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
  OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
-->
