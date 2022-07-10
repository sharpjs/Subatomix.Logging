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

    // For testing
    internal void SimulateUnmanagedDisposal()
        => Dispose(managed: false);

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
        => -1; // Expected value for SELECT statements

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
        => _map[ordinal].IsNull(_rows.Current);

    /// <inheritdoc/>
    public override object GetValue(int ordinal)
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
        where TValue : notnull
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
        where TValue : notnull
    {
        var data       = _map[ordinal].GetValueAs<TValue[]>(_rows.Current);
        var dataLength = data.LongLength;

        if (buffer is null)
            return dataLength;

        if (dataOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(dataOffset));
        if (bufferOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferOffset));
        if (maxLength < 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength));

        if (dataOffset >= dataLength)
            return 0;

        // Convert dataLength to actual length of read
        dataLength = Math.Min(dataLength - dataOffset, maxLength);

        if (buffer.Length - bufferOffset < dataLength)
            throw new ArgumentException("Insufficient buffer length after offset.", nameof(buffer));

        Array.Copy(data, dataOffset, buffer, bufferOffset, dataLength);
        return dataLength;
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
        => new DbEnumerator(this);

    /// <inheritdoc/>
    public override DataTable GetSchemaTable()
    {
        var schema     = new DataTable();
        var colIsKey   = schema.Columns.Add("IsKey",            typeof(bool)  ); // required [0]
        var colOrdinal = schema.Columns.Add("ColumnOrdinal",    typeof(int)   ); // required [0]
        var colName    = schema.Columns.Add("ColumnName",       typeof(string)); // required [1]
        var colType    = schema.Columns.Add("DataType",         typeof(Type)  ); // required [1]
        var colSize    = schema.Columns.Add("ColumnSize",       typeof(long)  ); // required [1] for [var]binary, [n][var]char
        var colPrec    = schema.Columns.Add("NumericPrecision", typeof(byte)  ); // required [1] for decimal
        var colScale   = schema.Columns.Add("NumericScale",     typeof(byte)  ); // required [1] for decimal, time, datetime{2|offset}

        // [0]: required by SqlParameter.GetActualFieldsAndProperties
        //        https://github.com/dotnet/SqlClient/blob/v4.1.0/src/Microsoft.Data.SqlClient/netcore/src/Microsoft/Data/SqlClient/SqlParameter.cs#L1351
        // [1]: required by MetadataUtilsSmi.SmiMetaDataFromSchemaTableRow
        //        https://github.com/dotnet/SqlClient/blob/v4.1.0/src/Microsoft.Data.SqlClient/netcore/src/Microsoft/Data/SqlClient/Server/MetadataUtilsSmi.cs#L719

        foreach (var field in _map)
        {
            var type
                =  Nullable.GetUnderlyingType(field.NetType)
                ?? field.NetType;

            var row         = schema.NewRow();
            row[colIsKey]   = DBNull.Value; // assume not a key column
            row[colOrdinal] = DBNull.Value; // assume in column order
            row[colName]    = field.Name;
            row[colType]    = type;
            row[colSize]    = DBNull.Value; // assume maximum size
            row[colPrec]    = DBNull.Value; // assume default precision: 18 for decimal
            row[colScale]   = DBNull.Value; // assume default scale: 0 for decimal, 7 for time
            schema.Rows.Add(row);
        }

        return schema;
    }
}
