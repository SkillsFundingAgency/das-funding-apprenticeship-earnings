-- Script to clear all data from the database

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
--COMMIT TRAN
GO
