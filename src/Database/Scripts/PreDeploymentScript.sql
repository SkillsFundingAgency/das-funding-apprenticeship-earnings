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

--FLP-1628 (delete this script after 1628 deployed to prod)
IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'AdditionalPayment' AND s.name = 'Domain')
    DROP TABLE [Domain].[AdditionalPayment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Instalment' AND s.name = 'Domain')
    DROP TABLE [Domain].[Instalment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'MathsAndEnglishInstalment' AND s.name = 'Domain')
    DROP TABLE [Domain].[MathsAndEnglishInstalment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'MathsAndEnglishPeriodInLearning' AND s.name = 'Domain')
    DROP TABLE [Domain].[MathsAndEnglishPeriodInLearning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'MathsAndEnglish' AND s.name = 'Domain')
    DROP TABLE [Domain].[MathsAndEnglish];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'EarningsProfileHistory' AND s.name = 'History')
    DROP TABLE [History].[EarningsProfileHistory];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'EarningsProfile' AND s.name = 'Domain')
    DROP TABLE [Domain].[EarningsProfile];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'EpisodePrice' AND s.name = 'Domain')
    DROP TABLE [Domain].[EpisodePrice];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'EpisodePeriodInLearning' AND s.name = 'Domain')
    DROP TABLE [Domain].[EpisodePeriodInLearning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Episode' AND s.name = 'Domain')
    DROP TABLE [Domain].[Episode];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Earning' AND s.name = 'Domain')
    DROP TABLE [Domain].[Earning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Learning' AND s.name = 'Domain')
    DROP TABLE [Domain].[Learning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE t.name = 'Apprenticeship' AND s.name = 'Domain')
    DROP TABLE [Domain].[Apprenticeship];