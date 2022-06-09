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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics.Logging.Debug;

[TestFixture]
public class DebugLoggingBuilderExtensionsTests
{
    [Test]
    public void AddDebug_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddDebug())
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddDebug_Normal()
    {
        var services = new ServiceCollection();

        var builder = Mock.Of<ILoggingBuilder>(
            b => b.Services == services,
            MockBehavior.Strict
        );

        builder.AddDebug().Should().BeSameAs(builder);

        services                      .Should().HaveCount(1);
        services[0].ServiceType       .Should().Be(typeof(ILoggerProvider));
        services[0].ImplementationType.Should().Be(typeof(DebugLoggerProvider));
    }
}
