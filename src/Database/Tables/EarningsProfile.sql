CREATE TABLE [Domain].[EarningsProfile]
(
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [OnProgramTotal] DECIMAL(15,5) NOT NULL, 
    [CompletionPayment] DECIMAL(15,5) NULL
)
GO
ALTER TABLE Domain.[EarningsProfile]
ADD CONSTRAINT FK_EarningsProfile_Episode FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[EarningsProfile] (EpisodeKey);
GO