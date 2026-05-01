Feature: Achievement

Scenario: Achievement date is recorded after the completion date
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
		| AchievementDate| 2021-03-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 6000   | 2021         | 7              | Balancing  |
		| 3000   | 2021         | 8              | Completion |

Scenario: Achievement date is recorded then removed
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
		| AchievementDate| 2021-03-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| AchievementDate| null       |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 6000   | 2021         | 7              | Balancing  |

Scenario: Achievement date is recorded after the completion date then moved even later
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
		| AchievementDate| 2021-03-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| AchievementDate| 2021-04-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 6000   | 2021         | 7              | Balancing  |
		| 3000   | 2021         | 9              | Completion |
