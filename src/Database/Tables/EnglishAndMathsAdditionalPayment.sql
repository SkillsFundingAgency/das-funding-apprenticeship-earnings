CREATE TABLE [Domain].[EnglishAndMathsAdditionalPayment]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL,
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL,
    [AdditionalPaymentType] NVARCHAR(20) NOT NULL,
    [DueDate] DATETIME NOT NULL
)
GO
ALTER TABLE Domain.[EnglishAndMathsAdditionalPayment]
ADD CONSTRAINT FK_EnglishAndMathsAdditionalPayment_EnglishAndMaths FOREIGN KEY ([EnglishAndMathsKey]) REFERENCES Domain.EnglishAndMaths ([Key])
GO
CREATE INDEX IX_EnglishAndMathsAdditionalPayment_EnglishAndMathsKey
    ON [Domain].[EnglishAndMathsAdditionalPayment] (EnglishAndMathsKey);
GO
