namespace Subatomix.Logging.Legacy;

[TestFixture]
public class TraceEventTypeExtensionsTests
{
    [Test]
    [TestCase(TraceEventType.Critical,    LogLevel.Critical)]
    [TestCase(TraceEventType.Error,       LogLevel.Error)]
    [TestCase(TraceEventType.Warning,     LogLevel.Warning)]
    [TestCase(TraceEventType.Information, LogLevel.Information)]
    [TestCase(TraceEventType.Verbose,     LogLevel.Debug)]
    [TestCase(TraceEventType.Start,       LogLevel.Information)]
    [TestCase(TraceEventType.Stop,        LogLevel.Information)]
    [TestCase(TraceEventType.Suspend,     LogLevel.Information)]
    [TestCase(TraceEventType.Resume,      LogLevel.Information)]
    [TestCase(TraceEventType.Transfer,    LogLevel.Information)]
    [TestCase(-1,                         LogLevel.Information)]
    public void ToLogLevel(TraceEventType traceEventType, LogLevel logLevel)
    {
        traceEventType.ToLogLevel().Should().Be(logLevel);
    }
}
