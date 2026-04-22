/*
Pre-Deployment Script
*/
-- FLP-1645 (delete this script after 1645 deployed to prod)
IF OBJECT_ID('[Domain].[ShortCourseEpisode]', 'U') IS NOT NULL
BEGIN

    -- EmployerAccountId
    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'EmployerAccountId') IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [Domain].[ShortCourseEpisode] ALTER COLUMN [EmployerAccountId] BIGINT NULL');
    
        EXEC('UPDATE [Domain].[ShortCourseEpisode]
              SET [EmployerAccountId] = NULL
              WHERE [EmployerAccountId] IS NOT NULL');
    END

    -- FundingEmployerAccountId
    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'FundingEmployerAccountId') IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [Domain].[ShortCourseEpisode] ALTER COLUMN [FundingEmployerAccountId] BIGINT NULL');
    
        EXEC('UPDATE [Domain].[ShortCourseEpisode]
              SET [FundingEmployerAccountId] = NULL
              WHERE [FundingEmployerAccountId] IS NOT NULL');
    END

    -- LegalEntityName
    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'LegalEntityName') IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [Domain].[ShortCourseEpisode] ALTER COLUMN [LegalEntityName] NVARCHAR(255) NULL');
    
        EXEC('UPDATE [Domain].[ShortCourseEpisode]
              SET [LegalEntityName] = NULL
              WHERE [LegalEntityName] IS NOT NULL');
    END
END