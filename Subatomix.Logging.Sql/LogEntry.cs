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

internal class LogEntry
{
    public DateTime Date     { get; set; } // ╮ Primary
    public int      Ordinal  { get; set; } // ╯ Key

    public string?  TraceId  { get; set; }
    public int?     EventId  { get; set; }
    public LogLevel Level    { get; set; }
    public string   Message  { get; set; } = ""; // TODO: Require in constructor
    public string   Category { get; set; } = ""; // TODO: Require in constructor

    internal static readonly ObjectDataMap<LogEntry> Map = new(m => m
        //      NAME        TYPE             VALUE           CONVERSION
        .Field( "Date",     "datetime2(4)",  e => e.Date                     )
        .Field( "Seq",      "int",           e => e.Ordinal                  )
        .Field( "TraceId",  "varchar(32)",   e => e.TraceId ?.Truncate(  32) )
        .Field( "EventId",  "int",           e => e.EventId                  )
        .Field( "Level",    "tinyint",       e => e.Level    .ToByte()       )
        .Field( "Message",  "varchar(1024)", e => e.Message  .Truncate(1024) )
        .Field( "Category", "varchar(128)",  e => e.Category .Truncate( 128) )
    );
}
