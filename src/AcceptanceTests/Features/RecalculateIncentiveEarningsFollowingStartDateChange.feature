Feature: Recalculate incentive earnings following date or duration change

Scenario: Apprenticeship start date change results in less than 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-01 | 2022-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following start date change request is sent
		| NewStartDate | ApprovedDate |
		| 2021-08-02   | 2021-09-01   |
	Then a first incentive payment is generated
	And no second incentive payment is generated

Scenario: Apprenticeship start date change results in less than 90 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-01 | 2022-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following start date change request is sent
		| NewStartDate | ApprovedDate |
		| 2022-05-04   | 2021-09-01   |
	Then no first incentive payment is generated
	And no second incentive payment is generated

Scenario: Apprenticeship start date change results in 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2022-05-04 | 2022-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following start date change request is sent
		| NewStartDate | ApprovedDate |
		| 2021-08-01   | 2021-09-01   |
	Then a first incentive payment is generated
	And a second incentive payment is generated