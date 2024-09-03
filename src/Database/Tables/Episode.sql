﻿CREATE TABLE [Domain].[Episode]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[ApprenticeshipKey] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [FundingEmployerAccountId] BIGINT NULL, 
    [LegalEntityName] NVARCHAR(255) NOT NULL,
    [TrainingCode] NCHAR(50) NOT NULL,
    [AgeAtStartOfApprenticeship] INT NOT NULL
)
GO
ALTER TABLE Domain.Episode
ADD CONSTRAINT FK_Episode_Apprenticeship FOREIGN KEY (ApprenticeshipKey) REFERENCES Domain.Apprenticeship ([Key])
GO
CREATE INDEX IX_ApprenticeshipKey ON Domain.[Episode] (ApprenticeshipKey);
GO