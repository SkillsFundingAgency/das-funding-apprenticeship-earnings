CREATE TABLE [Domain].[EnglishAndMathsPeriodInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL,
    [OriginalExpectedEndDate] DATETIME NOT NULL
)
GO

ALTER TABLE Domain.EnglishAndMathsPeriodInLearning
ADD CONSTRAINT FK_EnglishAndMathsPeriodInLearning_EnglishAndMaths
FOREIGN KEY (EnglishAndMathsKey)
REFERENCES Domain.EnglishAndMaths ([Key])
ON DELETE CASCADE;
GO

CREATE INDEX IX_EnglishAndMaths
ON Domain.[EnglishAndMathsPeriodInLearning] (EnglishAndMathsKey);
GO
