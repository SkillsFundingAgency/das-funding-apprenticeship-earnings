Feature: Delete short course learning

Scenario: Short course is deleted - instalments are removed
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                                      |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete, LearningComplete |
	When the short course learning is deleted
	Then the short course has no instalments
