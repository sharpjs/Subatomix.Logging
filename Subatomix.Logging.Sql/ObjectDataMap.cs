// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;

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
    /// <exception cref="IndexOutOfRangeException">
    ///   There is no field with the specified <paramref name="ordinal"/> in
    ///   the map.
    /// </exception>
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
    ///   There is no field with the specified <paramref name="name"/> in the
    ///   map.
    /// </exception>
    /// <remarks>
    ///   This method uses case-sensitive ordinal comparison.
    /// </remarks>
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
        /// <remarks>
        ///   This overload is used for reference-type and non-nullable
        ///   value-type fields.
        /// </remarks>
        public Builder Field<TField>(string name, string dbType, Func<T, TField?> getter)
            where TField : notnull
        {
            _fields.Add(new GeneralField<TField>(name, dbType, getter));
            return this;
        }

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
        /// <remarks>
        ///   This overload is used for nullable value-type fields.
        /// </remarks>
        public Builder Field<TField>(string name, string dbType, Func<T, TField?> getter)
            where TField : struct
        {
            _fields.Add(new NullableValueField<TField>(name, dbType, getter));
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
        private protected Field(string name, string dbType)
        {
            Name   = name   ?? throw new ArgumentNullException(nameof(name));
            DbType = dbType ?? throw new ArgumentNullException(nameof(dbType));
        }

        /// <summary>
        ///   Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///   Gets the database type name of the field.
        /// </summary>
        public string DbType { get; }

        /// <summary>
        ///   Gets the .NET type of the field.
        /// </summary>
        public abstract Type NetType { get; }

        /// <summary>
        ///   Gets whether the field is <see langword="null"/> for the
        ///   specified object.
        /// </summary>
        /// <param name="obj">
        ///   The object for which to check if the field is
        ///   <see langword="null"/>.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the field is <see langword="null"/> for
        ///   <paramref name="obj"/>; <see langword="false"/> otherwise.
        /// </returns>
        public abstract bool IsNull(T obj);

        /// <summary>
        ///   Gets the value of the field for the specified object.
        /// </summary>
        /// <param name="obj">
        ///   The object for which to get the value of the field.
        /// </param>
        /// <returns>
        ///   The value of the field for <paramref name="obj"/>, or
        ///   <see cref="DBNull.Value"/> if the field is <see langword="null"/>
        ///   for <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public object GetValue(T obj)
            => GetValueUntyped(obj);

        /// <inheritdoc cref="GetValue(T)"/>
        protected abstract object GetValueUntyped(T obj);

        /// <summary>
        ///   Gets the value of the field for the specified object.
        /// </summary>
        /// <typeparam name="TValue">
        ///   The type of value in the field.
        /// </typeparam>
        /// <param name="obj">
        ///   The object for which to get the value of the field.
        /// </param>
        /// <returns>
        ///   The value of the field for <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///   The field is not of type <typeparamref name="TValue"/>, or the
        ///   value of the field is <see langword="null"/> for
        ///   <paramref name="obj"/>.
        /// </exception>
        public TValue GetValueAs<TValue>(T obj)
            where TValue : notnull
        {
            return this is Field<TValue> field
                ? field.GetValue(obj)
                : throw OnCannotCastTo(typeof(TValue));
        }

        protected InvalidCastException OnCannotCastTo(Type type)
            => new($"Unable to cast object of type '{NetType}' to type '{type}'.");

        protected static InvalidCastException OnNullValue()
            => new("Field value is null. This method or property cannot be called on null values.");
    }

    /// <summary>
    ///   A field of known type in an <see cref="ObjectDataMap{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">
    ///   The type of value in the field.
    /// </typeparam>
    public abstract class Field<TValue> : Field
        where TValue : notnull
    {
        private protected Field(string name, string dbType)
            : base(name, dbType) { }

        /// <inheritdoc/>
        public sealed override Type NetType
            => typeof(TValue);

        /// <summary>
        ///   Gets the value of the field for the specified object.
        /// </summary>
        /// <param name="obj">
        ///   The object for which to get the value of the field.
        /// </param>
        /// <returns>
        ///   The value of the field for <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///   The value of the field is <see langword="null"/> for
        ///   <paramref name="obj"/>.
        /// </exception>
        public new abstract TValue GetValue(T obj);
    }

    /// <summary>
    ///   A field of reference or non-nullable value type in an
    ///   <see cref="ObjectDataMap{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">
    ///   The type of value in the field.
    /// </typeparam>
    internal sealed class GeneralField<TValue> : Field<TValue>
        where TValue : notnull
    {
        // NOTE: 'TValue?' here is TValue with a nullable-reference annotation

        private readonly Func<T, TValue?> _getter;

        internal GeneralField(string name, string dbType, Func<T, TValue?> getter)
            : base(name, dbType)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            _getter = getter;
        }

        /// <inheritdoc/>
        public override bool IsNull(T obj)
            => GetValueCore(obj) is null;

        /// <inheritdoc/>
        protected override object GetValueUntyped(T obj)
            => GetValueCore(obj) ?? (object) DBNull.Value;

        /// <inheritdoc cref="ObjectDataMap{T}.Field.GetValue(T)"/>
        public override TValue GetValue(T obj)
            => GetValueCore(obj) ?? throw OnNullValue();

        private TValue? GetValueCore(T obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            return _getter(obj);
        }
    }

    /// <summary>
    ///   A field of nullable value type in an <see cref="ObjectDataMap{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">
    ///   The type of value in the field.
    /// </typeparam>
    internal sealed class NullableValueField<TValue> : Field<TValue>
        where TValue : struct
    {
        // NOTE: 'TValue?' here is Nullable<TValue>

        private readonly Func<T, TValue?> _getter;

        internal NullableValueField(string name, string dbType, Func<T, TValue?> getter)
            : base(name, dbType)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            _getter = getter;
        }

        /// <inheritdoc/>
        public override bool IsNull(T obj)
            => GetValueCore(obj) is null;

        /// <inheritdoc/>
        protected override object GetValueUntyped(T obj)
            => GetValueCore(obj) ?? (object) DBNull.Value;

        /// <inheritdoc cref="ObjectDataMap{T}.Field.GetValue(T)"/>
        public override TValue GetValue(T obj)
            => GetValueCore(obj) ?? throw OnNullValue();

        private TValue? GetValueCore(T obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            return _getter(obj);
        }
    }
}
