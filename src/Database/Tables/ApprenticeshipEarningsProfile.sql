CREATE TABLE [Domain].[ApprenticeshipEarningsProfile]
(
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [OnProgramTotal] DECIMAL(15,5) NOT NULL, 
    [CompletionPayment] DECIMAL(15,5) NULL, 
    [Version] UNIQUEIDENTIFIER NULL, 
    [IsApproved] BIT NOT NULL, 
    [CalculationData] NVARCHAR(MAX) NOT NULL
)
GO
ALTER TABLE Domain.[ApprenticeshipEarningsProfile]
ADD CONSTRAINT FK_ApprenticeshipEarningsProfile_ApprenticeshipEpisode FOREIGN KEY (EpisodeKey) REFERENCES Domain.ApprenticeshipEpisode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[ApprenticeshipEarningsProfile] (EpisodeKey);
GO