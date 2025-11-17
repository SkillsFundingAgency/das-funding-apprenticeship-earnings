Feature: AdditionalPaymentsAfterCompletion

Validates that additional payments post completion are not retained


#Scenario: No learning support after completion
#	Note the logic for this scenario is actually covered by the outer api which  
#	will explicitly update learning support payments

Scenario: No 90 days 16-18 (19-24) incentive payments if completed before qualifying date
	Given an apprenticeship has been created with the following information
		| Age | StartDate  | EndDate    |
		| 18  | 2020-08-01 | 2021-10-01 |
	And the apprenticeship commitment is approved
	When the following completion is sent
		| CompletionDate |
		| 2020-10-01     |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | true                 |
		| EmployerIncentive | 500    | 2020-10-29 | true                 |
		| ProviderIncentive | 500    | 2021-07-31 | true                 |
		| EmployerIncentive | 500    | 2021-07-31 | true                 |

Scenario: No 365 days 16-18 (19-24) incentive payments if completed before qualifying date
	Given an apprenticeship has been created with the following information
		| Age | StartDate  | EndDate    |
		| 18  | 2020-08-01 | 2021-10-01 |
	And the apprenticeship commitment is approved
	When the following completion is sent
		| CompletionDate |
		| 2021-06-01     |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    | IsAfterLearningEnded |
		| ProviderIncentive | 500    | 2020-10-29 | false                |
		| EmployerIncentive | 500    | 2020-10-29 | false                |
		| ProviderIncentive | 500    | 2021-07-31 | true                 |
		| EmployerIncentive | 500    | 2021-07-31 | true                 |