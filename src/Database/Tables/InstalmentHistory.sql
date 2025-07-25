﻿CREATE TABLE [Domain].[InstalmentHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [EpisodePriceKey] UNIQUEIDENTIFIER NULL, 
    [Type] NCHAR(50) NULL,
    [OriginalKey] UNIQUEIDENTIFIER NULL,
    [Version] UNIQUEIDENTIFIER NULL
)
GO
ALTER TABLE Domain.[InstalmentHistory]
ADD CONSTRAINT FK_InstalmentHistory_EarningsProfileHistory FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.EarningsProfileHistory ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[InstalmentHistory] ([EarningsProfileId]);
GO