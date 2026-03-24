Feature: Get short course earnings via API

Scenario: Short course earnings are returned for a given learning key and ukprn
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	When I request the short course earnings
	Then the earnings response contains
		| CollectionYear | CollectionPeriod | Amount | Type                          |
		| 2021           | 7                | 600    | ThirtyPercentLearningComplete |
		| 2021           | 11               | 1400   | LearningComplete              |
