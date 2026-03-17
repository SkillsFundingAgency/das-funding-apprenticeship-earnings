Feature: Update short course completion date

Tests that the completion date is persisted and used to calculate the delivery period of the LearningComplete instalment.

Scenario: Completion date set via update on programme - LearningComplete instalment moves to completion period
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      | 2000       |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value                                           |
		| CompletionDate | 2021-05-25                                      |
		| Milestones     | ThirtyPercentLearningComplete, LearningComplete |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 7              | ThirtyPercentLearningComplete |
		| 1400   | 2021         | 10             | LearningComplete              |

Scenario: Completion date cleared via update on programme - LearningComplete instalment reverts to end date period
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | CompletionDate |
		| 2021-01-01 | 2021-06-25      | 2000       | 2021-05-25     |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value                                           |
		| CompletionDate |                                                 |
		| Milestones     | ThirtyPercentLearningComplete, LearningComplete |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 7              | ThirtyPercentLearningComplete |
		| 1400   | 2021         | 11             | LearningComplete              |
