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

namespace Subatomix.Logging.Sql;

[TestFixture]
public class ObjectDataMapTests
{
    [Test]
    public void Construct_NullBuilder()
    {
        Invoking(() => new ObjectDataMap<Thing>(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_General_NullName()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field(null!, "any", x => "any")))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_General_NullDbType()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field("any", null!, x => "any")))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_General_NullGetter()
    {
        var nullGetter = null as Func<Thing, string?>;

        Invoking(() => new ObjectDataMap<Thing>(m => m.Field("any", "any", nullGetter!)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_NullableValue_NullName()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field(null!, "any", x => (int?) 0)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_NullableValue_NullDbType()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field("any", null!, x => (int?) 0)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_NullableValue_NullGetter()
    {
        var nullGetter = null as Func<Thing, int?>;

        Invoking(() => new ObjectDataMap<Thing>(m => m.Field("any", "any", nullGetter!)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Count_Get()
    {
        TestMap.Count.Should().Be(3);
    }

    [Test]
    public void Item_Get()
    {
        AssertField<string>(TestMap[0], "name",   "varchar(100)");
        AssertField<int   >(TestMap[1], "number", "int"         );
        AssertField<int   >(TestMap[2], "amount", "int"         );
    }

    [Test]
    public void Item_Get_IndexTooLow()
    {
        TestMap
            .Invoking(m => m[-1])
            .Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void Item_Get_IndexTooHigh()
    {
        TestMap
            .Invoking(m => m[3])
            .Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void GetOrdinal()
    {
        TestMap.GetOrdinal("name"  ).Should().Be(0);
        TestMap.GetOrdinal("number").Should().Be(1);
        TestMap.GetOrdinal("amount").Should().Be(2);
    }

    [Test]
    public void GetOrdinal_NullName()
    {
        TestMap
            .Invoking(m => m.GetOrdinal(null!))
            .Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void GetOrdinal_NotFound()
    {
        TestMap
            .Invoking(m => m.GetOrdinal("not found"))
            .Should().Throw<IndexOutOfRangeException>();
    }

    [Test]
    public void GetEnumerator_Generic()
    {
        using var e = TestMap.GetEnumerator();

        e.MoveNext().Should().BeTrue(); AssertField<string>(e.Current, "name",   "varchar(100)");
        e.MoveNext().Should().BeTrue(); AssertField<int   >(e.Current, "number", "int"         );
        e.MoveNext().Should().BeTrue(); AssertField<int   >(e.Current, "amount", "int"         );
    }

    [Test]
    public void GetEnumerator_Nongeneric()
    {
        var e = ((IEnumerable) TestMap).GetEnumerator();

        e.MoveNext().Should().BeTrue(); AssertField<string>(e.Current, "name",   "varchar(100)");
        e.MoveNext().Should().BeTrue(); AssertField<int   >(e.Current, "number", "int"         );
        e.MoveNext().Should().BeTrue(); AssertField<int   >(e.Current, "amount", "int"         );
    }

    [Test]
    public void GetValue_ReferenceType_NotNull()
    {
        TestMap[0].GetValue(new() { Name = "a" }).Should().Be("a");
    }

    [Test]
    public void GetValue_ReferenceType_Null()
    {
        TestMap[0].GetValue(new() { Name = null }).Should().Be(DBNull.Value);
    }

    [Test]
    public void GetValue_ValueType_NotNullable()
    {
        TestMap[1].GetValue(new() { Number = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValue_ValueType_NotNull()
    {
        TestMap[2].GetValue(new() { Amount = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValue_ValueType_Null()
    {
        TestMap[2].GetValue(new() { Amount = null }).Should().Be(DBNull.Value);
    }

    [Test]
    public void GetValue_NullObject()
    {
        TestMap[0]
            .Invoking(m => m.GetValue(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetValueAs_ReferenceType_NotNull()
    {
        TestMap[0].GetValueAs<string>(new() { Name = "a" }).Should().Be("a");
    }

    [Test]
    public void GetValueAs_ReferenceType_Null()
    {
        TestMap[0]
            .Invoking(m => m.GetValueAs<string>(new() { Name = null }))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Data is null.*");
    }

    [Test]
    public void GetValueAs_ValueType_NonNullable()
    {
        TestMap[1].GetValueAs<int>(new() { Number = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValueAs_ValueType_NotNull()
    {
        TestMap[2].GetValueAs<int>(new() { Amount = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValueAs_ValueType_Null()
    {
        TestMap[2]
            .Invoking(m => m.GetValueAs<int>(new() { Amount = null }))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Data is null.*");
    }

    [Test]
    public void GetValueAs_WrongType()
    {
        TestMap[0]
            .Invoking(m => m.GetValueAs<int>(new()))
            .Should().Throw<InvalidCastException>()
            .WithMessage("Unable to cast object of type 'System.String' to type 'System.Int32'.");
    }

    [Test]
    public void GetValueAs_NullObject()
    {
        TestMap[0]
            .Invoking(m => m.GetValueAs<string>(null!))
            .Should().Throw<ArgumentNullException>();
    }

    private static void AssertField<T>(object obj, string name, string dbType)
    {
        obj.Should().Match<ObjectDataMap<Thing>.Field>(f
            => f.Name    == name
            && f.DbType  == dbType
            && f.NetType == typeof(T)
        );
    }

    internal class Thing
    {
        public string? Name   { get; set; } // reference type
        public int     Number { get; set; } // non-nullable value type
        public int?    Amount { get; set; } // nullable value type (Nullable<T>)
    }

    private ObjectDataMap<Thing> TestMap { get; } = new(m => m
        .Field("name",   "varchar(100)", x => x.Name)
        .Field("number", "int",          x => x.Number)
        .Field("amount", "int",          x => x.Amount)
    );
}
