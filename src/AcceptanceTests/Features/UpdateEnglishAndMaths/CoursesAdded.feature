Feature: EnglishAndMathsPayments

Validates english and maths payments are correctly calculated

Scenario: english and maths earnings for a brand new course
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
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

Scenario: english and maths earnings past the end of the apprenticeship
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate    | Course      | LearnAimRef | Amount |
		| 2021-1-1  | 2021-12-31 | LateEnglish | 12345678    | 1200   |
	Then english and maths instalments are persisted as follows
		| Course      | Amount | AcademicYear | DeliveryPeriod | Type    |
		| LateEnglish | 100    | 2021         | 6              | Regular |
		| LateEnglish | 100    | 2021         | 7              | Regular |
		| LateEnglish | 100    | 2021         | 8              | Regular |
		| LateEnglish | 100    | 2021         | 9              | Regular |
		| LateEnglish | 100    | 2021         | 10             | Regular |
		| LateEnglish | 100    | 2021         | 11             | Regular |
		| LateEnglish | 100    | 2021         | 12             | Regular |
		| LateEnglish | 100    | 2122         | 1              | Regular |
		| LateEnglish | 100    | 2122         | 2              | Regular |
		| LateEnglish | 100    | 2122         | 3              | Regular |
		| LateEnglish | 100    | 2122         | 4              | Regular |
		| LateEnglish | 100    | 2122         | 5              | Regular |
	And the earnings history is maintained
	
Scenario: english and maths before the start of the apprenticeship
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course     | LearnAimRef | Amount |
		| 2020-1-1  | 2021-1-15 | EarlyMaths | 12345678    | 600    |
	Then english and maths instalments are persisted as follows
		| Course     | Amount | AcademicYear | DeliveryPeriod | Type    |
		| EarlyMaths | 50     | 1920         | 6              | Regular |
		| EarlyMaths | 50     | 1920         | 7              | Regular |
		| EarlyMaths | 50     | 1920         | 8              | Regular |
		| EarlyMaths | 50     | 1920         | 9              | Regular |
		| EarlyMaths | 50     | 1920         | 10             | Regular |
		| EarlyMaths | 50     | 1920         | 11             | Regular |
		| EarlyMaths | 50     | 1920         | 12             | Regular |
		| EarlyMaths | 50     | 2021         | 1              | Regular |
		| EarlyMaths | 50     | 2021         | 2              | Regular |
		| EarlyMaths | 50     | 2021         | 3              | Regular |
		| EarlyMaths | 50     | 2021         | 4              | Regular |
		| EarlyMaths | 50     | 2021         | 5              | Regular |
	And the earnings history is maintained

Scenario: english and maths earnings for a course which does not span a census date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course | LearnAimRef | Amount |
		| 2021-02-01 | 2021-02-26 | Maths1 | Maths1      | 900    |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 900    | 2021         | 7              | Regular |
	And the earnings history is maintained
		
Scenario: english and maths completion earnings
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   |
	When the following english and maths completion change request is sent
		| StartDate | EndDate   | Course | LearnAimRef | Amount | CompletionDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-11-01     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type      |
		| Maths1 | 100    | 2021         | 1              | Regular   |
		| Maths1 | 100    | 2021         | 2              | Regular   |
		| Maths1 | 100    | 2021         | 3              | Regular   |
		| Maths1 | 1200   | 2021         | 4              | Balancing |
	And the earnings history is maintained

Scenario: english and maths earnings for a course with prior learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | PriorLearningAdjustmentPercentage |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 63                                |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 63     | 2021         | 1              | Regular |
		| Maths1 | 63     | 2021         | 2              | Regular |
		| Maths1 | 63     | 2021         | 3              | Regular |
		| Maths1 | 63     | 2021         | 4              | Regular |
		| Maths1 | 63     | 2021         | 5              | Regular |
		| Maths1 | 63     | 2021         | 6              | Regular |
		| Maths1 | 63     | 2021         | 7              | Regular |
		| Maths1 | 63     | 2021         | 8              | Regular |
		| Maths1 | 63     | 2021         | 9              | Regular |
		| Maths1 | 63     | 2021         | 10             | Regular |
		| Maths1 | 63     | 2021         | 11             | Regular |
		| Maths1 | 63     | 2021         | 12             | Regular |
		| Maths1 | 63     | 2122         | 1              | Regular |
		| Maths1 | 63     | 2122         | 2              | Regular |
		| Maths1 | 63     | 2122         | 3              | Regular |
	And the earnings history is maintained