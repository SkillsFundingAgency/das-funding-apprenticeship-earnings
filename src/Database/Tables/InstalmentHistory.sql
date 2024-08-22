CREATE TABLE [Domain].[InstalmentHistory]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] MONEY NOT NULL
)
GO
ALTER TABLE Domain.[InstalmentHistory]
ADD CONSTRAINT FK_InstalmentHistory_EarningsProfileHistory FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.EarningsProfile ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[InstalmentHistory] ([EarningsProfileId]);
GO