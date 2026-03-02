CREATE TABLE [History].[EarningsProfileHistory]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
	[Version] UNIQUEIDENTIFIER NOT NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[State] NVARCHAR(MAX) NOT NULL,
	
	CONSTRAINT FK_EarningsProfileHistory_ApprenticeshipEarningsProfile
        FOREIGN KEY ([EarningsProfileId])
        REFERENCES Domain.[ApprenticeshipEarningsProfile]([EarningsProfileId])
)
GO

CREATE NONCLUSTERED INDEX IX_EarningsProfileHistory_Version
    ON [History].[EarningsProfileHistory] ([Version]);


