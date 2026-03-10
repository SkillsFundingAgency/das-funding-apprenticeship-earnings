CREATE TABLE [History].[ShortCourseEarningsProfileHistory]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
	[Version] UNIQUEIDENTIFIER NOT NULL,
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[State] NVARCHAR(MAX) NOT NULL,
)
GO

CREATE NONCLUSTERED INDEX IX_ShortCourseEarningsProfileHistory_Version
    ON [History].[ShortCourseEarningsProfileHistory] ([Version]);