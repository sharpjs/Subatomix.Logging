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

DROP ROLE      IF EXISTS Writer;
DROP PROCEDURE IF EXISTS log.Write;
DROP TYPE      IF EXISTS log.EntryRow;
DROP TABLE     IF EXISTS log.Entry;
DROP TABLE     IF EXISTS log.Level;
DROP TABLE     IF EXISTS log.Machine;
DROP TABLE     IF EXISTS log.Log;
DROP SCHEMA    IF EXISTS log;

-- ----------------------------------------------------------------------------

ALTER DATABASE CURRENT
COLLATE Latin1_General_100_CI_AI_SC_UTF8;

ALTER DATABASE CURRENT
SET RECOVERY SIMPLE;

ALTER DATABASE CURRENT
SET ALLOW_SNAPSHOT_ISOLATION ON;

ALTER DATABASE CURRENT
SET READ_COMMITTED_SNAPSHOT              ON
  , MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT ON
  , ANSI_NULL_DEFAULT                    ON
  , ANSI_NULLS                           ON
  , ANSI_PADDING                         ON
  , ANSI_WARNINGS                        ON
  , ARITHABORT                           ON
  , CONCAT_NULL_YIELDS_NULL              ON
  , QUOTED_IDENTIFIER                    ON
  , AUTO_UPDATE_STATISTICS               ON
  , AUTO_UPDATE_STATISTICS_ASYNC         ON
;

-- ----------------------------------------------------------------------------

GO
CREATE SCHEMA log AUTHORIZATION dbo;
GO

-- ----------------------------------------------------------------------------

CREATE TABLE log.Log
(
    Id          int IDENTITY    NOT NULL
  , Name        varchar(128)    NOT NULL -- Example: app.env0.web

  , CONSTRAINT Log_PK
        PRIMARY KEY (Id)
        WITH (DATA_COMPRESSION = PAGE, OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)
);

CREATE UNIQUE INDEX UX_Name
    ON log.Log (Name)
    WITH (DATA_COMPRESSION = PAGE, FILLFACTOR = 90)
;

-- ----------------------------------------------------------------------------

CREATE TABLE log.Machine
(
    Id          int IDENTITY    NOT NULL
  , Name        varchar(255)    NOT NULL -- Example: server1

  , CONSTRAINT Machine_PK
        PRIMARY KEY (Id)
        WITH (DATA_COMPRESSION = PAGE, OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)
);

CREATE UNIQUE INDEX UX_Name
    ON log.Machine (Name)
    WITH (DATA_COMPRESSION = PAGE, FILLFACTOR = 90)
;

-- ----------------------------------------------------------------------------

CREATE TABLE log.Level
(
    Level       tinyint         NOT NULL
  , Name        varchar(20)     NOT NULL

  , CONSTRAINT Level_PK
        PRIMARY KEY (Level)
        WITH (DATA_COMPRESSION = PAGE)
);

CREATE UNIQUE INDEX UX_Name
    ON log.Level (Name)
    WITH (DATA_COMPRESSION = PAGE)
;

INSERT log.Level
VALUES
    (0, 'Trace'      )
  , (1, 'Debug'      )
  , (2, 'Information')
  , (3, 'Warning'    )
  , (4, 'Error'      )
  , (5, 'Critical'   )
;

-- ----------------------------------------------------------------------------

CREATE TABLE log.Entry
(
    Date        datetime2(4)        NOT NULL CONSTRAINT Entry_DF_Date DEFAULT SYSUTCDATETIME()
  , Seq         bigint IDENTITY     NOT NULL -- Uniquifying sequence number
  , LogId       int                 NOT NULL -- Id of log stream
  , MachineId   int                 NOT NULL -- Id of sending machine
  , ProcessId   int                     NULL -- Id of process within OS
  , TraceId     varchar(32)             NULL -- Id of root logical operation
  , EventId     int                     NULL -- Id of event kind (arbitrary)
  , Level       tinyint             NOT NULL -- Severity level
  , Message     varchar(1024)       NOT NULL -- State formatted as string
  , Category    varchar(128)        NOT NULL -- Logger category name

  , CONSTRAINT Entry_PK
        PRIMARY KEY (Date, Seq)
        WITH (DATA_COMPRESSION = PAGE, OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)

  , CONSTRAINT Entry_FK_Log
        FOREIGN KEY (LogId) REFERENCES log.Log (Id)

  , CONSTRAINT Entry_FK_Machine
        FOREIGN KEY (MachineId) REFERENCES log.Machine (Id)

  , CONSTRAINT Entry_CK_Level
        CHECK (Level BETWEEN 0 AND 5)
);

CREATE INDEX IX_LogId
    ON log.Entry (LogId) -- IMPLICIT (Date, Seq)
    INCLUDE (Level)
    WITH (DATA_COMPRESSION = PAGE, OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)
    -- TODO: Benchmark to figure out what is best for this index
    -- TODO: Does OPTIMIZE_FOR_SEQUENTIAL_KEY really help for mid-index hotspots?
;

CREATE INDEX IX_MachineId
    ON log.Entry (MachineId) -- IMPLICIT (Date, Seq)
    INCLUDE (Level)
    WITH (DATA_COMPRESSION = PAGE, OPTIMIZE_FOR_SEQUENTIAL_KEY = ON)
    -- TODO: Benchmark to figure out what is best for this index
    -- TODO: Does OPTIMIZE_FOR_SEQUENTIAL_KEY really help for mid-index hotspots?
;

-- ----------------------------------------------------------------------------

CREATE TYPE log.EntryRow AS TABLE
(
    Date        datetime2(4)        NOT NULL
  , Seq         int                 NOT NULL
  , TraceId     varchar(32)             NULL
  , EventId     int                     NULL
  , Level       tinyint             NOT NULL
  , Message     varchar(1024)       NOT NULL
  , Category    varchar(128)        NOT NULL

  , PRIMARY KEY (Date, Seq)
);

-- ----------------------------------------------------------------------------

GO
CREATE PROCEDURE log.Write
    @LogName        varchar(128),
    @MachineName    varchar(255),
    @ProcessId      int,
    @EntryRows      log.EntryRow READONLY
AS BEGIN
    SET NOCOUNT ON;
    DECLARE
        @LogId      int,
        @MachineId  int,
        @Chances    tinyint = 3
    ;

    WHILE 0=0
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            SELECT @MachineId = Id FROM log.Machine WHERE Name = @MachineName;

            IF @MachineId IS NULL
            BEGIN
                INSERT log.Machine (Name) VALUES (@MachineName);
                SET @MachineId = SCOPE_IDENTITY();
            END;

            SELECT @LogId = Id FROM log.Log WHERE Name = @LogName;

            IF @LogId IS NULL
            BEGIN
                INSERT log.Log (Name) VALUES (@LogName);
                SET @LogId = SCOPE_IDENTITY();
            END;

            INSERT log.Entry
                (Date, LogId, MachineId, ProcessId, TraceId, EventId, Level, Message, Category)
            SELECT
                Date, @LogId, @MachineId, @ProcessId, TraceId, EventId, Level, Message, Category
            FROM
                @EntryRows
            ORDER BY
                Date, Seq
           ;

            COMMIT TRANSACTION;
            RETURN;
        END TRY
        BEGIN CATCH
            IF XACT_STATE() != 0 ROLLBACK;

            -- Check if out of chances
            SET @Chances -= 1;
            IF @Chances = 0 THROW;

            -- Check if error is ineligible for a quick retry
            -- > 1205: Chosen as deadlock victim
            -- > 3960: Snapshot update conflict
            IF ERROR_NUMBER() NOT IN (1205, 3960) THROW;

            -- Retry after 1ms
            WAITFOR DELAY '00:00:00.001';
        END CATCH;
    END;
END;
GO

-- ----------------------------------------------------------------------------

CREATE ROLE Writer AUTHORIZATION dbo;
GRANT REFERENCES, EXECUTE ON TYPE::log.EntryRow TO Writer;
GRANT REFERENCES, EXECUTE ON       log.Write    TO Writer;
