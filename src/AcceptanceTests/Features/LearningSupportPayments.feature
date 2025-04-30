Feature: LearningSupportPayments

A short summary of the feature

@tag1

Scenario: Basic template DO NOT APPROVE PR IF THIS IS HERE
	Given An apprenticeship has been created
	And the following learning support payment information is provided
		| AcademicYear | DeliveryPeriod |
		| 2021         | 1              |
		| 2021         | 2              |
		| 2021         | 3              |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate   |
		| LearningSupport | 150    | 2020-8-1  |
		| LearningSupport | 150    | 2020-9-1  |
		| LearningSupport | 150    | 2020-10-1 |




Scenario: Calculate learning support earnings
	Given that the Learning Support Funding (LSF) FAM type is recorded against the on-programme learning aim in the ILR
	And the "Date Applies From" is in a previous month
	When the employer approves the apprenticeship record
	Then calculate learning support earnings of £150 per month for each delivery period within the dates provided (government funded)
	And "forecast" the earnings across the number of calendar months between and including the "Date Applies From" and "Date Applies To"
	And where the “Date Applies To” does not reach a census date (i.e. the final day of the month), funding must not be earned for that delivery period
	And the maximum earning for learning support for a single delivery period must be £150

Scenario: Calculate learning support earnings from the current date/month (additional test scenario)
	Given that the Learning Support Funding (LSF) FAM type is recorded against the on-programme learning aim in the ILR
	And the "Date Applies From" is in the current month
	When the employer approves the apprenticeship record
	Then calculate learning support earnings of £150 per month for each delivery period within the dates provided (government funded)
	And "forecast" the earnings across the number of calendar months between and including the "Date Applies From" and "Date Applies To"
	And where the “Date Applies To” does not reach a census date (i.e. the final day of the month), funding must not be earned for that delivery period
	And the maximum earning for learning support for a single delivery period must be £150

Scenario: Calculate learning support earnings from a future date (additional test scenario)
	Given that the Learning Support Funding (LSF) FAM type is recorded against the on-programme learning aim in the ILR
	And the "Date Applies From" is in a future month
	When the employer approves the apprenticeship record
	Then calculate learning support earnings of £150 per month for each delivery period within the dates provided (government funded)
	And "forecast" the earnings across the number of calendar months between and including the "Date Applies From" and "Date Applies To"
	And where the “Date Applies To” does not reach a census date (i.e. the final day of the month), funding must not be earned for that delivery period
	And the maximum earning for learning support for a single delivery period must be £150

Scenario: Calculate learning support earnings when the “Date Applies To” is after the planned end date (additional test scenario)
	Given that the Learning Support Funding (LSF) FAM type is recorded against the on-programme learning aim in the ILR
	And the "Date Applies To" is after the planned training end date
	When the employer approves the apprenticeship record
	Then calculate learning support earnings of £150 per month for each delivery period after the planned training end date (government funded)
	And where the “Date Applies To” does not reach a census date (i.e. the final day of the month), funding must not be earned for that delivery period
	And the maximum earning for learning support for a single delivery period must be £150