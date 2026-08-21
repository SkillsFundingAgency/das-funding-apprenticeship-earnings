DECLARE @Uln NVARCHAR(10) = '9999999999';

DECLARE @ApprenticeshipLearningKeys TABLE ([LearningKey] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ShortCourseLearningKeys TABLE ([LearningKey] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ApprenticeshipEpisodeKeys TABLE ([Key] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ShortCourseEpisodeKeys TABLE ([Key] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ApprenticeshipEarningsProfileIds TABLE ([EarningsProfileId] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @ShortCourseEarningsProfileIds TABLE ([EarningsProfileId] UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @HasData BIT = 0;

INSERT INTO @ApprenticeshipLearningKeys ([LearningKey])
SELECT [LearningKey]
FROM [Domain].[ApprenticeshipLearning]
WHERE [Uln] = @Uln;

INSERT INTO @ShortCourseLearningKeys ([LearningKey])
SELECT [LearningKey]
FROM [Domain].[ShortCourseLearning]
WHERE [Uln] = @Uln;

IF EXISTS (SELECT 1 FROM @ApprenticeshipLearningKeys) OR EXISTS (SELECT 1 FROM @ShortCourseLearningKeys)
BEGIN
    SET @HasData = 1;
END

IF @HasData = 0
BEGIN
    PRINT CONCAT('No records found for ULN ', @Uln, '. Nothing to delete.');
    RETURN;
END

INSERT INTO @ApprenticeshipEpisodeKeys ([Key])
SELECT [Key]
FROM [Domain].[ApprenticeshipEpisode]
WHERE [LearningKey] IN (SELECT [LearningKey] FROM @ApprenticeshipLearningKeys);

INSERT INTO @ShortCourseEpisodeKeys ([Key])
SELECT [Key]
FROM [Domain].[ShortCourseEpisode]
WHERE [LearningKey] IN (SELECT [LearningKey] FROM @ShortCourseLearningKeys);

INSERT INTO @ApprenticeshipEarningsProfileIds ([EarningsProfileId])
SELECT [EarningsProfileId]
FROM [Domain].[ApprenticeshipEarningsProfile]
WHERE [EpisodeKey] IN (SELECT [Key] FROM @ApprenticeshipEpisodeKeys);

INSERT INTO @ShortCourseEarningsProfileIds ([EarningsProfileId])
SELECT [EarningsProfileId]
FROM [Domain].[ShortCourseEarningsProfile]
WHERE [EpisodeKey] IN (SELECT [Key] FROM @ShortCourseEpisodeKeys);

BEGIN TRANSACTION;

    DELETE ap
    FROM [Domain].[ApprenticeshipAdditionalPayment] ap
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = ap.[EarningsProfileId];

    DELETE ai
    FROM [Domain].[ApprenticeshipInstalment] ai
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = ai.[EarningsProfileId];

    DELETE empi
    FROM [Domain].[EnglishAndMathsPeriodInLearning] empi
    INNER JOIN [Domain].[EnglishAndMaths] em ON em.[Key] = empi.[EnglishAndMathsKey]
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = em.[EarningsProfileId];

    DELETE emi
    FROM [Domain].[EnglishAndMathsInstalment] emi
    INNER JOIN [Domain].[EnglishAndMaths] em ON em.[Key] = emi.[EnglishAndMathsKey]
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = em.[EarningsProfileId];

    DELETE em
    FROM [Domain].[EnglishAndMaths] em
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = em.[EarningsProfileId];

    DELETE aeph
    FROM [History].[ApprenticeshipEarningsProfileHistory] aeph
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = aeph.[EarningsProfileId];

    DELETE aep
    FROM [Domain].[ApprenticeshipEarningsProfile] aep
    INNER JOIN @ApprenticeshipEarningsProfileIds ep ON ep.[EarningsProfileId] = aep.[EarningsProfileId];

    DELETE apil
    FROM [Domain].[ApprenticeshipPeriodInLearning] apil
    INNER JOIN @ApprenticeshipEpisodeKeys ek ON ek.[Key] = apil.[EpisodeKey];

    DELETE aprice
    FROM [Domain].[ApprenticeshipEpisodePrice] aprice
    INNER JOIN @ApprenticeshipEpisodeKeys ek ON ek.[Key] = aprice.[EpisodeKey];

    DELETE ae
    FROM [Domain].[ApprenticeshipEpisode] ae
    INNER JOIN @ApprenticeshipEpisodeKeys ek ON ek.[Key] = ae.[Key];

    DELETE al
    FROM [Domain].[ApprenticeshipLearning] al
    INNER JOIN @ApprenticeshipLearningKeys lk ON lk.[LearningKey] = al.[LearningKey];

    DELETE sci
    FROM [Domain].[ShortCourseInstalment] sci
    INNER JOIN @ShortCourseEarningsProfileIds ep ON ep.[EarningsProfileId] = sci.[EarningsProfileId];

    DELETE sceph
    FROM [History].[ShortCourseEarningsProfileHistory] sceph
    INNER JOIN @ShortCourseEarningsProfileIds ep ON ep.[EarningsProfileId] = sceph.[EarningsProfileId];

    DELETE scep
    FROM [Domain].[ShortCourseEarningsProfile] scep
    INNER JOIN @ShortCourseEarningsProfileIds ep ON ep.[EarningsProfileId] = scep.[EarningsProfileId];

    DELETE sce
    FROM [Domain].[ShortCourseEpisode] sce
    INNER JOIN @ShortCourseEpisodeKeys ek ON ek.[Key] = sce.[Key];

    DELETE scl
    FROM [Domain].[ShortCourseLearning] scl
    INNER JOIN @ShortCourseLearningKeys lk ON lk.[LearningKey] = scl.[LearningKey];

--COMMIT TRANSACTION;
ROLLBACK TRANSACTION;
