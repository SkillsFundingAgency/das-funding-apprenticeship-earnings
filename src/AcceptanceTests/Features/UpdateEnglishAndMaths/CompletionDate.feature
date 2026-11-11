Feature: EnglishAndMathsCompletionDate

Validates English and Maths payments are correctly calculated when the completion date changes

Scenario: English and Maths completion moved later
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1      | 1500   | 2020-11-01     |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-12-01     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 100    | 2021         | 3              | Regular   |
		| Maths1 | 100    | 2021         | 4              | Regular   |
		| Maths1 | 1100   | 2021         | 5              | Balancing |
	And the english and maths earnings history is maintained

Scenario: English and Maths completion moved earlier
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-11-01     |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-10-01     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 1300   | 2021         | 3              | Balancing |
	And the english and maths earnings history is maintained

Scenario: English and Maths completion removed
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1      | 1500   | 2020-11-01     |
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
	And the english and maths earnings history is maintained

Scenario: English and Maths early completion within last month
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate    | Course | LearnAimRef | Amount |
		| 2020-8-1  | 2021-10-31 | Maths1 | Maths1      | 1500   |
	When the following english and maths completion change request is sent
		| StartDate | EndDate    | Course | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-10-31 | Maths1 | Maths1      | 1500   | 2021-10-01     |
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
	And the english and maths earnings history is maintained
Scenario Outline: English and Maths completed before the end of the qualifying period earns nothing
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2028-07-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate   | EndDate    | Course | LearnAimRef | Amount |
		| <StartDate> | 2027-05-05 | Maths1 | Maths1      | 400    |
	When the following english and maths completion change request is sent
		| StartDate   | EndDate    | Course | LearnAimRef | Amount | CompletionDate   |
		| <StartDate> | 2027-05-05 | Maths1 | Maths1      | 400    | <CompletionDate> |
	Then no english and maths earnings are persisted

Examples:
	| Description                    | StartDate  | CompletionDate |
	| Completes on a census date     | 2027-01-19 | 2027-01-31     |
	| Completes after a census date  | 2027-01-20 | 2027-02-01     |
	| Completes before a census date | 2027-01-01 | 2027-01-13     |

Scenario: English and Maths completed on a census date after the qualifying period earns everything in that month
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2028-07-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course | LearnAimRef | Amount |
		| 2027-01-17 | 2027-05-05 | Maths1 | Maths1      | 400    |
	When the following english and maths completion change request is sent
		| StartDate  | EndDate    | Course | LearnAimRef | Amount | CompletionDate |
		| 2027-01-17 | 2027-05-05 | Maths1 | Maths1      | 400    | 2027-01-31     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2627         | 6              | Regular   |
		| Maths1 | 300    | 2627         | 6              | Balancing |

Scenario: English and Maths completed after a census date after the qualifying period
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2028-07-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course | LearnAimRef | Amount |
		| 2027-01-18 | 2027-05-05 | Maths1 | Maths1      | 400    |
	When the following english and maths completion change request is sent
		| StartDate  | EndDate    | Course | LearnAimRef | Amount | CompletionDate |
		| 2027-01-18 | 2027-05-05 | Maths1 | Maths1      | 400    | 2027-02-01     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2627         | 6              | Regular   |
		| Maths1 | 300    | 2627         | 7              | Balancing |

Scenario: English and Maths completed before a census date after the qualifying period earns everything in that month
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2028-07-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course | LearnAimRef | Amount |
		| 2027-01-01 | 2027-05-05 | Maths1 | Maths1      | 400    |
	When the following english and maths completion change request is sent
		| StartDate  | EndDate    | Course | LearnAimRef | Amount | CompletionDate |
		| 2027-01-01 | 2027-05-05 | Maths1 | Maths1      | 400    | 2027-01-15     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 400    | 2627         | 6              | Balancing |
