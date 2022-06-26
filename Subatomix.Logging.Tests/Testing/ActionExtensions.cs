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
