CREATE TABLE [Domain].[EpisodeBreakInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL
)
GO
ALTER TABLE Domain.EpisodeBreakInLearning
ADD CONSTRAINT FK_EpisodeBreakInLearning_Episode FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[EpisodeBreakInLearning] (EpisodeKey);
GO