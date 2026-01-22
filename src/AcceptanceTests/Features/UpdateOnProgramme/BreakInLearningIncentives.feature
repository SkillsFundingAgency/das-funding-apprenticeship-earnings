Feature: BreakInLearningIncentivePayments

Acceptance tests related to effects breaks in learning has on incentive payments.

Scenario: Incentive Payments 16-18 are pushed back due to Break in Learning
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And the following on-programme request is sent
		| Key               | Value                                                                        |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-08-31, OriginalExpectedEndDate:2024-07-31 |
		| PeriodsInLearning | StartDate:2020-09-08, EndDate:2024-07-31, OriginalExpectedEndDate:2024-07-31 |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-11-05 |
		| EmployerIncentive | 500    | 2020-11-05 |
		| ProviderIncentive | 500    | 2021-08-07 |
		| EmployerIncentive | 500    | 2021-08-07 |

Scenario: Incentive Payments 19-24 are pushed back due to Break in Learning
	Given an apprenticeship has been created with the following information
		| Age |
		| 20  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And the following on-programme request is sent
		| Key                            | Value                                                                        |
		| PeriodsInLearning              | StartDate:2020-08-01, EndDate:2020-08-31, OriginalExpectedEndDate:2024-07-31 |
		| PeriodsInLearning              | StartDate:2020-09-08, EndDate:2024-07-31, OriginalExpectedEndDate:2024-07-31 |
		| CareLeaverEmployerConsentGiven | true                                                                         |
		| IsCareLeaver                   | true                                                                         |
		| HasEHCP                        | true                                                                         |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-11-05 |
		| EmployerIncentive | 500    | 2020-11-05 |
		| ProviderIncentive | 500    | 2021-08-07 |
		| EmployerIncentive | 500    | 2021-08-07 |
