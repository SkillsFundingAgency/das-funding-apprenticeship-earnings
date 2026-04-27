CREATE TABLE [Domain].[ShortCourseEpisode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[LearningKey] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] BIGINT NOT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [TrainingCode] NCHAR(50) NOT NULL,
    [CompletionDate] DATETIME NULL, 
    [AchievementDate] DATETIME NULL, 
    [WithdrawalDate] DATETIME NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [CoursePrice] DECIMAL(15,5) NOT NULL,
    [Milestones] INT NOT NULL DEFAULT 0
)
GO
ALTER TABLE Domain.ShortCourseEpisode
ADD CONSTRAINT FK_ShortCourseEpisode_Learning FOREIGN KEY (LearningKey) REFERENCES [Domain].[ShortCourseLearning] ([LearningKey])
GO
CREATE INDEX IX_learningKey ON Domain.[ShortCourseEpisode] (LearningKey);
GO
