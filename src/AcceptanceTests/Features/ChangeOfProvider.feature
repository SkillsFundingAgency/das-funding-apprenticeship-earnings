Feature: Change of Provider

Scenario: Second provider creates a new episode on an existing learning record
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And a short course has been created by a new provider with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-02-01 | 2021-07-25      |       3000 |
	Then the short course learning has 2 episodes
