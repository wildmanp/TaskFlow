
/* 03-queries.sql
   Interogari pe TaskFlowLab.
   Data de referinta: 2026-10-15 12:00.
*/

USE TaskFlowLab;
GO

SET NOCOUNT ON;

-- Q1. TOP: cele mai noi 5 sarcini ale utilizatorului 3
PRINT N'--- Q1. TOP';

SELECT TOP (5)
    t.Id,
    CAST(t.Title AS NVARCHAR(30)) AS Title,
    t.Status,
    t.CreatedAt
FROM dbo.Tasks AS t
WHERE t.AssigneeId = 3
ORDER BY t.CreatedAt DESC;


-- Q2. OFFSET/FETCH: pagina a treia
PRINT N'--- Q2. OFFSET/FETCH';

SELECT
    t.Id,
    CAST(t.Title AS NVARCHAR(30)) AS Title
FROM dbo.Tasks AS t
JOIN dbo.Projects AS p ON p.Id = t.ProjectId
WHERE p.Name = N'Website'
ORDER BY t.Id
OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY;


-- Q3. INNER JOIN: sarcina, proiectul si executantul
PRINT N'--- Q3. INNER JOIN';

SELECT TOP (5)
    t.Id,
    CAST(t.Title AS NVARCHAR(30)) AS Title,
    CAST(p.Name AS NVARCHAR(20)) AS Project,
    CAST(u.DisplayName AS NVARCHAR(20)) AS Assignee
FROM dbo.Tasks AS t
INNER JOIN dbo.Projects AS p ON p.Id = t.ProjectId
INNER JOIN dbo.Users AS u ON u.Id = t.AssigneeId
ORDER BY t.Id;


-- Q4. LEFT JOIN: toate proiectele si numarul de sarcini
PRINT N'--- Q4. LEFT JOIN';

SELECT
    CAST(p.Name AS NVARCHAR(20)) AS Project,
    COUNT(t.Id) AS TaskCount
FROM dbo.Projects AS p
LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
GROUP BY p.Name
ORDER BY p.Name;


-- Q5. Anti-join: utilizatori fara sarcini
PRINT N'--- Q5. ANTI-JOIN';

SELECT
    u.Id,
    CAST(u.DisplayName AS NVARCHAR(20)) AS DisplayName
FROM dbo.Users AS u
LEFT JOIN dbo.Tasks AS t ON t.AssigneeId = u.Id
WHERE t.Id IS NULL;


-- Q6. GROUP BY: numarul de sarcini pe statusuri
PRINT N'--- Q6. GROUP BY';

SELECT
    CASE t.Status
        WHEN 0 THEN 'Todo'
        WHEN 1 THEN 'InProgress'
        ELSE 'Done'
    END AS StatusName,
    COUNT(*) AS TaskCount
FROM dbo.Tasks AS t
GROUP BY t.Status
ORDER BY t.Status;


-- Q7. GROUP BY + HAVING
PRINT N'--- Q7. HAVING';

SELECT
    CAST(u.DisplayName AS NVARCHAR(20)) AS Assignee,
    COUNT(*) AS OpenTasks
FROM dbo.Tasks AS t
JOIN dbo.Users AS u ON u.Id = t.AssigneeId
WHERE t.Status <> 2
GROUP BY u.DisplayName
HAVING COUNT(*) > 1000
ORDER BY OpenTasks DESC, Assignee;


-- Q8. Subinterogare cu IN
PRINT N'--- Q8. SUBQUERY IN';

SELECT
    t.Id,
    CAST(t.Title AS NVARCHAR(30)) AS Title
FROM dbo.Tasks AS t
WHERE t.Id IN
(
    SELECT c.TaskId
    FROM dbo.TaskComments AS c
    GROUP BY c.TaskId
    HAVING COUNT(*) > 1
)
ORDER BY t.Id
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;


-- Q9. EXISTS: proiecte cu sarcini critice fara executant
PRINT N'--- Q9. EXISTS';

SELECT
    CAST(p.Name AS NVARCHAR(20)) AS Project
FROM dbo.Projects AS p
WHERE EXISTS
(
    SELECT 1
    FROM dbo.Tasks AS t
    WHERE t.ProjectId = p.Id
      AND t.Priority = 3
      AND t.AssigneeId IS NULL
)
ORDER BY p.Name;


-- Q10. CTE: sarcini nefinalizate cu termen depasit
PRINT N'--- Q10. CTE';

DECLARE @Now DATETIME2(0) = '2026-10-15T12:00:00';

;WITH Overdue AS
(
    SELECT
        t.ProjectId,
        t.Id
    FROM dbo.Tasks AS t
    WHERE t.Status <> 2
      AND t.DueDate < @Now
)
SELECT
    CAST(ISNULL(p.Name, N'(no project)') AS NVARCHAR(20)) AS Project,
    COUNT(*) AS OverdueCount
FROM Overdue AS o
LEFT JOIN dbo.Projects AS p ON p.Id = o.ProjectId
GROUP BY p.Name
ORDER BY OverdueCount DESC;


-- Q11. N:M: etichetele primelor cinci sarcini
PRINT N'--- Q11. STRING_AGG';

SELECT
    t.Id,
    CAST(
        STRING_AGG(g.Name, N', ')
        WITHIN GROUP (ORDER BY g.Name)
        AS NVARCHAR(30)
    ) AS Tags
FROM dbo.Tasks AS t
JOIN dbo.TaskTags AS tt ON tt.TaskId = t.Id
JOIN dbo.Tags AS g ON g.Id = tt.TagId
WHERE t.Id <= 5
GROUP BY t.Id
ORDER BY t.Id;
GO


-- Q12. VIEW: creare vedere
CREATE OR ALTER VIEW dbo.vw_TaskDetails
AS
SELECT
    t.Id,
    t.Title,
    t.Priority,
    t.Status,
    t.DueDate,
    p.Name AS ProjectName,
    u.DisplayName AS AssigneeName,
    (
        SELECT COUNT(*)
        FROM dbo.TaskComments AS c
        WHERE c.TaskId = t.Id
    ) AS CommentCount
FROM dbo.Tasks AS t
LEFT JOIN dbo.Projects AS p ON p.Id = t.ProjectId
LEFT JOIN dbo.Users AS u ON u.Id = t.AssigneeId;
GO


-- Testarea vederii
SET NOCOUNT ON;

PRINT N'--- Q12. VIEW';

SELECT
    Id,
    CAST(Title AS NVARCHAR(30)) AS Title,
    CAST(ProjectName AS NVARCHAR(20)) AS ProjectName,
    CAST(AssigneeName AS NVARCHAR(20)) AS AssigneeName,
    CommentCount
FROM dbo.vw_TaskDetails
WHERE Id IN (499, 500, 1000);


-- Q13. DML: INSERT, UPDATE, DELETE si OUTPUT
PRINT N'--- Q13. DML';

DECLARE @NewId INT;
DECLARE @Inserted TABLE (Id INT);

INSERT INTO dbo.Tasks
    (Title, Priority, ProjectId, AssigneeId)
OUTPUT inserted.Id INTO @Inserted (Id)
VALUES
    (N'Prepare sprint demo', 2, 1, 2);

SELECT @NewId = Id FROM @Inserted;

UPDATE dbo.Tasks
SET Status = 1
WHERE Id = @NewId;

SELECT
    CAST(Title AS NVARCHAR(30)) AS Title,
    Priority,
    Status,
    CompletedAt
FROM dbo.Tasks
WHERE Id = @NewId;

DELETE FROM dbo.Tasks
WHERE Id = @NewId;

SELECT COUNT(*) AS TasksAfterDelete
FROM dbo.Tasks;
GO
