/*
Post-Deployment Script
*/

-- FLP-1890 (delete on release)
-- Backfill ShortCourseLearning.TrainingCode from ShortCourseEpisode.TrainingCode.
IF COL_LENGTH('[Domain].[ShortCourseLearning]', 'TrainingCode') IS NOT NULL
BEGIN
	UPDATE [scl]
	SET [scl].[TrainingCode] = [sce].[TrainingCode]
	FROM [Domain].[ShortCourseLearning] AS [scl]
	INNER JOIN [Domain].[ShortCourseEpisode] AS [sce]
	ON [sce].[LearningKey] = [scl].[LearningKey]
	WHERE [scl].[TrainingCode] = ''
END
