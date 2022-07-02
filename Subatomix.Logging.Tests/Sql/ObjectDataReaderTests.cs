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
        using var reader = MakeReader();

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
        using var reader  = MakeReader(new Thing { IntProperty = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.IntProperty));

        reader.Read();
        reader.IsDBNull(ordinal).Should().BeFalse();
    }

    [Test]
    public void IsDBNull_ValueType_True()
    {

        using var reader  = MakeReader(new Thing { IntProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.IntProperty));

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
        using var reader  = MakeReader(new Thing { IntProperty = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.IntProperty));

        reader.Read();
        reader.GetValue(ordinal).Should().BeOfType<int>().And.Be(42);
    }

    [Test]
    public void GetValue_ValueType_Null()
    {
        using var reader  = MakeReader(new Thing { IntProperty = null });
              var ordinal = reader.GetOrdinal(nameof(Thing.IntProperty));

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
            .WithMessage("Data is null.*");
    }

    [Test]
    public void GetString_WrongType()
    {
        using var reader  = MakeReader(new Thing { IntProperty = 42 });
              var ordinal = reader.GetOrdinal(nameof(Thing.IntProperty));

        reader.Read();
        reader.Invoking(r => r.GetString(ordinal))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.Int32' to type 'System.String'.");
    }

    private static ObjectDataReader<Thing> MakeReader(params Thing[] things)
        => new(things, Thing.Map);

    internal class Thing
    {
        public string?   StringProperty    { get; set; }
        public bool?     BoolProperty      { get; set; }
        public byte?     ByteProperty      { get; set; }
        public char?     CharProperty      { get; set; }
        public short?    ShortProperty     { get; set; }
        public int?      IntProperty       { get; set; }
        public long?     LongProperty      { get; set; }
        public float?    FloatProperty     { get; set; }
        public double?   DoubleProperty    { get; set; }
        public decimal?  DecimalProperty   { get; set; }
        public Guid?     GuidProperty      { get; set; }
        public DateTime? DateTimeProperty  { get; set; }
        public byte[]?   ByteArrayProperty { get; set; }
        public char[]?   CharArrayProperty { get; set; } 

        public static ObjectDataMap<Thing> Map { get; } = new(m => m
            .Field(nameof(StringProperty),    "varchar",          x => x.StringProperty)
            .Field(nameof(BoolProperty),      "bit",              x => x.BoolProperty)
            .Field(nameof(ByteProperty),      "tinyint",          x => x.ByteProperty)
            .Field(nameof(CharProperty),      "char",             x => x.CharProperty)
            .Field(nameof(ShortProperty),     "smallint",         x => x.ShortProperty)
            .Field(nameof(IntProperty),       "int",              x => x.IntProperty)
            .Field(nameof(LongProperty),      "bigint",           x => x.LongProperty)
            .Field(nameof(FloatProperty),     "float",            x => x.FloatProperty)
            .Field(nameof(DoubleProperty),    "double",           x => x.DoubleProperty)
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
