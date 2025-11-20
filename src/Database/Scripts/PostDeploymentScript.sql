/*
Post-Deployment Script
*/

--FLP-1444 - Remove Query.Earning
IF OBJECT_ID('[Query].[Earning]', 'U') IS NOT NULL
    DROP TABLE [Query].[Earning];


--FLP-1321 - Move FundingBandMaximum
--This scripts copies funding band from EpisodePrice to Episode for existing records
--To be removed in a follow-up deployment once released to prod
UPDATE e
SET e.FundingBandMaximum = (
    SELECT TOP 1 ep.FundingBandMaximum
    FROM Domain.EpisodePrice ep
    WHERE ep.[EpisodeKey] = e.[Key]
)
FROM Domain.Episode e
WHERE e.FundingBandMaximum IS NULL;



