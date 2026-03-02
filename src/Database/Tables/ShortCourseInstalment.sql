CREATE TABLE [Domain].[ShortCourseInstalment]
(
    [Key] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [EarningsProfileId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYear] SMALLINT NOT NULL, 
    [DeliveryPeriod] TINYINT NOT NULL,
    [Amount] DECIMAL(15,5) NOT NULL, 
    [Type] NVARCHAR(50) NULL
)
GO
ALTER TABLE Domain.[ShortCourseInstalment]
ADD CONSTRAINT FK_ShortCourseInstalment_ShortCourseEarningsProfile FOREIGN KEY ([EarningsProfileId]) REFERENCES Domain.ShortCourseEarningsProfile ([EarningsProfileId])
GO
CREATE INDEX IX_EarningsProfileId ON Domain.[ShortCourseInstalment] ([EarningsProfileId]);
GO