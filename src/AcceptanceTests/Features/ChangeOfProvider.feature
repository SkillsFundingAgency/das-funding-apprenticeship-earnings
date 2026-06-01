Feature: Change of Provider

Scenario: Second provider creates a new episode on an existing learning record
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And a short course has been created by a new provider with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-02-01 | 2021-07-25      |       3000 |
	Then the short course learning has 2 episodes

Scenario: Provider A did not claim 30% - Provider B gets both unapproved earnings
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	When Short Course Update OnProgramme is triggered with
		| Key            | Value      |
		| WithdrawalDate | 2021-03-01 |
	Given a short course has been created by a new provider with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-03-15 | 2021-09-15      |       3000 |
	Then On programme short course earnings for the current episode are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		|    900 |         2021 |             10 | ThirtyPercentLearningComplete |
		|   2100 |         2122 |              2 | LearningComplete              |

Scenario: Provider A claimed 30% - Provider B unapproved 30% earning is suppressed
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      |       2000 | ThirtyPercentLearningComplete |
	When Short Course Update OnProgramme is triggered with
		| Key            | Value      |
		| WithdrawalDate | 2021-03-01 |
	Given a short course has been created by a new provider with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-03-15 | 2021-09-15      |       3000 |
	Then On programme short course earnings for the current episode are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type             |
		|   2100 |         2122 |              2 | LearningComplete |
