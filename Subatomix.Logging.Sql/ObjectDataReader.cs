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
using System.Data;
using System.Data.Common;

namespace Subatomix.Logging.Sql;

/// <summary>
///   A <see cref="DbDataReader"/> implementation that reads from a collection
///   of objects.
/// </summary>
/// <typeparam name="T">
///   The type of items in the underlying collection.
/// </typeparam>
internal class ObjectDataReader<T> : DbDataReader
{
    private readonly ObjectDataMap<T> _map;
    private readonly IEnumerator<T>   _rows;
    private readonly bool             _hasRows;

    /// <summary>
    ///   Initializes a new <see cref="ObjectDataReader{T}"/> instance that
    ///   reads from the specified collection using the specified map.
    /// </summary>
    /// <param name="source">
    ///   The collection from which to read.
    /// </param>
    /// <param name="map">
    ///   A mapping that specifies how the reader should project an object of
    ///   type <typeparamref name="T"/> onto a set of fields.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="source"/> and/or <paramref name="map"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public ObjectDataReader(IEnumerable<T> source, ObjectDataMap<T> map)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (map == null)
            throw new ArgumentNullException(nameof(map));

        _map     = map;
        _hasRows = source.Any();
        _rows    = source.GetEnumerator();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool managed)
    {
        if (managed)
            _rows.Dispose();

        base.Dispose(managed);
    }

    /// <inheritdoc/>
    public override int FieldCount
        => _map.Count;

    /// <inheritdoc/>
    public override bool HasRows
        => _hasRows;

    /// <inheritdoc/>
    public override bool IsClosed
        => false; // Cannot be closed

    /// <inheritdoc/>
    public override int Depth
        => 0; // Cannot be nested

    /// <inheritdoc/>
    public override int RecordsAffected
        => -1; // Expected value for SELECT queries

    /// <inheritdoc/>
    public override object this[int ordinal]
        => GetValue(ordinal);

    /// <inheritdoc/>
    public override object this[string name]
        => GetValue(GetOrdinal(name));

    /// <inheritdoc/>
    public override bool NextResult()
    {
        while (_rows.MoveNext()) /* do nothing */;
        return false;
    }

    /// <inheritdoc/>
    public override bool Read()
        => _rows.MoveNext();

    /// <inheritdoc/>
    public override string GetName(int ordinal)
        => _map[ordinal].Name;
    
    /// <inheritdoc/>
    public override Type GetFieldType(int ordinal)
        => _map[ordinal].NetType;

    /// <inheritdoc/>
    public override string GetDataTypeName(int ordinal)
        => _map[ordinal].DbType;

    /// <inheritdoc/>
    public override int GetOrdinal(string name)
        => _map.GetOrdinal(name);

    /// <inheritdoc/>
    public override bool IsDBNull(int ordinal)
        => GetValueAsObject(ordinal) is null;

    /// <inheritdoc/>
    public override object GetValue(int ordinal)
        => GetValueAsObject(ordinal) ?? DBNull.Value;

    private object? GetValueAsObject(int ordinal)
        => _map[ordinal].GetValue(_rows.Current);

    /// <inheritdoc/>
    public override string GetString(int ordinal)
        => GetValueAs<string>(ordinal);

    /// <inheritdoc/>
    public override bool GetBoolean(int ordinal)
        => GetValueAs<bool>(ordinal);

    /// <inheritdoc/>
    public override byte GetByte(int ordinal)
        => GetValueAs<byte>(ordinal);

    /// <inheritdoc/>
    public override char GetChar(int ordinal)
        => GetValueAs<char>(ordinal);

    /// <inheritdoc/>
    public override short GetInt16(int ordinal)
        => GetValueAs<short>(ordinal);

    /// <inheritdoc/>
    public override int GetInt32(int ordinal)
        => GetValueAs<int>(ordinal);

    /// <inheritdoc/>
    public override long GetInt64(int ordinal)
        => GetValueAs<long>(ordinal);

    /// <inheritdoc/>
    public override float GetFloat(int ordinal)
        => GetValueAs<float>(ordinal);

    /// <inheritdoc/>
    public override double GetDouble(int ordinal)
        => GetValueAs<double>(ordinal);

    /// <inheritdoc/>
    public override decimal GetDecimal(int ordinal)
        => GetValueAs<decimal>(ordinal);

    /// <inheritdoc/>
    public override Guid GetGuid(int ordinal)
        => GetValueAs<Guid>(ordinal);

    /// <inheritdoc/>
    public override DateTime GetDateTime(int ordinal)
        => GetValueAs<DateTime>(ordinal);

    private TValue GetValueAs<TValue>(int ordinal)
        => _map[ordinal].GetValueAs<TValue>(_rows.Current);

    /// <inheritdoc/>
    public override long GetBytes(
        int     ordinal,
        long    dataOffset,
        byte[]? buffer,
        int     bufferOffset,
        int     length)
        => GetArray(ordinal, dataOffset, buffer, bufferOffset, length);

    /// <inheritdoc/>
    public override long GetChars(
        int     ordinal,
        long    dataOffset,
        char[]? buffer,
        int     bufferOffset,
        int     length)
        => GetArray(ordinal, dataOffset, buffer, bufferOffset, length);

    private long GetArray<TValue>(
        int       ordinal,
        long      dataOffset,
        TValue[]? buffer,
        int       bufferOffset,
        int       maxLength)
        where TValue : struct
    {
        if (!_map[ordinal].TryGetValueAs<TValue[]>(_rows.Current, out var data))
            return 0;

        var dataLength = data.LongLength;

        if (buffer is null)
            return dataLength;

        var bufferLength = buffer.Length;

        if ( dataOffset   < 0 || dataOffset   >= dataLength   ||
             bufferOffset < 0 || bufferOffset >= bufferLength ||
             maxLength    < 0 )
            return 0;

        dataLength   -= dataOffset;
        bufferLength -= bufferOffset;

        var length = Math.Min(maxLength, Math.Min(dataLength, bufferLength));

        Array.Copy(data, dataOffset, buffer, bufferOffset, length);
        return length;
    }

    /// <inheritdoc/>
    public override int GetValues(object[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var length = Math.Min(FieldCount, values.Length);

        for (var i = 0; i < length; i++)
            values[i] = GetValue(i);

        return length;
    }

    /// <inheritdoc/>
    public override IEnumerator GetEnumerator()
    {
        return new DbEnumerator(this);
    }

    /// <inheritdoc/>
    public override DataTable GetSchemaTable()
    {
        var schema     = new DataTable();
        var colIsKey   = schema.Columns.Add("IsKey",            typeof(bool)  );
        var colOrdinal = schema.Columns.Add("ColumnOrdinal",    typeof(int)   );
        var colName    = schema.Columns.Add("ColumnName",       typeof(string));
        var colType    = schema.Columns.Add("DataType",         typeof(Type)  );
        var colSize    = schema.Columns.Add("ColumnSize",       typeof(long)  );
        var colPrec    = schema.Columns.Add("NumericPrecision", typeof(byte)  );
        var colScale   = schema.Columns.Add("NumericScale",     typeof(byte)  );

        foreach (var field in _map)
        {
            var type
                =  Nullable.GetUnderlyingType(field.NetType)
                ?? field.NetType;

            var row         = schema.NewRow();
            row[colIsKey]   = false;
            row[colOrdinal] = DBNull.Value; // Consumer will fill this in
            row[colName]    = field.Name;
            row[colType]    = type;
            row[colSize]    = DBNull.Value; // Consumer will fill this in
            row[colPrec]    = DBNull.Value; // Consumer will fill this in
            row[colScale]   = DBNull.Value; // Consumer will fill this in
            schema.Rows.Add(row);
        }

        return schema;
    }
}
