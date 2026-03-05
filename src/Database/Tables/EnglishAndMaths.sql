CREATE TABLE [Domain].[EnglishAndMaths]
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
ALTER TABLE Domain.[EnglishAndMaths]
ADD CONSTRAINT FK_EnglishAndMaths_ApprenticeshipEarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.ApprenticeshipEarningsProfile ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[EnglishAndMaths] ([EarningsProfileId]);
GO

CREATE INDEX IX_EnglishAndMaths_EarningsProfileId
    ON [Domain].[EnglishAndMaths] (EarningsProfileId);