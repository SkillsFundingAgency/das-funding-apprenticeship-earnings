CREATE TABLE [dbo].[Episode]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ApprenticeshipKey] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [LegalEntityName] NVARCHAR(255) NOT NULL, 
    [TrainingCode] NCHAR(10) NOT NULL, 
    [FundingEmployerAccountId] BIGINT NULL, 
    [FundingType] INT NOT NULL, 
    [AgeAtStartOfApprenticeship] TINYINT NOT NULL
)
GO
ALTER TABLE dbo.Episode
ADD CONSTRAINT FK_Episode_Apprenticeship FOREIGN KEY (ApprenticeshipKey) REFERENCES dbo.Apprenticeship ([Key])
GO
CREATE INDEX IX_ApprenticeshipKey ON [dbo].[Episode] (ApprenticeshipKey);
GO