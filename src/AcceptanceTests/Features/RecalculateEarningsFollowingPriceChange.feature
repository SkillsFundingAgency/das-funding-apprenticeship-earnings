Feature: Recalculate earnings following price change

Scenario: Price change approved in the year it was requested, below or at funding band max; recalc earnings
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following price change request is sent
		| EffectiveFromDate | ChangeRequestDate | NewTrainingPrice | NewAssessmentPrice |
		| 2021-02-15        | 2021-01-15        | 18000            | 750                |
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

Scenario: Price change approved in the year it was requested, above funding band max; recalc earnings
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 22500              |
	And earnings are calculated
	When the following price change request is sent
		| EffectiveFromDate | ChangeRequestDate | NewTrainingPrice | NewAssessmentPrice |
		| 2021-02-15        | 2021-01-15        | 30000            | 750                |
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