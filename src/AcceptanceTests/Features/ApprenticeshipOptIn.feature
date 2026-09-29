Feature: Apprenticeship Opt In

Scenario: Apprenticeship earnings are not created for a provider that has not opted in
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Ukprn    |
		| 2025-08-01 | 2027-07-31 | 12000      | 99999999 |
	Then no apprenticeship earnings profile is created

Scenario: Apprenticeship earnings are not created when the start date is before the opt in start date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2015-08-01 | 2017-07-31 | 12000      |
	Then no apprenticeship earnings profile is created
