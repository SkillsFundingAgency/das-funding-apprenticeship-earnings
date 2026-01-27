/*
Pre-Deployment Script
*/



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