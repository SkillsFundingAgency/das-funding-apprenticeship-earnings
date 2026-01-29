CREATE TABLE [Domain].[MathsAndEnglish]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [Course] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [WithdrawalDate] DATETIME NULL,
    [ActualEndDate] DATETIME NULL,
    [PriorLearningAdjustmentPercentage] INT NULL, 
    [PauseDate] DATETIME NULL, 
    [CompletionDate] DATETIME NULL, 
    [LearnAimRef] VARCHAR(8) NOT NULL DEFAULT ''
)
GO

CREATE INDEX IX_MathsAndEnglish_EarningsProfileId
    ON [Domain].[MathsAndEnglish] (EarningsProfileId);