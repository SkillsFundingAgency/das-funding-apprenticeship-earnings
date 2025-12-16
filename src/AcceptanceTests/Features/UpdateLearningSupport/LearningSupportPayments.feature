Feature: LearningSupportPayments

Validates learning support payments are correctly calculated

Scenario: Add learning support payments
	Given an apprenticeship has been created
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
		| 2021-1-1  | 2021-3-31 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate   |
		| LearningSupport | 150    | 2020-8-31 |
		| LearningSupport | 150    | 2020-9-30 |
		| LearningSupport | 150    | 2021-1-31 |
		| LearningSupport | 150    | 2021-2-28 |
		| LearningSupport | 150    | 2021-3-31 |

Scenario: Learning support payments are not lost on a recalculation
	Given an apprenticeship has been created with the following information
		| Age |
		| 21  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2024-07-31 | 15000 |
	And earnings are calculated
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-31 |
	And care details are saved with
		| CareLeaverEmployerConsentGiven | IsCareLeaver | HasEHCP |
		| true                           | true         | true    |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| LearningSupport   | 150    | 2020-8-31  |
		| LearningSupport   | 150    | 2020-9-30  |
		| LearningSupport   | 150    | 2020-10-31 |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |
		| ProviderIncentive | 500    | 2021-07-31 |
		| EmployerIncentive | 500    | 2021-07-31 |
	And the earnings history is maintained

# where the “Date Applies To” does not reach a census date (i.e. the final day of the month), funding must not be earned for that delivery period
Scenario: Calculate learning support earnings (from the previous month)
	Given the date is now 2020-09-01
	Given an apprenticeship has been created
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate   |
		| LearningSupport | 150    | 2020-8-31 |
		| LearningSupport | 150    | 2020-9-30 |
	And the earnings history is maintained

Scenario: Calculate learning support earnings from the current date/month (additional test scenario)
	Given the date is now 2020-08-01
	Given an apprenticeship has been created
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate   |
		| LearningSupport | 150    | 2020-8-31 |
		| LearningSupport | 150    | 2020-9-30 |
	And the earnings history is maintained

Scenario: Calculate learning support earnings from a future date (additional test scenario)
	Given the date is now 2020-08-01
	Given an apprenticeship has been created
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-9-1  | 2020-11-1 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    |
		| LearningSupport | 150    | 2020-9-30  |
		| LearningSupport | 150    | 2020-10-31 |
	And the earnings history is maintained

Scenario: Calculate learning support earnings when the “Date Applies To” is after the planned end date (additional test scenario)
	Given the date is now 2020-08-01
	Given an apprenticeship has been created
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-9-1  | 2021-6-1 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    |
		| LearningSupport | 150    | 2020-9-30  |
		| LearningSupport | 150    | 2020-10-31 |
		| LearningSupport | 150    | 2020-11-30 |
		| LearningSupport | 150    | 2020-12-31 |
		| LearningSupport | 150    | 2021-1-31  |
		| LearningSupport | 150    | 2021-2-28  |
		| LearningSupport | 150    | 2021-3-31  |
		| LearningSupport | 150    | 2021-4-30  |
		| LearningSupport | 150    | 2021-5-31  |
	And the earnings history is maintained

Scenario: Add learning support payments for the same period twice, should only record one LS payment per period, and on programme payments should not be effected
	Given an apprenticeship has been created
	And Earnings are generated with the correct learning amounts
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
		| 2021-1-1  | 2021-3-31 |
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
		| 2021-1-1  | 2021-3-31 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate   |
		| LearningSupport | 150    | 2020-8-31 |
		| LearningSupport | 150    | 2020-9-30 |
		| LearningSupport | 150    | 2021-1-31 |
		| LearningSupport | 150    | 2021-2-28 |
		| LearningSupport | 150    | 2021-3-31 |
	And Earnings are generated with the correct learning amounts
	And the earnings history is maintained

Scenario: Overlapping learning support payments for the same period should not be duplicated
	Given an apprenticeship has been created
	And Earnings are generated with the correct learning amounts
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2020-10-1 |
		| 2020-8-1  | 2021-3-31 |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    |
		| LearningSupport | 150    | 2020-8-31  |
		| LearningSupport | 150    | 2020-9-30  |
		| LearningSupport | 150    | 2020-10-31 |
		| LearningSupport | 150    | 2020-11-30 |
		| LearningSupport | 150    | 2020-12-31 |
		| LearningSupport | 150    | 2021-1-31  |
		| LearningSupport | 150    | 2021-2-28  |
		| LearningSupport | 150    | 2021-3-31  |
	And Earnings are generated with the correct learning amounts
	And the earnings history is maintained