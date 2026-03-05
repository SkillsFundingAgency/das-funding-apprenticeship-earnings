CREATE TABLE [Domain].[ShortCourseEarningsProfile]
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
ALTER TABLE Domain.[ShortCourseEarningsProfile]
ADD CONSTRAINT FK_ShortCourseEarningsProfile_ShortCourseEpisode FOREIGN KEY (EpisodeKey) REFERENCES Domain.ShortCourseEpisode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[ShortCourseEarningsProfile] (EpisodeKey);
GO