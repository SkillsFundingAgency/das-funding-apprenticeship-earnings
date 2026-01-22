CREATE TABLE [Domain].[EpisodePeriodInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL,
    [OriginalExpectedEndDate] DATETIME NOT NULL
)
GO

ALTER TABLE Domain.EpisodePeriodInLearning
ADD CONSTRAINT FK_EpisodePeriodInLearning_Episode
FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO

CREATE INDEX IX_EpisodeKey
ON Domain.[EpisodePeriodInLearning] (EpisodeKey);
GO
