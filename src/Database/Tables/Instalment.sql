﻿CREATE TABLE [Domain].[Instalment]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] MONEY NOT NULL
)
GO
ALTER TABLE Domain.[Instalment]
ADD CONSTRAINT FK_Instalment_EarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.EarningsProfile ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[Instalment] ([EarningsProfileId]);
GO