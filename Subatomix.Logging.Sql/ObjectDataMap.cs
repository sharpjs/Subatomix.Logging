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

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Subatomix.Logging.Sql;

/// <summary>
///   A mapping that specifies how a <see cref="ObjectDataReader{T}"/> should
///   project an object of type <typeparamref name="T"/> onto a set of fields.
/// </summary>
/// <typeparam name="T">
///   The type of object to project onto a set of fields.
/// </typeparam>
internal class ObjectDataMap<T> : IReadOnlyList<ObjectDataMap<T>.Field>
{
    private readonly Field[] _fields;

    /// <summary>
    ///   Initializes a new <see cref="ObjectDataMap{T}"/> instance using the
    ///   specified builder delegate.
    /// </summary>
    /// <param name="build">
    ///   A delegate that builds the map.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="build"/> is <see langword="null"/>.
    /// </exception>
    public ObjectDataMap(Action<Builder> build)
    {
        if (build is null)
            throw new ArgumentNullException(nameof(build));

        var builder = new Builder();
        build(builder);
        _fields = builder.Complete();
    }

    /// <summary>
    ///   Gets the count of fields in the map.
    /// </summary>
    public int Count
        => _fields.Length;

    /// <summary>
    ///   Gets field with the specified ordinal.
    /// </summary>
    /// <param name="ordinal">
    ///   The zero-based ordinal of the field.
    /// </param>
    public Field this[int ordinal]
        => _fields[ordinal];

    /// <summary>
    ///   Gets the ordinal of the field with the specified name.
    /// </summary>
    /// <param name="name">
    ///   The name of the field.
    /// </param>
    /// <returns>
    ///   The ordinal of the field with name <paramref name="name"/>.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   There is no field with name <paramref name="name"/> in the map.
    /// </exception>
    public int GetOrdinal(string name)
    {
        // For this purpose, linear search is completely acceptable
        var ordinal = Array.FindIndex(_fields, f => f.Name == name);
        if (ordinal >= 0)
            return ordinal;

        throw new IndexOutOfRangeException(
            "The specified name is not a valid column name."
        );
    }

    /// <inheritdoc/>
    public IEnumerator<Field> GetEnumerator()
        => ((IEnumerable<Field>) _fields).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => _fields.GetEnumerator();

    /// <summary>
    ///   A builder used to initialize a <see cref="ObjectDataMap{T}"/> during
    ///   construction.
    /// </summary>
    public class Builder
    {
        private readonly List<Field> _fields = new();

        /// <summary>
        ///   Adds the specified field to the map.
        /// </summary>
        /// <typeparam name="TField">
        ///   The CLR data type of the field.
        /// </typeparam>
        /// <param name="name">
        ///   The name of the field.
        /// </param>
        /// <param name="dbType">
        ///   The SQL data type of the field.
        /// </param>
        /// <param name="getter">
        ///   A delegate that gets the value of the field from an object of
        ///   type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        ///   The builder, to permit chaining.
        /// </returns>
        public Builder Field<TField>(string name, string dbType, Func<T, TField> getter)
        {
            _fields.Add(new Field<TField>(name, dbType, getter));
            return this;
        }

        /// <summary>
        ///   Completes the builder, returns the fields configured for the map.
        /// </summary>
        /// <returns>
        ///   A new array containing the fields previously configured for the
        ///   map.
        /// </returns>
        internal Field[] Complete()
        {
            return _fields.ToArray();
        }
    }

    /// <summary>
    ///   A field in an <see cref="ObjectDataMap{T}"/>.
    /// </summary>
    public abstract class Field
    {
        protected Field(string name, string dbType)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (dbType == null)
                throw new ArgumentNullException(nameof(dbType));

            Name   = name;
            DbType = dbType;
        }

        /// <summary>
        ///   Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///   Gets the SQL data type name of the field.
        /// </summary>
        public string DbType { get; }

        /// <summary>
        ///   Gets the CLR data type of the field.
        /// </summary>
        public abstract Type NetType { get; }

        public object? GetValue(T obj)
        {
            return GetValueAsObject(obj);
        }

        public TValue GetValueAs<TValue>(T obj)
        {
            return this is Field<TValue> field
                ? field.GetValue(obj)
                : (TValue) GetValueAsObject(obj)!;
        }

        public bool TryGetValueAs<TValue>(T obj, [MaybeNullWhen(false)] out TValue value)
        {
            return this is Field<TValue> field
                ? (value = field.GetValue(obj), ok: true ) .ok
                : (value = default,             ok: false) .ok;
        }

        protected abstract object? GetValueAsObject(T obj);
    }

    /// <summary>
    ///   A field in an <see cref="ObjectDataMap{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">
    ///   The type of value in the field.
    /// </typeparam>
    public class Field<TValue> : Field
    {
        private readonly Func<T, TValue> _getter;

        public Field(string name, string dbType, Func<T, TValue> getter)
            : base(name, dbType)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            _getter = getter;
        }

        /// <inheritdoc/>
        public override Type NetType
            => typeof(TValue);

        public new TValue GetValue(T obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            return _getter(obj);
        }

        protected override object? GetValueAsObject(T obj)
        {
            return GetValue(obj);
        }
    }
}
