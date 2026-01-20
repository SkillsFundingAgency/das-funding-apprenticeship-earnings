CREATE TABLE [Domain].[MathsAndEnglishPeriodInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [MathsAndEnglishKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL,
    [OriginalExpectedEndDate] DATETIME NOT NULL
)
GO

ALTER TABLE Domain.MathsAndEnglishPeriodInLearning
ADD CONSTRAINT FK_MathsAndEnglishPeriodInLearning_MathsAndEnglish
FOREIGN KEY (MathsAndEnglishKey) REFERENCES Domain.MathsAndEnglish ([Key])
GO

CREATE INDEX IX_MathsAndEnglish
ON Domain.[MathsAndEnglishPeriodInLearning] (MathsAndEnglishKey);
GO
