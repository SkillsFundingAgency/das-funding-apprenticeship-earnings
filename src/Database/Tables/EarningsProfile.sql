CREATE TABLE [Domain].[EarningsProfile]
(
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [AdjustedPrice] MONEY NULL, 
    [CompletionPayment] MONEY NULL
)
GO
ALTER TABLE Domain.[EarningsProfile]
ADD CONSTRAINT FK_EarningsProfile_Episode FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[EarningsProfile] (EpisodeKey);
GO