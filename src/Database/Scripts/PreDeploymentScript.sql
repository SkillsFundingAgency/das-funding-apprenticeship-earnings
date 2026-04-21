/*
Pre-Deployment Script
*/


-- FLP-1645 (delete this script after 1645 deployed to prod)
IF OBJECT_ID('[Domain].[ShortCourseEpisode]', 'U') IS NOT NULL
BEGIN

    -- Make columns nullable first (if they exist and aren't already nullable)
    
    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'EmployerAccountId') IS NOT NULL
    BEGIN
        ALTER TABLE [Domain].[ShortCourseEpisode]
        ALTER COLUMN [EmployerAccountId] BIGINT NULL;
    END

    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'FundingEmployerAccountId') IS NOT NULL
    BEGIN
        ALTER TABLE [Domain].[ShortCourseEpisode]
        ALTER COLUMN [FundingEmployerAccountId] BIGINT NULL;
    END

    IF COL_LENGTH('[Domain].[ShortCourseEpisode]', 'LegalEntityName') IS NOT NULL
    BEGIN
        ALTER TABLE [Domain].[ShortCourseEpisode]
        ALTER COLUMN [LegalEntityName] NVARCHAR(255) NULL;
    END

    -- Clear data
    UPDATE [Domain].[ShortCourseEpisode]
    SET 
        EmployerAccountId = NULL,
        FundingEmployerAccountId = NULL,
        LegalEntityName = NULL
    WHERE
        EmployerAccountId IS NOT NULL
        OR FundingEmployerAccountId IS NOT NULL
        OR LegalEntityName IS NOT NULL;
END