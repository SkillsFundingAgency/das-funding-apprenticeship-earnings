CREATE TABLE [Domain].[ShortCourseEpisode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[LearningKey] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [FundingEmployerAccountId] BIGINT NULL, 
    [LegalEntityName] NVARCHAR(255) NOT NULL,
    [TrainingCode] NCHAR(50) NOT NULL,
    [CompletionDate] DATETIME NULL, 
    [WithdrawalDate] DATETIME NULL, 
    [PauseDate] DATETIME NULL,
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [CoursePrice] DECIMAL(15,5) NOT NULL
)
GO
ALTER TABLE Domain.ShortCourseEpisode
ADD CONSTRAINT FK_ShortCourseEpisode_Learning FOREIGN KEY (LearningKey) REFERENCES [Domain].[Learning] ([LearningKey])
GO
CREATE INDEX IX_learningKey ON Domain.[ShortCourseEpisode] (LearningKey);
GO
