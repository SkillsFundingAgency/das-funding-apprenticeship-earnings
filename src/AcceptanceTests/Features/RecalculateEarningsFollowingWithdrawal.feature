Feature: Recalculate earnings following withdrawal

Scenario: Withdrawal made partway through apprenticeship; recalc earnings
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-11-15        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |

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
