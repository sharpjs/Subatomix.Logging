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

using Field = ObjectDataMap<ObjectDataMapTests.Thing>.Field;

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
    public void Construct_Field_NullName()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field(null!, "any", x => 0)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_NullDbType()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field("any", null!, x => 0)))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_Field_NullGetter()
    {
        Invoking(() => new ObjectDataMap<Thing>(m => m.Field<int>("any", "any", null!)))
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
        AssertField<int    >(TestMap[0], "id",     "int"         );
        AssertField<string?>(TestMap[1], "name",   "varchar(100)");
        AssertField<int?   >(TestMap[2], "number", "int"         );
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
        TestMap.GetOrdinal("id"    ).Should().Be(0);
        TestMap.GetOrdinal("name"  ).Should().Be(1);
        TestMap.GetOrdinal("number").Should().Be(2);
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

        e.MoveNext().Should().BeTrue(); AssertField<int    >(e.Current, "id",     "int"         );
        e.MoveNext().Should().BeTrue(); AssertField<string?>(e.Current, "name",   "varchar(100)");
        e.MoveNext().Should().BeTrue(); AssertField<int?   >(e.Current, "number", "int"         );
    }

    [Test]
    public void GetEnumerator_Nongeneric()
    {
        var e = ((IEnumerable) TestMap).GetEnumerator();

        e.MoveNext().Should().BeTrue(); AssertField<int    >(e.Current, "id",     "int"         );
        e.MoveNext().Should().BeTrue(); AssertField<string?>(e.Current, "name",   "varchar(100)");
        e.MoveNext().Should().BeTrue(); AssertField<int?   >(e.Current, "number", "int"         );
    }

    [Test]
    public void GetValue()
    {
        TestMap[0].GetValue(new() { Id = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValue_NullObject()
    {
        TestMap[0].Invoking(m => m.GetValue(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetValueAs_Valid()
    {
        TestMap[0].GetValueAs<int>(new() { Id = 42 }).Should().Be(42);
    }

    [Test]
    public void GetValueAs_Invalid()
    {
        TestMap[0]
            .Invoking(m => m.GetValueAs<string>(new() { Id = 42 }))
            .Should()
            .Throw<InvalidCastException>();
    }

    [Test]
    public void GetValueAs_NullObject()
    {
        TestMap[0].Invoking(m => m.GetValueAs<int>(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TryGetValueAs_Valid()
    {
        TestMap[0].TryGetValueAs<int>(new() { Id = 42 }, out var value).Should().BeTrue();

        value.Should().Be(42);
    }

    [Test]
    public void TryGetValueAs_Invalid()
    {
        TestMap[0].TryGetValueAs<string>(new() { Id = 42 }, out var value).Should().BeFalse();

        value.Should().BeNull();
    }

    [Test]
    public void TryGetValueAs_NullObject()
    {
        TestMap[0].TryGetValueAs<string>(null!, out var value).Should().BeFalse();

        value.Should().BeNull();
    }

    private static void AssertField<T>(object obj, string name, string dbType)
    {
        obj.Should().Match<Field>(f
            => f.Name    == name
            && f.DbType  == dbType
            && f.NetType == typeof(T)
        );
    }

    private readonly ObjectDataMap<Thing> TestMap = new(m => m
        .Field("id",     "int",          x => x.Id)
        .Field("name",   "varchar(100)", x => x.Name)
        .Field("number", "int",          x => x.Number)
    );

    internal class Thing
    {
        public int     Id     { get; set; } // value type
        public string? Name   { get; set; } // reference type
        public int?    Number { get; set; } // Nullable<T>
    }
}
