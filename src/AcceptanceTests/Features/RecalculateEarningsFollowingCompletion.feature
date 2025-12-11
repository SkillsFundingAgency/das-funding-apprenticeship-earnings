Feature: Recalculate earnings following completion

Scenario: Early Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 6000   | 2021         | 7              | Balancing  |
		| 3000   | 2021         | 7              | Completion |
	And the earnings history is maintained

Scenario: Late Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-09-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Regular    |
		| 3000   | 2122         | 2              | Completion |
	And the earnings history is maintained

Scenario: On Time Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-07-31 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Regular    |
		| 3000   | 2021         | 12             | Completion |
	And the earnings history is maintained

Scenario: Existing Completion is updated to be later
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-03-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 5000   | 2021         | 8              | Balancing  |
		| 3000   | 2021         | 8              | Completion |
	And the earnings history is maintained

Scenario: Existing Completion is updated to be earlier
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-01-15 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 7000   | 2021         | 6              | Balancing  |
		| 3000   | 2021         | 6              | Completion |
	And the earnings history is maintained

Scenario: Existing Completion is removed
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-02-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | null |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Regular    |
	And the earnings history is maintained

Scenario: Early Completion within last month
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| CompletionDate | 2021-07-30 |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Balancing  |
		| 3000   | 2021         | 12             | Completion |