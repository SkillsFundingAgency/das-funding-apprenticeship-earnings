CREATE TABLE [History].[EnglishAndMathsEarningsProfileHistory]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EnglishAndMathsKey] UNIQUEIDENTIFIER NOT NULL,
	[EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
	[Version] UNIQUEIDENTIFIER NOT NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[State] NVARCHAR(MAX) NOT NULL,
)
GO

CREATE NONCLUSTERED INDEX IX_EnglishAndMathsEarningsProfileHistory_Version
    ON [History].[EnglishAndMathsEarningsProfileHistory] ([Version]);
