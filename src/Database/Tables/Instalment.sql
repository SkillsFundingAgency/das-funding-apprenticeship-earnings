CREATE TABLE [dbo].[Instalment]
(
	[Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [EarningProfileKey] UNIQUEIDENTIFIER NOT NULL, 
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL, 
    [Amount] MONEY NOT NULL
)
GO
ALTER TABLE dbo.Instalment
ADD CONSTRAINT FK_Instalment_EarningProfile FOREIGN KEY (EarningProfileKey) REFERENCES dbo.EarningProfile ([Key])
GO
CREATE INDEX IX_EarningProfileKey ON [dbo].[Instalment] (EarningProfileKey);
GO