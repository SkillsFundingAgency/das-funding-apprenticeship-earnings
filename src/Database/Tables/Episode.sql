CREATE TABLE [Domain].[Episode]
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
    [FundingBandMaximum] DECIMAL(15,5) NULL, 
    [TrainingType] NVARCHAR(50) NOT NULL
)
GO
ALTER TABLE Domain.Episode
ADD CONSTRAINT FK_Episode_Apprenticeship FOREIGN KEY (LearningKey) REFERENCES [Domain].[Learning] ([LearningKey])
GO
CREATE INDEX IX_learningKey ON Domain.[Episode] (LearningKey);
GO