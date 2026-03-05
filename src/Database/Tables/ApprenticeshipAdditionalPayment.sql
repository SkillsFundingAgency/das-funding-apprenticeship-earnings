CREATE TABLE [Domain].[ApprenticeshipAdditionalPayment]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL,
    [AdditionalPaymentType] NVARCHAR(20) NOT NULL,
    [DueDate] DATETIME NOT NULL
)
GO


GO
ALTER TABLE Domain.[ApprenticeshipAdditionalPayment]
ADD CONSTRAINT FK_ApprenticeshipAdditionalPayment_ApprenticeshipEarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.ApprenticeshipEarningsProfile ([EarningsProfileId])
GO

CREATE INDEX IX_ApprenticeshipAdditionalPayment_EarningsProfileId
    ON [Domain].[ApprenticeshipAdditionalPayment] (EarningsProfileId);
GO