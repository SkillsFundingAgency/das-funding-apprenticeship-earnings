CREATE TABLE [Domain].[ApprenticeshipPeriodInLearning]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL,
    [OriginalExpectedEndDate] DATETIME NOT NULL
)
GO

ALTER TABLE Domain.ApprenticeshipPeriodInLearning
ADD CONSTRAINT FK_ApprenticeshipPeriodInLearning_ApprenticeshipEpisode
FOREIGN KEY (EpisodeKey) REFERENCES Domain.ApprenticeshipEpisode ([Key])
GO

CREATE INDEX IX_EpisodeKey
ON Domain.[ApprenticeshipPeriodInLearning] (EpisodeKey);
GO
