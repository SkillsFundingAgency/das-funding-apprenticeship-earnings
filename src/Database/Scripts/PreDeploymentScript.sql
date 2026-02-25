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

DELETE FROM [Domain].[MathsAndEnglishPeriodInLearning];
DELETE FROM [Domain].[MathsAndEnglishInstalment];
DELETE FROM [Domain].[MathsAndEnglish];

DELETE FROM [Domain].[Instalment];
DELETE FROM [Domain].[AdditionalPayment];
DELETE FROM [History].[EarningsProfileHistory];
DELETE FROM [Domain].[EarningsProfile];

DELETE FROM [Domain].[EpisodePrice];
DELETE FROM [Domain].[EpisodePeriodInLearning];
DELETE FROM [Domain].[Episode];

DELETE FROM [Domain].[Apprenticeship];

ROLLBACK TRAN
GO