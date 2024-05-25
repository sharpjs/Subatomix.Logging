// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;

namespace Subatomix.Logging.Testing;

internal static class ActionExtensions
{
    [return: NotNullIfNotNull("action")] 
    public static Action<Arg>? Expecting(this Action? action, Arg arg)
    {
        if (action is null)
            return null;

        return a =>
        {
            a.Should().BeSameAs(arg);
            action();
        };
    }

    [return: NotNullIfNotNull("action")] 
    public static Func<Result>? Returning(this Action? action, Result result)
    {
        if (action is null)
            return null;

        return () =>
        {
            action();
            return result;
        };
    }

    [return: NotNullIfNotNull("action")] 
    public static Func<Arg, Result>? Returning(this Action<Arg>? action, Result result)
    {
        if (action is null)
            return null;

        return a =>
        {
            action(a);
            return result;
        };
    }

    [return: NotNullIfNotNull("action")] 
    public static Func<Arg, Task>? Expecting(this Func<Task>? action, Arg arg)
    {
        if (action is null)
            return null;

        return async a =>
        {
            a.Should().BeSameAs(arg);
            await action();
        };
    }

    [return: NotNullIfNotNull("action")] 
    public static Func<Task<Result>>? Returning(this Func<Task>? action, Result result)
    {
        if (action is null)
            return null;

        return async () =>
        {
            await action();
            return result;
        };
    }

    [return: NotNullIfNotNull("action")] 
    public static Func<Arg, Task<Result>>? Returning(this Func<Arg, Task>? action, Result result)
    {
        if (action is null)
            return null;

        return async a =>
        {
            await action(a);
            return result;
        };
    }
}
