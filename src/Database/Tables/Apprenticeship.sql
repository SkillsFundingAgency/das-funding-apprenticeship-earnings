CREATE TABLE [dbo].[Apprenticeship]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [ApprovalsApprenticeshipId] BIGINT NOT NULL, 
    [Uln] NVARCHAR(10) NOT NULL
)
