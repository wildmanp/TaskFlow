
/* 02-seed.sql
   Populează TaskFlowLab cu date de test.
   Se rulează imediat după 01-schema.sql.
*/

USE TaskFlowLab;
GO

SET NOCOUNT ON;

-- 1. Utilizatori
INSERT INTO dbo.Users (Email, DisplayName, IsActive)
VALUES
    (N'ana.popescu@taskflow.local', N'Ana Popescu', 1),
    (N'stefan.turcanu@taskflow.local', N'Ștefan Țurcanu', 1),
    (N'anna.petrova@taskflow.local', N'Анна Петрова', 1),
    (N'ion.rusu@taskflow.local', N'Ion Rusu', 1),
    (N'maria.ceban@taskflow.local', N'Maria Ceban', 1),
    (N'guest@taskflow.local', N'Guest User', 0);

-- 2. Proiecte
INSERT INTO dbo.Projects (Name, Description, OwnerId)
VALUES
    (N'Website', N'Public website of the company', 1),
    (N'Mobile App', N'iOS and Android client', 2),
    (N'Internal Tools', N'Scripts and admin tools', 3),
    (N'Archive 2025', NULL, 1);

-- 3. Etichete
INSERT INTO dbo.Tags (Name)
VALUES
    (N'bug'),
    (N'feature'),
    (N'docs'),
    (N'urgent'),
    (N'backend'),
    (N'frontend');

-- 4. Generarea celor 10.000 de sarcini
DECLARE @BaseDate DATETIME2(0) = '2026-01-01T09:00:00';

WITH Numbers AS
(
    SELECT TOP (10000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
)
INSERT INTO dbo.Tasks
(
    Title, Description, Priority, Status,
    DueDate, CompletedAt, ProjectId, AssigneeId, CreatedAt
)
SELECT
    CONCAT(
        CHOOSE(n % 4 + 1,
            N'Fix bug',
            N'Write docs',
            N'Review PR',
            N'Deploy build'
        ),
        N' #', n
    ),
    CASE
        WHEN n % 5 = 0 THEN NULL
        ELSE CONCAT(N'Auto-generated task number ', n)
    END,
    n % 4,
    n % 3,
    CASE
        WHEN n % 6 = 0 THEN NULL
        ELSE DATEADD(DAY, n % 365, @BaseDate)
    END,
    CASE
        WHEN n % 3 = 2 THEN DATEADD(DAY, n % 365, @BaseDate)
        ELSE NULL
    END,
    CASE
        WHEN n % 10 = 0 THEN NULL
        ELSE (n / 3) % 3 + 1
    END,
    CASE
        WHEN n % 7 = 0 THEN NULL
        WHEN n % 10 < 4 THEN 1
        WHEN n % 10 < 6 THEN 2
        WHEN n % 10 < 8 THEN 3
        WHEN n % 10 = 8 THEN 4
        ELSE 5
    END,
    DATEADD(MINUTE, n, @BaseDate)
FROM Numbers;

-- 5. Legături N:M dintre Tasks și Tags
INSERT INTO dbo.TaskTags (TaskId, TagId)
SELECT t.Id, t.Id % 6 + 1
FROM dbo.Tasks AS t

UNION ALL

SELECT t.Id, (t.Id + 2) % 6 + 1
FROM dbo.Tasks AS t
WHERE t.Id % 4 = 0;

-- 6. Comentarii: primul comentariu la fiecare 50 de sarcini
INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
SELECT
    t.Id,
    t.Id % 5 + 1,
    CONCAT(N'First comment for task ', t.Id)
FROM dbo.Tasks AS t
WHERE t.Id % 50 = 0;

-- Al doilea comentariu la fiecare 500 de sarcini
INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
SELECT
    t.Id,
    3,
    N'Follow-up: please check again'
FROM dbo.Tasks AS t
WHERE t.Id % 500 = 0;

-- 7. Verificarea numărului de înregistrări
SELECT
    CAST(x.TableName AS VARCHAR(15)) AS TableName,
    x.RowsCount
FROM
(
    SELECT 'Users' AS TableName, COUNT(*) AS RowsCount, 1 AS SortOrder
    FROM dbo.Users

    UNION ALL
    SELECT 'Projects', COUNT(*), 2 FROM dbo.Projects

    UNION ALL
    SELECT 'Tags', COUNT(*), 3 FROM dbo.Tags

    UNION ALL
    SELECT 'Tasks', COUNT(*), 4 FROM dbo.Tasks

    UNION ALL
    SELECT 'TaskTags', COUNT(*), 5 FROM dbo.TaskTags

    UNION ALL
    SELECT 'TaskComments', COUNT(*), 6 FROM dbo.TaskComments
) AS x
ORDER BY x.SortOrder;
GO
