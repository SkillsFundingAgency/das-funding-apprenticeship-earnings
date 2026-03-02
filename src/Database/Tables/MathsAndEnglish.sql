CREATE TABLE [Domain].[MathsAndEnglish]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [Course] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [WithdrawalDate] DATETIME NULL,
    [PriorLearningAdjustmentPercentage] INT NULL, 
    [PauseDate] DATETIME NULL, 
    [CompletionDate] DATETIME NULL, 
    [LearnAimRef] VARCHAR(8) NOT NULL DEFAULT ''
)
GO

GO
ALTER TABLE Domain.[MathsAndEnglish]
ADD CONSTRAINT FK_MathsAndEnglish_ApprenticeshipEarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.ApprenticeshipEarningsProfile ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[MathsAndEnglish] ([EarningsProfileId]);
GO

CREATE INDEX IX_MathsAndEnglish_EarningsProfileId
    ON [Domain].[MathsAndEnglish] (EarningsProfileId);