CREATE TABLE [Domain].[ApprenticeshipInstalment]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [EpisodePriceKey] UNIQUEIDENTIFIER NULL, 
    [Type] NVARCHAR(50) NULL
)
GO
ALTER TABLE Domain.[ApprenticeshipInstalment]
ADD CONSTRAINT FK_ApprenticeshipInstalment_ApprenticeshipEarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.ApprenticeshipEarningsProfile ([EarningsProfileId])
GO
ALTER TABLE Domain.[ApprenticeshipInstalment]
ADD CONSTRAINT FK_ApprenticeshipInstalment_ApprenticeshipEpisodePrice FOREIGN KEY ([EpisodePriceKey]) REFERENCES Domain.ApprenticeshipEpisodePrice ([Key])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[ApprenticeshipInstalment] ([EarningsProfileId]);
GO