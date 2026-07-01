Feature: Update short course expected end date

Tests that the ExpectedEndDate is persisted and used to recalculate the LearningComplete delivery period

Scenario: ExpectedEndDate changed via update on programme
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      | 2000       |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key             | Value      |
		| ExpectedEndDate | 2021-08-25 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 8              | ThirtyPercentLearningComplete |
		| 1400   | 2122         | 1              | LearningComplete              |
	And the payments event is sent to pv2 with the correct information
