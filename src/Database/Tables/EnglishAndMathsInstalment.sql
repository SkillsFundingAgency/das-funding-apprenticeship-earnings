CREATE TABLE [Domain].[EnglishAndMathsInstalment]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL,
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL DEFAULT 'Regular'
)
GO
ALTER TABLE Domain.[EnglishAndMathsInstalment]
ADD CONSTRAINT FK_EnglishAndMathsInstalment_EnglishAndMaths FOREIGN KEY ([Key]) REFERENCES Domain.EnglishAndMaths ([Key])
GO