/*
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
*/

using System.Diagnostics;

namespace Subatomix.Diagnostics;

using static ActivityIdFormat;

internal static class ActivityExtensions
{
    internal static void SetStatusIfUnset(this Activity activity, Exception? exception)
    {
        if (activity is null)
            throw new ArgumentNullException(nameof(activity));

        if (activity.Status != ActivityStatusCode.Unset)
            return;

        var (code, description) = exception is not null
            ? (ActivityStatusCode.Error, exception.Message)
            : (ActivityStatusCode.Ok,    null);

        activity.SetStatus(code, description);
    }

    internal static void SetTelemetryTags(this Activity activity)
    {
        if (activity is null)
            throw new ArgumentNullException(nameof(activity));

        // Set tags for consumption by the DependencyTrackingTelemetryModule of
        // Application Insights (AI).  The module, properly configured, will
        // report each Activity as a DependencyTelemetry item.
        //
        // Sources:
        // - https://github.com/microsoft/ApplicationInsights-dotnet/blob/2.20.0/WEB/Src/DependencyCollector/DependencyCollector/TelemetryDiagnosticSourceListener.cs
        // - https://github.com/opentracing/specification/blob/master/semantic_conventions.md
        //
        // Correspondence of telemetry properties to activity tags:
        // - telemetry.Type       = Tags["peer.service"]  ?? Tags["component"]     ?? diagnosticSource.Name
        // - telemetry.Target     = Tags["peer.hostname"] ?? Tags["http.url"].Host ?? Tags["peer.address"]
        // - telemetry.Name       = activity.OperationName
        // - telemetry.Data       = Tags["db.statement"]  ?? Tags["http.url"]
        // - telemetry.Success    = !bool.Parse(Tags["error"])
        // - telemetry.ResultCode = Tags["http.status_code"]

        // Set telemetry.Type indirectly via tag.
        //
        // Evidence (but not documentation) suggests that AI recognizes the
        // dependency type 'InProc' as an internal operation.  Specifically:
        //
        // - https://github.com/microsoft/ApplicationInsights-dotnet/blob/2.20.0/WEB/Src/DependencyCollector/DependencyCollector/Implementation/RemoteDependencyConstants.cs
        //   https://github.com/microsoft/ApplicationInsights-dotnet/blob/2.20.0/WEB/Src/DependencyCollector/DependencyCollector/Implementation/AzureSdk/AzureSdkDiagnosticsEventHandler.cs
        //   AI SDK RemoteDependencyConstants defines an InProc constant.  The
        //   class AzureSdkDiagnosticsEventHandler has a comment mentioning the
        //   value's use for an "internal operation".
        //
        // - https://github.com/microsoft/ApplicationInsights-dotnet/pull/1300#issuecomment-553998634
        //   "Internal spans ... should be dependency telemetry with kind
        //   InProc".
        //
        // - https://github.com/microsoft/ApplicationInsights-dotnet/issues/1199#issuecomment-525109854
        //   "RemoteDependency with Type 'InProc' is exactly what you need to
        //   do - UX will treat it accordingly. Unfortunately, there is no
        //   public doc on this yet."
        //
        // - https://github.com/Azure/azure-sdk-for-net/pull/23541
        //   "InProc type should be used for all the in-process spans".
        // 
        // - https://docs.rs/opentelemetry-application-insights/0.20.0/opentelemetry_application_insights/
        //   "for INTERNAL Spans the Dependency Type is always 'InProc'".
        //
        // - https://github.com/Azure/azure-sdk-for-java/blob/main/sdk/monitor/azure-monitor-opentelemetry-exporter/src/main/java/com/azure/monitor/opentelemetry/exporter/AzureMonitorTraceExporter.java
        //   Java package azure-monitor-opentelemetry-exporter has file
        //   AzureMonitorTraceExporter.java that uses 'InProc' as the type for
        //   internal operations.  The file also contains a comment stating
        //   that AI's App Map feature knows to ignore InProc dependencies.
        // 
        activity.SetTag("peer.service", "InProc");

        // Set telemetry.Target indirectly via tag.
        //
        // Experiments show that the AI portal UI uses this value in various
        // ways:
        //
        // - Transaction search: displays the Target value as the item content.
        //
        // - End-to-end transaction details, view all telemetry: displays the
        //   the Name value, but with the Target value removed if Target is a
        //   prefix of Name.
        //
        // - End-to-end transaction details, item properties pane: displays the
        //   Target value as the 'Base name' property.
        //
        activity.SetTag("peer.hostname", "internal");

        // Set telemetry.Data indirectly via tag.
        //
        // Experiments show that the AI portal UI displays this value in a
        // monospace box labeled 'Command' beneath the item properties.
        //
        activity.SetTag("db.statement", $"Activity: {activity.OperationName}");

        // Set telemetry.Success indirectly via tag.
        //
        // Experiments show that in the AI portal UI, this value controls the
        // the 'Call status' item property and the color of the timeline item.
        //
        activity.SetTag("error", activity.Status == ActivityStatusCode.Error ? "true" : "false");
    }

    internal static void GetRootOperationId(this Activity activity, Span<char> chars)
    {
        activity
            .GetRootOperationId()
            .FillLettersAndDigits(chars);
    }

    internal static string GetRootOperationId(this Activity activity)
    {
        if (activity is null)
            throw new ArgumentNullException(nameof(activity));

        return activity.IdFormat == W3C
            ? activity.GetRootOperationIdW3C()
            : activity.GetRootOperationIdFallback();
    }

    internal static Guid GetRootOperationGuid(this Activity activity)
    {
        if (activity is null)
            throw new ArgumentNullException(nameof(activity));

        return activity.IdFormat == W3C
            ? ParseHexAsGuid(activity.GetRootOperationIdW3C())
            : SynthesizeGuid(activity.GetRootOperationIdFallback());
    }

    private static string GetRootOperationIdW3C(this Activity activity)
    {
        var id = activity.TraceId;
        if (id == default)
            throw MakeNotStartedException();

        return id.ToHexString();
    }

    private static string GetRootOperationIdFallback(this Activity activity)
    {
        var id = activity.RootId;
        if (id is null)
            throw MakeNotStartedException();

        return id;
    }

    private static Guid ParseHexAsGuid(string s)
    {
        const string HexDigitsFormat = "N";

        return Guid.ParseExact(s, HexDigitsFormat);
    }

    internal static Guid SynthesizeGuid(string s)
    {
        // NOTE: The GUIDs generated by this method are not even remotely
        // cryptographically secure.  Do NOT use this for cryptography.

        // 'reg' is a linear feedback shift register.
        // Info: https://en.wikipedia.org/wiki/Linear-feedback_shift_register
        ulong reg = 0xD91E_8A62_B35A_7D6E; // meaningless nonzero seed

        // 'bit' has the XOR of register bits 0, 2, 5, and 7 in its bit 0.
        // On each iteration, this bit shifts into the high bit of 'reg'.
        ulong bit;

        // Consume the input, injecting its entropy into the LFSR
        foreach (var character in s.AsSpan())
        {
            bit  = (reg >> 0) ^ (reg >> 2) ^ (reg >> 5) ^ (reg >> 7);
            reg  = (reg >> 1) | (bit << 63);
            reg += character;
        }

        // Begin assembling the GUID
        Span<byte> bytes = stackalloc byte[16];

        // Run the LFSR to generate a GUID's worth of pseudo-random bytes
        for (var index = 0; index < bytes.Length; index++)
        {
            // 8 rounds of LFSR generates 8 random bits
            for (var round = 0; round < 8; round++)
            {
                bit = (reg >> 0) ^ (reg >> 2) ^ (reg >> 5) ^ (reg >> 7);
                reg = (reg >> 1) | (bit << 63);
            }

            bytes[index] = (byte) reg;
        }

        // Microsoft GUIDs actually are 'version 4, variant 1' UUIDs as
        // described by RFC4122 and other standards.  Compliant GUIDs have
        // the following format:
        // 
        // xxxxxxxx-xxxx-Mxxx-Nxxx-xxxxxxxxxxxx
        //
        // where
        //   M is 4
        //   N is 8, 9, A, or B

        // Fixup to become RFC4122-compliant
        bytes[7] &= 0b_00001111;
        bytes[7] |= 0b_01000000;
        bytes[8] &= 0b_00111111;
        bytes[8] |= 0b_10000000;

        // Assemble the GUID
#if NET6_0_OR_GREATER
        return new Guid(bytes);
#else
        return new Guid(bytes.ToArray());
#endif
    }

    private static Exception MakeNotStartedException()
    {
        return new InvalidOperationException(
            "Cannot get root operation id for an unstarted Action."
        );
    }
}
