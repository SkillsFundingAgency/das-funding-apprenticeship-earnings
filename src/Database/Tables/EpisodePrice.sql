CREATE TABLE [Domain].[EpisodePrice]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[EpisodeKey] UNIQUEIDENTIFIER NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [AgreedPrice] MONEY NULL, 
    [FundingBandMaximum] INT NOT NULL
)
GO
ALTER TABLE Domain.EpisodePrice
ADD CONSTRAINT FK_EpisodePrice_Episode FOREIGN KEY (EpisodeKey) REFERENCES Domain.Episode ([Key])
GO
CREATE INDEX IX_EpisodeKey ON Domain.[EpisodePrice] (EpisodeKey);
GO