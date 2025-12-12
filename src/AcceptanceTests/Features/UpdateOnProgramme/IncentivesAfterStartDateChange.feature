Feature: Start date changes and incentive payments

Tests the effects of startdate changes on incentive payments

Scenario: Apprenticeship start date change results in less than 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2021-08-02 |
	Then a first incentive payment is generated
	And no second incentive payment is generated

Scenario: Apprenticeship start date change results in less than 90 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2022-05-04 |
	Then no first incentive payment is generated
	And no second incentive payment is generated

Scenario: Apprenticeship start date change results in 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2022-05-04 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2021-08-01 |
	Then a first incentive payment is generated
	And a second incentive payment is generated