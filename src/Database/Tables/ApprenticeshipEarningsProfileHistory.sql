CREATE TABLE [History].[ApprenticeshipEarningsProfileHistory]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
	[Version] UNIQUEIDENTIFIER NOT NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[State] NVARCHAR(MAX) NOT NULL,
	
	CONSTRAINT FK_ApprenticeshipEarningsProfileHistory_ApprenticeshipEarningsProfile
        FOREIGN KEY ([EarningsProfileId])
        REFERENCES Domain.[ApprenticeshipEarningsProfile]([EarningsProfileId])
)
GO

CREATE NONCLUSTERED INDEX IX_ApprenticeshipEarningsProfileHistory_Version
    ON [History].[ApprenticeshipEarningsProfileHistory] ([Version]);


