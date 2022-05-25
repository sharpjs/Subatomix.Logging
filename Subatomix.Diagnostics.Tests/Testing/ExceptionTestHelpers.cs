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

namespace Subatomix.Diagnostics.Testing;

/// <summary>
///   Helper methods for exception-related tests.
/// </summary>
internal static class ExceptionTestHelpers
{
    private const string
        DefaultMessage = "A test exception was thrown.";

    /// <summary>
    ///   Creates an exception that has been thrown and caught.
    /// </summary>
    public static Exception Thrown()
    {
        return Thrown(DefaultMessage);
    }

    /// <summary>
    ///   Creates an exception that has been thrown and caught.
    /// </summary>
    /// <param name="innerException">
    ///   The exception that is the cause of the current exception.
    /// </param>
    public static Exception Thrown(Exception innerException)
    {
        return Thrown(DefaultMessage, innerException);
    }

    /// <summary>
    ///   Creates an exception that has been thrown and caught.
    /// </summary>
    /// <param name="message">
    ///   A message that describes the error.
    /// </param>
    public static Exception Thrown(string message)
    {
        try
        {
            throw new InvalidOperationException(message);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    ///   Creates an exception that has been thrown and caught.
    /// </summary>
    /// <param name="message">
    ///   A message that describes the error.
    /// </param>
    /// <param name="innerException">
    ///   The exception that is the cause of the current exception.
    /// </param>
    public static Exception Thrown(string message, Exception innerException)
    {
        try
        {
            throw new InvalidOperationException(message, innerException);
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    ///   Creates an exception that has been thrown and caught.
    /// </summary>
    /// <param name="innerException">
    ///   The exceptions that are the causes of the current exception.
    /// </param>
    public static Exception Thrown(params Exception[] innerExceptions)
    {
        try
        {
            throw new AggregateException(innerExceptions);
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
