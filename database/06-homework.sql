
USE TaskFlowLab;
GO

DROP TABLE IF EXISTS dbo.TimeEntryActivities;
DROP TABLE IF EXISTS dbo.TimeEntries;
DROP TABLE IF EXISTS dbo.Activities;
GO

CREATE TABLE dbo.Activities
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Activities PRIMARY KEY (Id),
    CONSTRAINT UQ_Activities_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.TimeEntries
(
    Id INT IDENTITY(1,1) NOT NULL,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    WorkDate DATE NOT NULL,
    Hours DECIMAL(4,2) NOT NULL,
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2(0) NOT NULL
        CONSTRAINT DF_TimeEntries_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TimeEntries PRIMARY KEY (Id),
    CONSTRAINT FK_TimeEntries_Tasks
        FOREIGN KEY (TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TimeEntries_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION,
    CONSTRAINT CK_TimeEntries_Hours
        CHECK (Hours > 0 AND Hours <= 24),
    CONSTRAINT UQ_TimeEntries_Task_User_Date
        UNIQUE (TaskId, UserId, WorkDate)
);
GO

CREATE TABLE dbo.TimeEntryActivities
(
    TimeEntryId INT NOT NULL,
    ActivityId INT NOT NULL,

    CONSTRAINT PK_TimeEntryActivities
        PRIMARY KEY (TimeEntryId, ActivityId),
    CONSTRAINT FK_TimeEntryActivities_TimeEntries
        FOREIGN KEY (TimeEntryId)
        REFERENCES dbo.TimeEntries(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TimeEntryActivities_Activities
        FOREIGN KEY (ActivityId)
        REFERENCES dbo.Activities(Id) ON DELETE NO ACTION
);
GO

INSERT INTO dbo.Activities (Name)
VALUES
    (N'coding'),
    (N'review'),
    (N'testing'),
    (N'documentation'),
    (N'meeting');
GO

;WITH Numbers AS
(
    SELECT TOP (5000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a
    CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.TimeEntries
    (TaskId, UserId, WorkDate, Hours, Note)
SELECT
    n,
    (n % 5) + 1,
    DATEADD(DAY, n % 31, CONVERT(DATE, '20261001', 112)),
    CAST(((n % 32) + 1) * 0.25 AS DECIMAL(4,2)),
    N'Inregistrare generata'
FROM Numbers;
GO

INSERT INTO dbo.TimeEntryActivities (TimeEntryId, ActivityId)
SELECT
    t.Id,
    a.ActivityId
FROM dbo.TimeEntries t
CROSS APPLY
(
    VALUES
        ((t.Id - 1) % 5 + 1),
        (t.Id % 5 + 1)
) a(ActivityId);
GO

SELECT COUNT(*) AS TotalTimeEntries FROM dbo.TimeEntries;
SELECT COUNT(*) AS TotalActivities FROM dbo.Activities;
SELECT COUNT(*) AS TotalTimeEntryActivities FROM dbo.TimeEntryActivities;
GO

SELECT
    p.Id AS ProjectId,
    p.Name AS ProjectName,
    SUM(COALESCE(te.Hours, 0)) AS TotalHours
FROM dbo.Projects p
LEFT JOIN dbo.Tasks t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries te
    ON te.TaskId = t.Id
    AND te.WorkDate >= '20261001'
    AND te.WorkDate < '20261101'
GROUP BY p.Id, p.Name
ORDER BY p.Id;
GO

SELECT TOP (3)
    u.Id AS UserId,
    u.DisplayName AS UserName,
    SUM(te.Hours) AS TotalHours
FROM dbo.Users u
INNER JOIN dbo.TimeEntries te ON te.UserId = u.Id
GROUP BY u.Id, u.DisplayName
ORDER BY TotalHours DESC;
GO

SELECT
    t.Id,
    t.Title
FROM dbo.Tasks t
WHERE t.Status = 1
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.TimeEntries te
    WHERE te.TaskId = t.Id
);
GO

SELECT
    u.Id,
    u.DisplayName,
    AVG(d.DailyHours) AS AverageHoursPerDay
FROM dbo.Users u
INNER JOIN
(
    SELECT
        UserId,
        WorkDate,
        SUM(Hours) AS DailyHours
    FROM dbo.TimeEntries
    GROUP BY UserId, WorkDate
) d ON d.UserId = u.Id
GROUP BY u.Id, u.DisplayName
HAVING AVG(d.DailyHours) > 6;
GO

;WITH WeeklyHours AS
(
    SELECT
        DATEADD(
            DAY,
            -((DATEPART(WEEKDAY, WorkDate) + @@DATEFIRST - 2) % 7),
            WorkDate
        ) AS WeekStart,
        SUM(Hours) AS TotalHours
    FROM dbo.TimeEntries
    WHERE WorkDate >= '20261001'
      AND WorkDate < '20261101'
    GROUP BY DATEADD(
        DAY,
        -((DATEPART(WEEKDAY, WorkDate) + @@DATEFIRST - 2) % 7),
        WorkDate
    )
)
SELECT
    WeekStart,
    TotalHours
FROM WeeklyHours
ORDER BY WeekStart;
GO

;WITH ProjectStats AS
(
    SELECT
        p.Id,
        p.Name,
        COUNT(t.Id) AS TotalTasks,
        SUM(CASE WHEN t.Status = 2 THEN 1 ELSE 0 END) AS CompletedTasks
    FROM dbo.Projects p
    LEFT JOIN dbo.Tasks t ON t.ProjectId = p.Id
    GROUP BY p.Id, p.Name
)
SELECT
    Id,
    Name,
    TotalTasks,
    CompletedTasks,
    CAST(
        100.0 * CompletedTasks / NULLIF(TotalTasks, 0)
        AS DECIMAL(5,2)
    ) AS CompletedPercentage
FROM ProjectStats
WHERE TotalTasks > 0
AND 100.0 * CompletedTasks / NULLIF(TotalTasks, 0) < 40;
GO

SELECT TOP (5)
    t.Id,
    t.Title,
    COUNT(c.Id) AS CommentCount,
    MAX(c.CreatedAt) AS LastCommentDate
FROM dbo.Tasks t
LEFT JOIN dbo.TaskComments c ON c.TaskId = t.Id
GROUP BY t.Id, t.Title
ORDER BY CommentCount DESC, LastCommentDate DESC;
GO

SELECT
    p.Id AS ProjectId,
    p.Name AS ProjectName,
    SUM(COALESCE(te.Hours, 0)) AS TotalHours
FROM dbo.Projects p
LEFT JOIN dbo.Tasks t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries te
    ON te.TaskId = t.Id
    AND te.WorkDate >= '20261001'
    AND te.WorkDate < '20261101'
GROUP BY p.Id, p.Name
ORDER BY p.Id
OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY;
GO

CREATE OR ALTER VIEW dbo.vw_UserWorkload
AS
WITH TaskStats AS
(
    SELECT
        u.Id AS UserId,
        COUNT(CASE WHEN t.Status <> 2 THEN 1 END) AS OpenTasks,
        COUNT(CASE
            WHEN t.Status <> 2
             AND t.DueDate < '20261031'
            THEN 1
        END) AS OverdueTasks
    FROM dbo.Users u
    LEFT JOIN dbo.Tasks t ON t.AssigneeId = u.Id
    GROUP BY u.Id
),
TimeStats AS
(
    SELECT
        UserId,
        SUM(Hours) AS TotalHours
    FROM dbo.TimeEntries
    WHERE WorkDate >= '20261002'
      AND WorkDate < '20261101'
    GROUP BY UserId
)
SELECT
    u.Id AS UserId,
    u.DisplayName,
    COALESCE(ts.OpenTasks, 0) AS OpenTasks,
    COALESCE(ts.OverdueTasks, 0) AS OverdueTasks,
    COALESCE(ws.TotalHours, 0) AS TotalHours
FROM dbo.Users u
LEFT JOIN TaskStats ts ON ts.UserId = u.Id
LEFT JOIN TimeStats ws ON ws.UserId = u.Id;
GO

SELECT *
FROM dbo.vw_UserWorkload
ORDER BY UserId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LogTime
    @TaskId INT,
    @UserId INT,
    @WorkDate DATE,
    @Hours DECIMAL(4,2),
    @Note NVARCHAR(500) = NULL,
    @NewTimeEntryId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Hours IS NULL OR @Hours <= 0 OR @Hours > 24
        THROW 51001, 'Hours must be greater than 0 and not more than 24.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE Id = @TaskId)
        THROW 51002, 'Task does not exist.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
        THROW 51003, 'User does not exist.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Tasks WHERE Id = @TaskId AND Status = 2)
        THROW 51004, 'Time cannot be logged for a completed task.', 1;

    DECLARE @Inserted TABLE (Id INT);

    INSERT INTO dbo.TimeEntries
        (TaskId, UserId, WorkDate, Hours, Note)
    OUTPUT inserted.Id INTO @Inserted(Id)
    VALUES
        (@TaskId, @UserId, @WorkDate, @Hours, @Note);

    SELECT @NewTimeEntryId = Id
    FROM @Inserted;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_MoveProjectTasks
    @FromProjectId INT,
    @ToProjectId INT,
    @AuthorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @FromProjectId = @ToProjectId
            THROW 51010, 'Source and destination projects must be different.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Id = @FromProjectId)
            THROW 51011, 'Source project does not exist.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Id = @ToProjectId)
            THROW 51012, 'Destination project does not exist.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @AuthorId)
            THROW 51013, 'Author does not exist.', 1;

        DECLARE @Moved TABLE
        (
            TaskId INT,
            OldProjectId INT
        );

        UPDATE dbo.Tasks
        SET ProjectId = @ToProjectId
        OUTPUT inserted.Id, deleted.ProjectId
            INTO @Moved(TaskId, OldProjectId)
        WHERE ProjectId = @FromProjectId
          AND Status <> 2;

        INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
        SELECT
            m.TaskId,
            @AuthorId,
            CONCAT(N'Moved from project ', p.Name)
        FROM @Moved m
        INNER JOIN dbo.Projects p ON p.Id = m.OldProjectId;

        DECLARE @MovedCount INT;
        SELECT @MovedCount = COUNT(*) FROM @Moved;

        COMMIT TRANSACTION;

        SELECT @MovedCount AS MovedTasks;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

SET STATISTICS IO ON;

SELECT
    p.Id AS ProjectId,
    p.Name AS ProjectName,
    SUM(COALESCE(te.Hours, 0)) AS TotalHours
FROM dbo.Projects p
LEFT JOIN dbo.Tasks t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries te
    ON te.TaskId = t.Id
    AND te.WorkDate >= '20261001'
    AND te.WorkDate < '20261101'
GROUP BY p.Id, p.Name
ORDER BY p.Id;

SELECT
    t.Id,
    t.Title
FROM dbo.Tasks t
WHERE t.Status = 1
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.TimeEntries te
    WHERE te.TaskId = t.Id
);

SET STATISTICS IO OFF;
GO

CREATE INDEX IX_TimeEntries_TaskId_WorkDate
ON dbo.TimeEntries (TaskId, WorkDate)
INCLUDE (Hours);
GO

CREATE INDEX IX_TimeEntries_UserId_WorkDate
ON dbo.TimeEntries (UserId, WorkDate);
GO

CREATE INDEX IX_TimeEntryActivities_ActivityId
ON dbo.TimeEntryActivities (ActivityId);
GO

SET STATISTICS IO ON;

SELECT
    p.Id AS ProjectId,
    p.Name AS ProjectName,
    SUM(COALESCE(te.Hours, 0)) AS TotalHours
FROM dbo.Projects p
LEFT JOIN dbo.Tasks t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries te
    ON te.TaskId = t.Id
    AND te.WorkDate >= '20261001'
    AND te.WorkDate < '20261101'
GROUP BY p.Id, p.Name
ORDER BY p.Id;

SELECT
    t.Id,
    t.Title
FROM dbo.Tasks t
WHERE t.Status = 1
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.TimeEntries te
    WHERE te.TaskId = t.Id
);

SET STATISTICS IO OFF;
GO

GRANT EXECUTE ON OBJECT::dbo.usp_LogTime TO TaskFlowApp;
GRANT EXECUTE ON OBJECT::dbo.usp_MoveProjectTasks TO TaskFlowApp;
GRANT SELECT ON OBJECT::dbo.vw_UserWorkload TO TaskFlowApp;
GO

BEGIN TRANSACTION;

EXEC dbo.usp_MoveProjectTasks
    @FromProjectId = 1,
    @ToProjectId = 2,
    @AuthorId = 1;

ROLLBACK TRANSACTION;
GO

DECLARE @TasksBefore INT;
DECLARE @TasksAfter INT;

SELECT @TasksBefore = COUNT(*)
FROM dbo.Tasks
WHERE ProjectId = 1;

BEGIN TRY
    EXEC dbo.usp_MoveProjectTasks
        @FromProjectId = 1,
        @ToProjectId = 2,
        @AuthorId = 999;
END TRY
BEGIN CATCH
    SELECT
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

SELECT @TasksAfter = COUNT(*)
FROM dbo.Tasks
WHERE ProjectId = 1;

SELECT
    @TasksBefore AS TasksBefore,
    @TasksAfter AS TasksAfter;
GO

DECLARE @TestTaskId INT;
DECLARE @LogResultId INT;

SELECT TOP (1)
    @TestTaskId = Id
FROM dbo.Tasks
WHERE Status <> 2
ORDER BY Id;

EXECUTE AS USER = 'TaskFlowApp';

BEGIN TRY
    SELECT TOP (1) *
    FROM dbo.TimeEntries;
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS DirectSelectError;
END CATCH;

EXEC dbo.usp_LogTime
    @TaskId = @TestTaskId,
    @UserId = 6,
    @WorkDate = '20261031',
    @Hours = 1.25,
    @Note = N'Work recorded',
    @NewTimeEntryId = @LogResultId OUTPUT;

SELECT @LogResultId AS NewTimeEntryId;

REVERT;
GO
