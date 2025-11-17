/*
Post-Deployment Script
*/

--FLP-1444 - Remove Query.Earning
IF OBJECT_ID('[Query].[Earning]', 'U') IS NOT NULL
    DROP TABLE [Query].[Earning];

