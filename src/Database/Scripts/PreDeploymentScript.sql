/*
Pre-Deployment Script
*/

/* FLP-1520: Remove BiL table */
IF EXISTS (SELECT 1 FROM sys.tables t
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'EpisodeBreakInLearning'
      AND s.name = 'Domain'
)
BEGIN
    DROP TABLE [Domain].[EpisodeBreakInLearning];
END



/* This can be deleted after 1421 is deployed to prod */
IF OBJECT_ID('Domain.MathsAndEnglishPeriodInLearning', 'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_MathsAndEnglishPeriodInLearning_MathsAndEnglish'
    )
    BEGIN
        ALTER TABLE Domain.MathsAndEnglishPeriodInLearning
        DROP CONSTRAINT FK_MathsAndEnglishPeriodInLearning_MathsAndEnglish;
    END
END
GO

--FLP-1493 DELETE AFTER
BEGIN TRAN

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'MathsAndEnglishPeriodInLearning' AND s.name = 'Domain')
    DELETE FROM [Domain].[MathsAndEnglishPeriodInLearning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'MathsAndEnglishInstalment' AND s.name = 'Domain')
    DELETE FROM [Domain].[MathsAndEnglishInstalment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'MathsAndEnglish' AND s.name = 'Domain')
    DELETE FROM [Domain].[MathsAndEnglish];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'Instalment' AND s.name = 'Domain')
    DELETE FROM [Domain].[Instalment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'AdditionalPayment' AND s.name = 'Domain')
    DELETE FROM [Domain].[AdditionalPayment];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'EarningsProfileHistory' AND s.name = 'History')
    DELETE FROM [History].[EarningsProfileHistory];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'EarningsProfile' AND s.name = 'Domain')
    DELETE FROM [Domain].[EarningsProfile];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'EpisodePrice' AND s.name = 'Domain')
    DELETE FROM [Domain].[EpisodePrice];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'EpisodePeriodInLearning' AND s.name = 'Domain')
    DELETE FROM [Domain].[EpisodePeriodInLearning];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'Episode' AND s.name = 'Domain')
    DELETE FROM [Domain].[Episode];

IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id 
           WHERE t.name = 'Apprenticeship' AND s.name = 'Domain')
    DELETE FROM [Domain].[Apprenticeship];

COMMIT TRAN
GO