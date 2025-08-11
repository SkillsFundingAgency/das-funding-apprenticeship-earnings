Feature: Recalculate earnings following withdrawal before end of qualification period reached

Scenario: Withdrawal made before end of 42 day qualifying period; recalc earnings as zero
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-09-15        |
	Then no on programme earnings are persisted

	Scenario: Withdrawal made before end of 14 qualifying period; recalc earnings as zero
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-01-31 | 2020-02-13 | 15000 | 25000              |
	And earnings are calculated
	When the following withdrawal is sent
		| LastDayOfLearning |
		| 2020-02-12        |
	Then no on programme earnings are persisted