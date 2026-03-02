CREATE TABLE [Domain].[ApprenticeshipEpisodePrice]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [AgreedPrice] DECIMAL(15,5) NOT NULL
)
GO
ALTER TABLE Domain.ApprenticeshipEpisodePrice
ADD CONSTRAINT FK_ApprenticeshipEpisodePrice_ApprenticeshipEpisode FOREIGN KEY (EpisodeKey) REFERENCES Domain.ApprenticeshipEpisode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[ApprenticeshipEpisodePrice] (EpisodeKey);
GO