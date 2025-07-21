CREATE TABLE [Domain].[MathsAndEnglish]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATETIME NOT NULL,
    [EndDate] DATETIME NOT NULL,
    [Course] NCHAR(50) NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [WithdrawalDate] DATETIME NULL,
    [ActualEndDate] DATETIME NULL
)
GO