
/* 04-indexes.sql
   Indecsi pentru baza TaskFlowLab.
*/

USE TaskFlowLab;
GO

SET NOCOUNT ON;

-- 0. Stergerea indecsilor existenti pentru executare repetata
DROP INDEX IF EXISTS IX_Projects_OwnerId ON dbo.Projects;
DROP INDEX IF EXISTS IX_Tasks_ProjectId ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_Title ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_AssigneeId ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_AssigneeId_Status_DueDate ON dbo.Tasks;
DROP INDEX IF EXISTS IX_TaskTags_TagId ON dbo.TaskTags;
DROP INDEX IF EXISTS IX_TaskComments_TaskId ON dbo.TaskComments;
DROP INDEX IF EXISTS IX_TaskComments_AuthorId ON dbo.TaskComments;
GO

-- 1. Indecsi pentru cheile straine
CREATE NONCLUSTERED INDEX IX_Projects_OwnerId
ON dbo.Projects (OwnerId);

CREATE NONCLUSTERED INDEX IX_Tasks_ProjectId
ON dbo.Tasks (ProjectId);

CREATE NONCLUSTERED INDEX IX_TaskTags_TagId
ON dbo.TaskTags (TagId);

CREATE NONCLUSTERED INDEX IX_TaskComments_TaskId
ON dbo.TaskComments (TaskId);

CREATE NONCLUSTERED INDEX IX_TaskComments_AuthorId
ON dbo.TaskComments (AuthorId);
GO

SET STATISTICS IO ON;
GO

-- 2. Cautare dupa titlu fara index
PRINT N'--- 2. Title fara index (Scan)';

SELECT Id, Title, Status
FROM dbo.Tasks
WHERE Title = N'Review PR #5002';
GO

-- 3. Crearea indexului pentru Title
CREATE NONCLUSTERED INDEX IX_Tasks_Title
ON dbo.Tasks (Title);
GO

PRINT N'--- 3. Title cu index (Seek + posibil Key Lookup)';

SELECT Id, Title, Status
FROM dbo.Tasks
WHERE Title = N'Review PR #5002';
GO

-- 4. Conditie nesargabila: LIKE cu % la inceput
PRINT N'--- 4. LIKE cu % la inceput (Scan)';

SELECT Id, Title
FROM dbo.Tasks
WHERE Title LIKE N'%5002';
GO

-- 5. Conditie sargabila: LIKE cu prefix
PRINT N'--- 5. LIKE cu prefix (posibil Seek)';

SELECT Id, Title
FROM dbo.Tasks
WHERE Title LIKE N'Review PR%';
GO

-- 6. Index de acoperire
CREATE NONCLUSTERED INDEX IX_Tasks_AssigneeId_Status_DueDate
ON dbo.Tasks (AssigneeId, Status, DueDate)
INCLUDE (Title, Priority);
GO

PRINT N'--- 6. Interogare cu index de acoperire';

SELECT
    Title,
    Priority,
    DueDate
FROM dbo.Tasks
WHERE AssigneeId = 3
  AND Status = 1
ORDER BY DueDate;
GO

SET STATISTICS IO OFF;
GO
