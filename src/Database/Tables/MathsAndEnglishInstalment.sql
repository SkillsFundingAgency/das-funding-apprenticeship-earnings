CREATE TABLE [Domain].[MathsAndEnglishInstalment]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [MathsAndEnglishKey] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL,
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL,
    [Type] NVARCHAR(50) NOT NULL DEFAULT 'Regular'
)
GO
ALTER TABLE Domain.[MathsAndEnglishInstalment]
ADD CONSTRAINT FK_MathsAndEnglishInstalment_MathsAndEnglish FOREIGN KEY ([Key]) REFERENCES Domain.MathsAndEnglish ([Key])
GO