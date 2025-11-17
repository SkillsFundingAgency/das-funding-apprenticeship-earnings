Feature: Recalculate earnings following withdrawal

Scenario: Withdrawal made partway through apprenticeship; recalc earnings
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-01 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-11-15        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | true                 |
		| EmployerIncentive | 500    | 2021-07-31 | true                 |

Scenario: Withdrawal made back to start of apprenticeship; remove all incentive payments
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-08-15        |
	Then no Additional Payments are persisted
	And the earnings history is maintained

Scenario: Withdrawal date falls before 90 day incentive date
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-01 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-10-15        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | true                 |
		| EmployerIncentive | 500    | 2020-10-29 | true                 |
		| ProviderIncentive | 500    | 2021-07-31 | true                 |
		| EmployerIncentive | 500    | 2021-07-31 | true                 |

Scenario: Withdrawal date falls after 90 day incentive date, but before census date (incentives still due)
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-01 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-10-30        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | true                 |
		| EmployerIncentive | 500    | 2021-07-31 | true                 |