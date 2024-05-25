// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Data.Common;

namespace Subatomix.Logging.Sql;

[TestFixture]
public class ObjectDataReaderTests
{
    [Test]
    public void Construct_NullSource()
    {
        Invoking(() => new ObjectDataReader<Thing>(null!, Thing.Map))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullMap()
    {
        Invoking(() => new ObjectDataReader<Thing>(new Thing[0], null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Dispose_Managed()
    {
        using var h = new DisposalTestHarness();

        h.Enumerator
            .Setup(e => e.Dispose())
            .Verifiable();

        ((IDisposable) h.Reader).Dispose();
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var h = new DisposalTestHarness();

        h.Reader.SimulateUnmanagedDisposal();
    }

    [Test]
    public void FieldCount_Get()
    {
        using var reader = MakeReader();

        reader.FieldCount.Should().Be(14);
    }

    [Test]
    public void HasRows_Get_False()
    {
        using var reader = MakeReader();

        reader.HasRows.Should().BeFalse();
    }

    [Test]
    public void HasRows_Get_True()
    {
        using var reader = MakeReader(new Thing());

        reader.HasRows.Should().BeTrue();
    }

    [Test]
    public void IsClosed_Get()
    {
        using var reader = MakeReader();

        reader.IsClosed.Should().BeFalse();

        reader.Close(); // does nothing

        reader.IsClosed.Should().BeFalse();
    }

    [Test]
    public void Depth_Get()
    {
        using var reader = MakeReader();

        // Cannot be nested
        reader.Depth.Should().Be(0);
    }

    [Test]
    public void RecordsAffected_Get()
    {
        using var reader = MakeReader();

        // Expected value for SELECT statements
        reader.RecordsAffected.Should().Be(-1);
    }

    [Test]
    public void Item_GetByOrdinal()
    {
        using var reader  = MakeReader(new Thing { ByteProperty = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.Read();
        reader[ordinal].Should().Be(42);
    }

    [Test]
    public void Item_GetByName()
    {
        using var reader = MakeReader(new Thing { ByteProperty = 42 });

        reader.Read();
        reader[nameof(Thing.ByteProperty)].Should().Be(42);
    }

    [Test]
    public void NextResult()
    {
        using var reader = MakeReader(new Thing());

        reader.NextResult().Should().BeFalse();
    }

    [Test]
    public void Read()
    {
        using var reader = MakeReader(new Thing());

        reader.Read().Should().BeTrue();
        reader.Read().Should().BeFalse();
        reader.Read().Should().BeFalse();
    }

    [Test]
    public void GetName()
    {
        using var reader  = MakeReader();
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.GetName(ordinal).Should().Be(nameof(Thing.ByteProperty));
    }

    [Test]
    public void GetFieldType()
    {
        using var reader  = MakeReader();
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.GetFieldType(ordinal).Should().Be(typeof(byte));
    }

    [Test]
    public void GetDataTypeName()
    {
        using var reader  = MakeReader();
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.GetDataTypeName(ordinal).Should().Be("tinyint");
    }

    [Test]
    public void IsDBNull_ReferenceType_False()
    {
        using var reader = MakeReader(new Thing { StringProperty = "not null" });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.IsDBNull(ordinal).Should().BeFalse();
    }

    [Test]
    public void IsDBNull_ReferenceType_True()
    {
        using var reader = MakeReader(new Thing { StringProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.IsDBNull(ordinal).Should().BeTrue();
    }

    [Test]
    public void IsDBNull_ValueType_False()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.IsDBNull(ordinal).Should().BeFalse();
    }

    [Test]
    public void IsDBNull_ValueType_True()
    {

        using var reader  = MakeReader(new Thing { Int32Property = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.IsDBNull(ordinal).Should().BeTrue();
    }

    [Test]
    public void GetValue_ReferenceType_NotNull()
    {
        using var reader  = MakeReader(new Thing { StringProperty = "a" });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.GetValue(ordinal).Should().BeOfType<string>().And.Be("a");
    }

    [Test]
    public void GetValue_ReferenceType_Null()
    {
        using var reader  = MakeReader(new Thing { StringProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.GetValue(ordinal).Should().BeSameAs(DBNull.Value);
    }

    [Test]
    public void GetValue_ValueType_NotNull()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.GetValue(ordinal).Should().BeOfType<int>().And.Be(42);
    }

    [Test]
    public void GetValue_ValueType_Null()
    {
        using var reader  = MakeReader(new Thing { Int32Property = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.GetValue(ordinal).Should().BeSameAs(DBNull.Value);
    }

    [Test]
    public void GetString_NotNull()
    {
        using var reader  = MakeReader(new Thing { StringProperty = "a" });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.GetString(ordinal).Should().Be("a");
    }

    [Test]
    public void GetString_Null()
    {
        using var reader  = MakeReader(new Thing { StringProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.Invoking(r => r.GetString(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetString_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetString(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.String'.");
    }

    [Test]
    public void GetBoolean_NotNull()
    {
        using var reader  = MakeReader(new Thing { BooleanProperty = true });
              var ordinal = reader.GetOrdinal(nameof(Thing.BooleanProperty));

        reader.Read();
        reader.GetBoolean(ordinal).Should().BeTrue();
    }

    [Test]
    public void GetBoolean_Null()
    {
        using var reader  = MakeReader(new Thing { BooleanProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.BooleanProperty));

        reader.Read();
        reader.Invoking(r => r.GetBoolean(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetBoolean_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetBoolean(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Boolean'.");
    }

    [Test]
    public void GetByte_NotNull()
    {
        using var reader  = MakeReader(new Thing { ByteProperty = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.Read();
        reader.GetByte(ordinal).Should().Be(42);
    }

    [Test]
    public void GetByte_Null()
    {
        using var reader  = MakeReader(new Thing { ByteProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteProperty));

        reader.Read();
        reader.Invoking(r => r.GetByte(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetByte_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetByte(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Byte'.");
    }

    [Test]
    public void GetChar_NotNull()
    {
        using var reader  = MakeReader(new Thing { CharProperty = 'a' });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharProperty));

        reader.Read();
        reader.GetChar(ordinal).Should().Be('a');
    }

    [Test]
    public void GetChar_Null()
    {
        using var reader  = MakeReader(new Thing { CharProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharProperty));

        reader.Read();
        reader.Invoking(r => r.GetChar(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetChar_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetChar(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Char'.");
    }

    [Test]
    public void GetInt16_NotNull()
    {
        using var reader  = MakeReader(new Thing { Int16Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int16Property));

        reader.Read();
        reader.GetInt16(ordinal).Should().Be(42);
    }

    [Test]
    public void GetInt16_Null()
    {
        using var reader  = MakeReader(new Thing { Int16Property = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int16Property));

        reader.Read();
        reader.Invoking(r => r.GetInt16(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetInt16_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetInt16(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Int16'.");
    }

    [Test]
    public void GetInt32_NotNull()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.GetInt32(ordinal).Should().Be(42);
    }

    [Test]
    public void GetInt32_Null()
    {
        using var reader  = MakeReader(new Thing { Int32Property = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetInt32(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetInt32_WrongType()
    {
        using var reader  = MakeReader(new Thing { StringProperty = "a" });
              var ordinal = reader.GetOrdinal(nameof(Thing.StringProperty));

        reader.Read();
        reader.Invoking(r => r.GetInt32(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.String' to type 'System.Int32'.");
    }

    [Test]
    public void GetInt64_NotNull()
    {
        using var reader  = MakeReader(new Thing { Int64Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int64Property));

        reader.Read();
        reader.GetInt64(ordinal).Should().Be(42);
    }

    [Test]
    public void GetInt64_Null()
    {
        using var reader  = MakeReader(new Thing { Int64Property = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int64Property));

        reader.Read();
        reader.Invoking(r => r.GetInt64(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetInt64_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetInt64(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Int64'.");
    }

    [Test]
    public void GetFloat_NotNull()
    {
        using var reader  = MakeReader(new Thing { SingleProperty = 42.42f });
              var ordinal = reader.GetOrdinal(nameof(Thing.SingleProperty));

        reader.Read();
        reader.GetFloat(ordinal).Should().Be(42.42f);
    }

    [Test]
    public void GetFloat_Null()
    {
        using var reader  = MakeReader(new Thing { SingleProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.SingleProperty));

        reader.Read();
        reader.Invoking(r => r.GetFloat(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetFloat_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetFloat(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Single'.");
    }

    [Test]
    public void GetDouble_NotNull()
    {
        using var reader  = MakeReader(new Thing { DoubleProperty = 42.42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.DoubleProperty));

        reader.Read();
        reader.GetDouble(ordinal).Should().Be(42.42);
    }

    [Test]
    public void GetDouble_Null()
    {
        using var reader  = MakeReader(new Thing { DoubleProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.DoubleProperty));

        reader.Read();
        reader.Invoking(r => r.GetDouble(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetDouble_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetDouble(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Double'.");
    }

    [Test]
    public void GetDecimal_NotNull()
    {
        using var reader  = MakeReader(new Thing { DecimalProperty = 42.42m });
              var ordinal = reader.GetOrdinal(nameof(Thing.DecimalProperty));

        reader.Read();
        reader.GetDecimal(ordinal).Should().Be(42.42m);
    }

    [Test]
    public void GetDecimal_Null()
    {
        using var reader  = MakeReader(new Thing { DecimalProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.DecimalProperty));

        reader.Read();
        reader.Invoking(r => r.GetDecimal(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetDecimal_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetDecimal(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Decimal'.");
    }

    [Test]
    public void GetGuid_NotNull()
    {
        var value = Guid.NewGuid();

        using var reader  = MakeReader(new Thing { GuidProperty = value });
              var ordinal = reader.GetOrdinal(nameof(Thing.GuidProperty));

        reader.Read();
        reader.GetGuid(ordinal).Should().Be(value);
    }

    [Test]
    public void GetGuid_Null()
    {
        using var reader  = MakeReader(new Thing { GuidProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.GuidProperty));

        reader.Read();
        reader.Invoking(r => r.GetGuid(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetGuid_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetGuid(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Guid'.");
    }

    [Test]
    public void GetDateTime_NotNull()
    {
        var value = DateTime.UtcNow;

        using var reader  = MakeReader(new Thing { DateTimeProperty = value });
              var ordinal = reader.GetOrdinal(nameof(Thing.DateTimeProperty));

        reader.Read();
        reader.GetDateTime(ordinal).Should().Be(value);
    }

    [Test]
    public void GetDateTime_Null()
    {
        using var reader  = MakeReader(new Thing { DateTimeProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.DateTimeProperty));

        reader.Read();
        reader.Invoking(r => r.GetDateTime(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetDateTime_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetDateTime(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.DateTime'.");
    }

    [Test]
    public void GetBytes_NotNull_LimitedByMaxSize()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader.GetBytes(ordinal, 0, target, 0, 2).Should().Be(2);

        target.Should().Equal(1, 3);
    }

    [Test]
    public void GetBytes_NotNull_LimitedByDataOffset()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader.GetBytes(ordinal, 3, target, 1, 42).Should().Be(1);

        target.Should().Equal(0, 7);
    }

    [Test]
    public void GetBytes_NotNull_NullBuffer()
    {
        var source = new byte[] { 1, 3, 3, 7 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader.GetBytes(ordinal, -1, null, -1, -1).Should().Be(4);
    }

    [Test]
    public void GetBytes_NotNull_NegativeDataOffset()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetBytes(ordinal, -1, target, 0, 0))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dataOffset");

        target.Should().Equal(0, 0);
    }

    [Test]
    public void GetBytes_NotNull_NegativeBufferOffset()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetBytes(ordinal, 0, target, -1, 0))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("bufferOffset");

        target.Should().Equal(0, 0);
    }

    [Test]
    public void GetBytes_NotNull_NegativeMaxLength()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetBytes(ordinal, 0, target, 0, -1))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxLength");

        target.Should().Equal(0, 0);
    }

    [Test]
    public void GetBytes_NotNull_DataOffsetBeyondData()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader.GetBytes(ordinal, 4, target, 42, 42).Should().Be(0);

        target.Should().Equal(0, 0);
    }

    [Test]
    public void GetBytes_NotNull_InsufficientBuffer()
    {
        var source = new byte[] { 1, 3, 3, 7 };
        var target = new byte[] { 0, 0 };

        using var reader  = MakeReader(new Thing { ByteArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetBytes(ordinal, 1, target, 0, 3))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("buffer");

        target.Should().Equal(0, 0);
    }

    [Test]
    public void GetBytes_Null()
    {
        using var reader  = MakeReader(new Thing { ByteArrayProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.ByteArrayProperty));

        reader.Read();
        reader.Invoking(r => r.GetBytes(ordinal, 0, null, 0, 0))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetBytes_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetBytes(ordinal, 0, null, 0, 0))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Byte[]'.");
    }

    [Test]
    public void GetChars_NotNull_LimitedByMaxSize()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader.GetChars(ordinal, 0, target, 0, 2).Should().Be(2);

        target.Should().Equal('a', 'b');
    }

    [Test]
    public void GetChars_NotNull_LimitedByDataOffset()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader.GetChars(ordinal, 3, target, 1, 42).Should().Be(1);

        target.Should().Equal(' ', 'd');
    }

    [Test]
    public void GetChars_NotNull_NullBuffer()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader.GetChars(ordinal, -1, null, -1, -1).Should().Be(4);
    }

    [Test]
    public void GetChars_NotNull_NegativeDataOffset()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetChars(ordinal, -1, target, 0, 0))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dataOffset");

        target.Should().Equal(' ', ' ');
    }

    [Test]
    public void GetChars_NotNull_NegativeBufferOffset()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetChars(ordinal, 0, target, -1, 0))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("bufferOffset");

        target.Should().Equal(' ', ' ');
    }

    [Test]
    public void GetChars_NotNull_NegativeMaxLength()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetChars(ordinal, 0, target, 0, -1))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxLength");

        target.Should().Equal(' ', ' ');
    }

    [Test]
    public void GetChars_NotNull_DataOffsetBeyondData()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader.GetChars(ordinal, 4, target, 42, 42).Should().Be(0);

        target.Should().Equal(' ', ' ');
    }

    [Test]
    public void GetChars_NotNull_InsufficientBuffer()
    {
        var source = new char[] { 'a', 'b', 'c', 'd' };
        var target = new char[] { ' ', ' ' };

        using var reader  = MakeReader(new Thing { CharArrayProperty = source });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader
            .Invoking(r => r.GetChars(ordinal, 1, target, 0, 3))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("buffer");

        target.Should().Equal(' ', ' ');
    }

    [Test]
    public void GetChars_Null()
    {
        using var reader  = MakeReader(new Thing { CharArrayProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.CharArrayProperty));

        reader.Read();
        reader.Invoking(r => r.GetChars(ordinal, 0, null, 0, 0))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Field value is null.*");
    }

    [Test]
    public void GetChars_WrongType()
    {
        using var reader  = MakeReader(new Thing { Int32Property = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.Int32Property));

        reader.Read();
        reader.Invoking(r => r.GetChars(ordinal, 0, null, 0, 0))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.Char[]'.");
    }

    [Test]
    public void GetValues_NullArray()
    {
        using var reader = MakeReader(new Thing());

        reader.Read();
        reader
            .Invoking(r => r.GetValues(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetValues_LimitedByArrayLength()
    {
        using var reader = MakeReader(new Thing
        {
            StringProperty  = "a",
            BooleanProperty = true,
        });

        reader.Read();

        var array = new object[3];
        reader.GetValues(array).Should().Be(3);

        array[0].Should().Be("a");
        array[1].Should().Be(true);
        array[2].Should().Be(DBNull.Value);
    }

    [Test]
    public void GetValues_LimitedByFieldCount()
    {
        using var reader = MakeReader(new Thing
        {
            StringProperty  = "a",
            BooleanProperty = true,
        });

        reader.Read();

        var array = new object[20];
        reader.GetValues(array).Should().Be(14);

        array[ 0].Should().Be("a");
        array[ 1].Should().Be(true);
        array[ 2].Should().Be(DBNull.Value);
        array[13].Should().Be(DBNull.Value);
        array[14].Should().BeNull();
    }

    [Test]
    public void GetEnumerator()
    {
        using var reader = MakeReader(new Thing());

        reader.GetEnumerator().Should().BeOfType<DbEnumerator>();
    }

    [Test]
    public void GetSchemaTable()
    {
        using var reader = MakeReader();

        var schema = reader.GetSchemaTable();

        schema.Rows.Count                 .Should().Be(14);

        schema.Rows[0]["IsKey"]           .Should().Be(DBNull.Value);
        schema.Rows[0]["ColumnOrdinal"]   .Should().Be(DBNull.Value);
        schema.Rows[0]["ColumnName"]      .Should().Be(nameof(Thing.StringProperty));
        schema.Rows[0]["DataType"]        .Should().Be(typeof(string));
        schema.Rows[0]["ColumnSize"]      .Should().Be(DBNull.Value);
        schema.Rows[0]["NumericPrecision"].Should().Be(DBNull.Value);
        schema.Rows[0]["NumericScale"]    .Should().Be(DBNull.Value);

        schema.Rows[1]["IsKey"]           .Should().Be(DBNull.Value);
        schema.Rows[1]["ColumnOrdinal"]   .Should().Be(DBNull.Value);
        schema.Rows[1]["ColumnName"]      .Should().Be(nameof(Thing.BooleanProperty));
        schema.Rows[1]["DataType"]        .Should().Be(typeof(bool));
        schema.Rows[1]["ColumnSize"]      .Should().Be(DBNull.Value);
        schema.Rows[1]["NumericPrecision"].Should().Be(DBNull.Value);
        schema.Rows[1]["NumericScale"]    .Should().Be(DBNull.Value);
    }

    private static ObjectDataReader<Thing> MakeReader(params Thing[] things)
        => new(things, Thing.Map);

    internal class Thing
    {
        public string?   StringProperty    { get; set; }
        public bool?     BooleanProperty   { get; set; }
        public byte?     ByteProperty      { get; set; }
        public char?     CharProperty      { get; set; }
        public short?    Int16Property     { get; set; }
        public int?      Int32Property     { get; set; }
        public long?     Int64Property     { get; set; }
        public float?    SingleProperty    { get; set; }
        public double?   DoubleProperty    { get; set; }
        public decimal?  DecimalProperty   { get; set; }
        public Guid?     GuidProperty      { get; set; }
        public DateTime? DateTimeProperty  { get; set; }
        public byte[]?   ByteArrayProperty { get; set; }
        public char[]?   CharArrayProperty { get; set; } 

        public static ObjectDataMap<Thing> Map { get; } = new(m => m
            .Field(nameof(StringProperty),    "varchar",          x => x.StringProperty)
            .Field(nameof(BooleanProperty),   "bit",              x => x.BooleanProperty)
            .Field(nameof(ByteProperty),      "tinyint",          x => x.ByteProperty)
            .Field(nameof(CharProperty),      "char",             x => x.CharProperty)
            .Field(nameof(Int16Property),     "smallint",         x => x.Int16Property)
            .Field(nameof(Int32Property),     "int",              x => x.Int32Property)
            .Field(nameof(Int64Property),     "bigint",           x => x.Int64Property)
            .Field(nameof(SingleProperty),    "float",            x => x.SingleProperty)
            .Field(nameof(DoubleProperty),    "float",            x => x.DoubleProperty)
            .Field(nameof(DecimalProperty),   "decimal",          x => x.DecimalProperty)
            .Field(nameof(GuidProperty),      "uniqueidentifier", x => x.GuidProperty)
            .Field(nameof(DateTimeProperty),  "datetime2",        x => x.DateTimeProperty)
            .Field(nameof(ByteArrayProperty), "binary",           x => x.ByteArrayProperty)
            .Field(nameof(CharArrayProperty), "char",             x => x.CharArrayProperty)
        );
    }

    private class DisposalTestHarness : TestHarnessBase
    {
        public Mock<IEnumerable<Thing>> Source     { get; }
        public Mock<IEnumerator<Thing>> Enumerator { get; }
        public ObjectDataReader<Thing>  Reader     { get; }

        public DisposalTestHarness()
        {
            Source     = Mocks.Create<IEnumerable<Thing>>();
            Enumerator = Mocks.Create<IEnumerator<Thing>>();

            Source
                .Setup(s => s.GetEnumerator())
                .Returns(Enumerator.Object);

            Enumerator
                .Setup(e => e.MoveNext())
                .Returns(false);

            Enumerator
                .Setup(e => e.Dispose());

            Reader = new(Source.Object, Thing.Map);

            Source    .Reset();
            Enumerator.Reset();
        }
    }
}
