Feature: Start date changes

Scenario: New start date earlier than current date and in the same current academic year
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-10-15 | 2021-08-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2020-09-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
		| 1000   | 2021         | 4              |
		| 1000   | 2021         | 5              |
		| 1000   | 2021         | 6              |
		| 1000   | 2021         | 7              |
		| 1000   | 2021         | 8              |
		| 1000   | 2021         | 9              |
		| 1000   | 2021         | 10             |
		| 1000   | 2021         | 11             |
		| 1000   | 2021         | 12             |
		| 1000   | 2122         | 1              |

Scenario: New start date later than current date and in the same current academic year
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-10-15 | 2021-08-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2021-03-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 2000   | 2021         | 8              |
		| 2000   | 2021         | 9              |
		| 2000   | 2021         | 10             |
		| 2000   | 2021         | 11             |
		| 2000   | 2021         | 12             |
		| 2000   | 2122         | 1              |
	And the earnings history is maintained

Scenario: New start date in the next academic year
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-10-15 | 2022-08-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                | Value      |
		| PriceStartDate     | 2022-03-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 2000   | 2122         | 8              |
		| 2000   | 2122         | 9              |
		| 2000   | 2122         | 10             |
		| 2000   | 2122         | 11             |
		| 2000   | 2122         | 12             |
		| 2000   | 2223         | 1              |
	And the earnings history is maintained

Scenario: A new start and end date are earlier than orginal start date
	Given the following Price Episodes
		| StartDate  | EndDate    | 
		| 2019-01-01 | 2020-09-01 | 
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| PriceStartDate | 2018-11-01 |
		| PriceEndDate   | 2020-07-01 |
	Then there are 20 earnings

Scenario: A new start is earlier than orginal start date but the end date remains the same
	Given the following Price Episodes
		| StartDate  | EndDate    | 
		| 2019-01-01 | 2020-09-01 | 
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| PriceStartDate | 2018-11-01 |
	Then there are 22 earnings

Scenario: A new earlier start and later end date
	Given the following Price Episodes
		| StartDate  | EndDate    | 
		| 2019-01-01 | 2020-09-01 | 
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| PriceStartDate | 2018-11-01 |
		| PriceEndDate   | 2020-11-01 |
	Then there are 24 earnings