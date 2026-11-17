Feature: Apprenticeship Opt In

Scenario: Apprenticeship is stored without earnings for a provider that has not opted in
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Ukprn    |
		| 2025-08-01 | 2027-07-31 | 12000      | 99999999 |
	Then the apprenticeship is stored without earnings and flagged as not using earn and pay calc

Scenario: Apprenticeship is stored without earnings when the start date is before the opt in start date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2015-08-01 | 2017-07-31 | 12000      |
	Then the apprenticeship is stored without earnings and flagged as not using earn and pay calc
