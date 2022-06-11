using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Subatomix.Logging.Console;

using static BindingFlags;

internal class PrettyConsoleLoggingBuilderExtensionsTests
{
    [Test]
    public void AddPrettyConsole_DefaultOptions_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddPrettyConsole())
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddPrettyConsole_CustomOptions_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddPrettyConsole(_ => { }))
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddPrettyConsoleFormatter_DefaultOptions_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddPrettyConsoleFormatter())
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddPrettyConsoleFormatter_CustomOptions_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddPrettyConsoleFormatter(_ => { }))
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddPrettyConsole_CustomOptions_NullDelegate()
    {
        Mock.Of<ILoggingBuilder>()
            .Invoking(b => b.AddPrettyConsole(null!))
            .Should().Throw<ArgumentNullException>().WithParameterName("configure");
    }

    [Test]
    public void AddPrettyConsoleFormatter_CustomOptions_NullDelegate()
    {
        Mock.Of<ILoggingBuilder>()
            .Invoking(b => b.AddPrettyConsoleFormatter(null!))
            .Should().Throw<ArgumentNullException>().WithParameterName("configure");
    }

    [Test]
    public void AddPrettyConsole_DefaultOptions_Normal()
    {
        var logger = new ServiceCollection()
            .AddLogging(l => l.AddPrettyConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILoggerProvider>()
            .CreateLogger("");

        ShouldBeConsoleLogger(logger, out var options, out var formatter);

        options.FormatterName.Should().Be(PrettyConsoleFormatter.Name);

        formatter.Should().BeOfType<PrettyConsoleFormatter>();
    }

    [Test]
    public void AddPrettyConsole_CustomOptions_Normal()
    {
        void Configure(PrettyConsoleFormatterOptions options)
            => options.ColorBehavior = LoggerColorBehavior.Disabled;

        var logger = new ServiceCollection()
            .AddLogging(l => l.AddPrettyConsole(Configure))
            .BuildServiceProvider()
            .GetRequiredService<ILoggerProvider>()
            .CreateLogger("");

        ShouldBeConsoleLogger(logger, out var options, out var formatter);

        options.FormatterName.Should().Be(PrettyConsoleFormatter.Name);

        formatter.Should().BeOfType<PrettyConsoleFormatter>()
            .Which.Options.ColorBehavior.Should().Be(LoggerColorBehavior.Disabled);
    }

    [Test]
    public void AddPrettyConsoleFormatter_DefaultOptions_Normal()
    {
        var formatter = new ServiceCollection()
            .AddLogging(l => l.AddPrettyConsoleFormatter())
            .BuildServiceProvider()
            .GetRequiredService<ConsoleFormatter>();

        formatter.Should().BeOfType<PrettyConsoleFormatter>();
    }

    [Test]
    public void AddPrettyConsoleFormatter_CustomOptions_Normal()
    {
        void Configure(PrettyConsoleFormatterOptions options)
            => options.ColorBehavior = LoggerColorBehavior.Disabled;

        var formatter = new ServiceCollection()
            .AddLogging(l => l.AddPrettyConsoleFormatter(Configure))
            .BuildServiceProvider()
            .GetRequiredService<ConsoleFormatter>();

        formatter.Should().BeOfType<PrettyConsoleFormatter>()
            .Which.Options.ColorBehavior.Should().Be(LoggerColorBehavior.Disabled);
    }

    private void ShouldBeConsoleLogger(
        ILogger                  logger,
        out ConsoleLoggerOptions options,
        out ConsoleFormatter     formatter)
    {
        logger.Should().BeOfType(Type.GetType(
            "Microsoft.Extensions.Logging.Console.ConsoleLogger, " +
            "Microsoft.Extensions.Logging.Console"
        ));

        options   = ShouldHaveProperty<ConsoleLoggerOptions>(logger, "Options"  );
        formatter = ShouldHaveProperty<ConsoleFormatter    >(logger, "Formatter");
    }

    private T ShouldHaveProperty<T>(object obj, string name)
    {
        return obj
            .GetType()
            .GetProperty(name, Instance | NonPublic)
                .Should().NotBeNull()
            .And.Subject.GetValue(obj)
                .Should().BeAssignableTo<T>()
            .Which;
    }
}
