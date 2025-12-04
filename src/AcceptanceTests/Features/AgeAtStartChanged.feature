Feature: AgeAtStartChanged

Tests earnings (specifically incentives) are recalculated effectively post a change in the learner's start date and/or date of birth.

Scenario: (change of dob) 90th and 365th day count must be recalculated (always eligible)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price | Age |
		| 2020-08-01 | 2021-10-01 | 7000  | 17  |
	And the apprenticeship commitment is approved
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | false                |
		| EmployerIncentive | 500    | 2021-07-31 | false                |
	When a new date of birth 2003-02-01 is sent
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | false                |
		| EmployerIncentive | 500    | 2021-07-31 | false                |
	And date of birth is updated to 2003-02-01

Scenario: (change of dob) 90th and 365th day count must be recalculated (was eligible, now ineligible)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price | Age |
		| 2020-08-01 | 2021-10-01 | 7000  | 17  |
	And the apprenticeship commitment is approved
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | false                |
		| EmployerIncentive | 500    | 2021-07-31 | false                |
	When a new date of birth 2000-02-01 is sent
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
	And date of birth is updated to 2000-02-01

Scenario: (change of dob) 90th and 365th day count must be recalculated (was ineligible, now eligible)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price | Age |
		| 2020-08-01 | 2021-10-01 | 7000  | 22  |
	And the apprenticeship commitment is approved
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
	When a new date of birth 2003-02-01 is sent
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | false                |
		| EmployerIncentive | 500    | 2021-07-31 | false                |
	And date of birth is updated to 2003-02-01

Scenario: (change of dob) No incentives generated (always ineligible)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price | Age |
		| 2020-08-01 | 2021-10-01 | 7000  | 22  |
	And the apprenticeship commitment is approved
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
	When a new date of birth 1997-02-01 is sent
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
	And date of birth is updated to 1997-02-01


#(change of start date)
#(change of start date and dob)
