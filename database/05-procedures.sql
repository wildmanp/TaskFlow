
/* 05-procedures.sql
   Proceduri stocate, tranzactii, parametrizare si drepturi minime.
*/

USE TaskFlowLab;
GO

-- 1. Crearea unei sarcini cu parametru OUTPUT
CREATE OR ALTER PROCEDURE dbo.usp_CreateTask
    @Title NVARCHAR(200),
    @ProjectId INT = NULL,
    @AssigneeId INT = NULL,
    @Priority TINYINT = 1,
    @DueDate DATETIME2(0) = NULL,
    @NewTaskId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Title IS NULL OR LEN(@Title) = 0
        THROW 50001, N'Title is required.', 1;

    IF @ProjectId IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM dbo.Projects WHERE Id = @ProjectId
       )
        THROW 50002, N'Project not found.', 1;

    INSERT INTO dbo.Tasks
        (Title, ProjectId, AssigneeId, Priority, DueDate)
    VALUES
        (@Title, @ProjectId, @AssigneeId, @Priority, @DueDate);

    SET @NewTaskId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

-- 2. Finalizarea sarcinii si adaugarea comentariului atomic
CREATE OR ALTER PROCEDURE dbo.usp_CompleteTask
    @TaskId INT,
    @AuthorId INT,
    @Comment NVARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Tasks
        SET Status = 2,
            CompletedAt = SYSUTCDATETIME()
        WHERE Id = @TaskId AND Status <> 2;

        IF @@ROWCOUNT = 0
            THROW 50003, N'Task not found or already completed.', 1;

        INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
        VALUES (@TaskId, @AuthorId, @Comment);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO

-- 3. Cautare parametrizata cu paginare
CREATE OR ALTER PROCEDURE dbo.usp_SearchTasks
    @Search NVARCHAR(100),
    @Status TINYINT = NULL,
    @Page INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page < 1 SET @Page = 1;
    IF @PageSize NOT BETWEEN 1 AND 100 SET @PageSize = 20;

    SELECT t.Id, t.Title, t.Status, t.DueDate
    FROM dbo.Tasks AS t
    WHERE t.Title LIKE @Search + N'%'
      AND (@Status IS NULL OR t.Status = @Status)
    ORDER BY t.Id
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- 4. Testarea procedurii de creare
SET NOCOUNT ON;

PRINT N'--- 1. usp_CreateTask';

DECLARE @Id INT;

EXEC dbo.usp_CreateTask
    @Title = N'Prepare release notes',
    @ProjectId = 1,
    @AssigneeId = 2,
    @Priority = 2,
    @NewTaskId = @Id OUTPUT;

SELECT
    CASE WHEN @Id > 10000 THEN 'yes' ELSE 'no' END AS IdAssigned,
    CAST(t.Title AS NVARCHAR(30)) AS Title,
    t.ProjectId,
    t.AssigneeId,
    t.Priority,
    t.Status
FROM dbo.Tasks AS t
WHERE t.Id = @Id;

-- Test: proiect inexistent
PRINT N'--- 1b. Proiect inexistent';

BEGIN TRY
    EXEC dbo.usp_CreateTask
        @Title = N'Broken task',
        @ProjectId = 999,
        @NewTaskId = @Id OUTPUT;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'Eroarea ', ERROR_NUMBER(), N': ', ERROR_MESSAGE());
END CATCH;

-- 5. Testarea finalizarii cu succes
PRINT N'--- 2. usp_CompleteTask: succes';

EXEC dbo.usp_CompleteTask
    @TaskId = 1,
    @AuthorId = 1,
    @Comment = N'Done, merged to main';

SELECT
    t.Id,
    t.Status,
    CASE WHEN t.CompletedAt IS NULL THEN 'no' ELSE 'yes' END AS HasCompletedAt,
    (SELECT COUNT(*) FROM dbo.TaskComments AS c
     WHERE c.TaskId = t.Id) AS Comments
FROM dbo.Tasks AS t
WHERE t.Id = 1;

-- 6. Testarea rollback-ului
PRINT N'--- 2b. Autor inexistent: rollback';

BEGIN TRY
    EXEC dbo.usp_CompleteTask
        @TaskId = 4,
        @AuthorId = 999,
        @Comment = N'This will fail';
END TRY
BEGIN CATCH
    PRINT CONCAT(N'Eroarea ', ERROR_NUMBER(), N': rollback efectuat');
END CATCH;

SELECT
    t.Id,
    t.Status,
    CASE WHEN t.CompletedAt IS NULL THEN 'no' ELSE 'yes' END AS HasCompletedAt,
    (SELECT COUNT(*) FROM dbo.TaskComments AS c
     WHERE c.TaskId = t.Id) AS Comments
FROM dbo.Tasks AS t
WHERE t.Id = 4;

-- 7. Testarea cautarii
PRINT N'--- 3. usp_SearchTasks';

EXEC dbo.usp_SearchTasks
    @Search = N'Deploy build #99',
    @Status = 0,
    @Page = 1,
    @PageSize = 5;

-- Test de injectie SQL
PRINT N'--- 3b. Test parametrizare';

EXEC dbo.usp_SearchTasks
    @Search = N'x''; DROP TABLE dbo.Tasks; --';

SELECT COUNT(*) AS TasksStillHere FROM dbo.Tasks;
GO

-- 8. Utilizator cu drepturi minime
IF DATABASE_PRINCIPAL_ID(N'TaskFlowApp') IS NULL
    CREATE USER TaskFlowApp WITHOUT LOGIN;
GO

GRANT EXECUTE ON OBJECT::dbo.usp_CreateTask TO TaskFlowApp;
GRANT EXECUTE ON OBJECT::dbo.usp_CompleteTask TO TaskFlowApp;
GRANT EXECUTE ON OBJECT::dbo.usp_SearchTasks TO TaskFlowApp;
GRANT SELECT ON OBJECT::dbo.vw_TaskDetails TO TaskFlowApp;
GO

-- 9. Verificarea permisiunilor
SET NOCOUNT ON;

PRINT N'--- 4. Verificarea drepturilor TaskFlowApp';

EXECUTE AS USER = N'TaskFlowApp';

BEGIN TRY
    SELECT TOP (1) Id FROM dbo.Tasks;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'SELECT direct din tabel: eroarea ', ERROR_NUMBER());
END CATCH;

BEGIN TRY
    DELETE FROM dbo.Tasks WHERE Id = 2;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'DELETE din tabel: eroarea ', ERROR_NUMBER());
END CATCH;

SELECT COUNT(*) AS VisibleViaView
FROM dbo.vw_TaskDetails;

EXEC dbo.usp_SearchTasks
    @Search = N'Fix bug #1000',
    @PageSize = 1;

REVERT;

SELECT CAST(USER_NAME() AS NVARCHAR(20)) AS CurrentUserAfterRevert;
GO
