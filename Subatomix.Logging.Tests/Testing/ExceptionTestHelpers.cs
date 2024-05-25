// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Testing;

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
            throw new ApplicationException(message);
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
            throw new ApplicationException(message, innerException);
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
