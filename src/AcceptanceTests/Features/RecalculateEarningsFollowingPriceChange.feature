Feature: Recalculate earnings following price change

Scenario: Price change approved in the year it was requested, below or at funding band max; recalc earnings (by request)
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2021-02-15 |
		| NewTrainingPrice   | 18000      |
		| NewAssessmentPrice | 750        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
		| 1000   | 2021         | 4              |
		| 1000   | 2021         | 5              |
		| 1000   | 2021         | 6              |
		| 1500   | 2021         | 7              |
		| 1500   | 2021         | 8              |
		| 1500   | 2021         | 9              |
		| 1500   | 2021         | 10             |
		| 1500   | 2021         | 11             |
		| 1500   | 2021         | 12             |
	And the earnings history is maintained

Scenario: Price change approved in the year it was requested, above funding band max; recalc earnings (by request)
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 22500
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2021-02-15 |
		| NewTrainingPrice   | 30000      |
		| NewAssessmentPrice | 750        |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
		| 1000   | 2021         | 4              |
		| 1000   | 2021         | 5              |
		| 1000   | 2021         | 6              |
		| 2000   | 2021         | 7              |
		| 2000   | 2021         | 8              |
		| 2000   | 2021         | 9              |
		| 2000   | 2021         | 10             |
		| 2000   | 2021         | 11             |
		| 2000   | 2021         | 12             |
	And the earnings history is maintained

Scenario: Price change following Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 21  |
	And a funding band maximum of 30000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2024-08-01 | 2026-07-31 | 12000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2025-04-01     |
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2024-08-01 |
		| NewTrainingPrice   | 5000       |
		| NewAssessmentPrice | 1000       |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 200    | 2425         | 1              | Regular    |
		| 200    | 2425         | 2              | Regular    |
		| 200    | 2425         | 3              | Regular    |
		| 200    | 2425         | 4              | Regular    |
		| 200    | 2425         | 5              | Regular    |
		| 200    | 2425         | 6              | Regular    |
		| 200    | 2425         | 7              | Regular    |
		| 200    | 2425         | 8              | Regular    |
		| 1200   | 2425         | 9              | Completion |
		| 3200   | 2425         | 9              | Balancing  |
	And the earnings history is maintained