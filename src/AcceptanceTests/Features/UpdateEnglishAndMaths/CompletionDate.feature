Feature: EnglishAndMathsCompletionDate

Validates English and Maths payments are correctly calculated when the completion date changes

Scenario: English and Maths completion moved later
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-12-01    |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 100    | 2021         | 3              | Regular   |
		| Maths1 | 100    | 2021         | 4              | Regular   |
		| Maths1 | 1100   | 2021         | 5              | Balancing |
	And the earnings history is maintained

Scenario: English and Maths completion moved earlier
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-11-01    |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-10-01    |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 1300   | 2021         | 3              | Balancing |
	And the earnings history is maintained

Scenario: English and Maths completion removed
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 100    | 2021         | 1              | Regular |
		| Maths1 | 100    | 2021         | 2              | Regular |
		| Maths1 | 100    | 2021         | 3              | Regular |
		| Maths1 | 100    | 2021         | 4              | Regular |
		| Maths1 | 100    | 2021         | 5              | Regular |
		| Maths1 | 100    | 2021         | 6              | Regular |
		| Maths1 | 100    | 2021         | 7              | Regular |
		| Maths1 | 100    | 2021         | 8              | Regular |
		| Maths1 | 100    | 2021         | 9              | Regular |
		| Maths1 | 100    | 2021         | 10             | Regular |
		| Maths1 | 100    | 2021         | 11             | Regular |
		| Maths1 | 100    | 2021         | 12             | Regular |
		| Maths1 | 100    | 2122         | 1              | Regular |
		| Maths1 | 100    | 2122         | 2              | Regular |
		| Maths1 | 100    | 2122         | 3              | Regular |
	And the earnings history is maintained

Scenario: English and Maths early completion within last month
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate    | Course | LearnAimRef | Amount |
		| 2020-8-1  | 2021-10-31 | Maths1 | Maths1      | 1500   |
	When the following english and maths completion change request is sent
		| StartDate | EndDate    | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-31 | Maths1 | Maths1      | 1500   | 2021-10-01    |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 100    | 2021         | 3              | Regular   |
		| Maths1 | 100    | 2021         | 4              | Regular   |
		| Maths1 | 100    | 2021         | 5              | Regular   |
		| Maths1 | 100    | 2021         | 6              | Regular   |
		| Maths1 | 100    | 2021         | 7              | Regular   |
		| Maths1 | 100    | 2021         | 8              | Regular   |
		| Maths1 | 100    | 2021         | 9              | Regular   |
		| Maths1 | 100    | 2021         | 10             | Regular   |
		| Maths1 | 100    | 2021         | 11             | Regular   |
		| Maths1 | 100    | 2021         | 12             | Regular   |
		| Maths1 | 100    | 2122         | 1              | Regular   |
		| Maths1 | 100    | 2122         | 2              | Regular   |
		| Maths1 | 100    | 2122         | 3              | Balancing |
	And the earnings history is maintained