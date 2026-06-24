Feature: Update short course start date

Tests that the StartDate is persisted and used to recalculate the ThirtyPercentLearningComplete delivery period

Scenario: StartDate changed via update on programme - ThirtyPercentLearningComplete instalment moves to new period
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      | 2000       |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key       | Value      |
		| StartDate | 2021-02-01 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 8              | ThirtyPercentLearningComplete |
		| 1400   | 2021         | 11             | LearningComplete              |
