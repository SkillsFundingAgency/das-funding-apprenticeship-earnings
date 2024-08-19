CREATE TABLE [dbo].[EarningProfile]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [AdjustedPrice] MONEY NOT NULL, 
    [CompletionPayment] MONEY NOT NULL, 
    [SupersededDate] DATETIME NULL
)
GO
ALTER TABLE dbo.EarningProfile
ADD CONSTRAINT FK_EarningProfile_Episode FOREIGN KEY (EpisodeKey) REFERENCES dbo.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON [dbo].[EarningProfile] (EpisodeKey);
GO