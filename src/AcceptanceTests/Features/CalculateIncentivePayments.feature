Feature: Calculate incentive payments

Scenario: Simple Incentive Payments Generation
	Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	| EmployerIncentive | 500    | 2021-07-31 |
	Then an EarningsGeneratedEvent is raised with the following Delivery Periods
	| Type              | Amount | CalendarMonth | CalendarYear |
	| EmployerIncentive | 500    | 10            | 2020         |
	| ProviderIncentive | 500    | 10            | 2020         |
	| EmployerIncentive | 500    | 7             | 2021         |
	| ProviderIncentive | 500    | 7             | 2021         |

