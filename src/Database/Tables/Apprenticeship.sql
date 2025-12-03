CREATE TABLE [Domain].[Apprenticeship]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ApprovalsApprenticeshipId] BIGINT NOT NULL,
    [Uln] NVARCHAR(10) NOT NULL, 
    [HasEHCP] BIT NULL, 
    [IsCareLeaver] BIT NULL, 
    [CareLeaverEmployerConsentGiven] BIT NULL, 
    [DateOfBirth] DATETIME NOT NULL DEFAULT ('2002-01-01')
)
