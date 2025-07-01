CREATE TABLE [Query].[Earning]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [LearningKey] UNIQUEIDENTIFIER NOT NULL, 
    [Uln] NVARCHAR(10) NOT NULL, 
	[Ukprn] BIGINT NOT NULL,
    [ApprovalsApprenticeshipId] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [FundingEmployerAccountId] BIGINT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [DeliveryPeriod] TINYINT NULL,
	[AcademicYear] SMALLINT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [LearningEpisodeKey] UNIQUEIDENTIFIER NULL, 
    [IsNonLevyFullyFunded] BIT NOT NULL DEFAULT 0
)
GO
CREATE INDEX IX_Earnings_ProviderAcademicYearFundingType ON [Query].[Earning] (Ukprn, AcademicYear, FundingType) INCLUDE (Amount)