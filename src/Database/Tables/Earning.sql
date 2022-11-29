CREATE TABLE [query].[Earning]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ApprenticeshipKey] UNIQUEIDENTIFIER NOT NULL, 
    [Uln] NVARCHAR(10) NOT NULL, 
	[Ukprn] BIGINT NOT NULL,
    [ApprovalsApprenticeshipId] BIGINT NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [FundingEmployerAccountId] BIGINT NULL, 
    [FundingType] NVARCHAR(50) NOT NULL, 
    [DeliveryPeriod] TINYINT NULL,
	[AcademicYear] SMALLINT NULL,
    [Amount] DECIMAL(9, 2) NOT NULL
)
GO
CREATE INDEX IX_Earnings_ProviderAcademicYearFundingType ON [query].[Earning] (Ukprn, AcademicYear, FundingType) INCLUDE (Amount)