namespace Subatomix.Diagnostics;

[TestFixture]
internal class StringExtensionsTests
{
    [Test]
    [TestCase(0, ""     )]
    [TestCase(3, "0aA"  )]
    [TestCase(4, "0aAb" )]
    [TestCase(5, "0aAb.")]
    public void FillLettersAndDigits(int n, string expected)
    {
        Span<char> chars = stackalloc char[n];

        "|0|a|A|b|".FillLettersAndDigits(chars);

        chars.ToString().Should().Be(expected);
    }
}
