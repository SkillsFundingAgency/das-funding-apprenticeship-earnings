Feature: GetShortCourse

Tests the Get Short Course Endpoint

Scenario: Get endpoint returns correct short course data
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete |
	And a LearningApproved event is received for the short course
	When a Get Short Course request is made for the short course
	Then the following data is returned from the get request
		| Amount | CollectionYear | CollectionPeriod | Type                          | IsPayable |
		| 600    | 2021           | 7                | ThirtyPercentLearningComplete | True      |
		| 1400   | 2021           | 11               | LearningComplete              | False     |
