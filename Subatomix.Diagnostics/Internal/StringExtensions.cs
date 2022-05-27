namespace Subatomix.Diagnostics;

internal static class StringExtensions
{
    internal static void FillLettersAndDigits(this string s, Span<char> chars)
    {
        var index = 0;

        if (index >= chars.Length)
            return;

        foreach (char c in s)
        {
            if (!char.IsLetterOrDigit(c))
                continue;

            chars[index++] = c;

            if (index >= chars.Length)
                return;
        }

        chars.Slice(index).Fill('.');
    }
}
