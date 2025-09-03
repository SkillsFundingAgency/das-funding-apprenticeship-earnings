--FLP-1274 - Reworking the way history is written - delete obsolete tables

-- Drop [Domain].[InstalmentHistory] if it exists
IF OBJECT_ID('[Domain].[InstalmentHistory]', 'U') IS NOT NULL
    DROP TABLE [Domain].[InstalmentHistory];

-- Drop [Domain].[AdditionalPaymentHistory] if it exists
IF OBJECT_ID('[Domain].[AdditionalPaymentHistory]', 'U') IS NOT NULL
    DROP TABLE [Domain].[AdditionalPaymentHistory];

-- Drop [Domain].[EarningsProfileHistory] if it exists
IF OBJECT_ID('[Domain].[EarningsProfileHistory]', 'U') IS NOT NULL
    DROP TABLE [Domain].[EarningsProfileHistory];