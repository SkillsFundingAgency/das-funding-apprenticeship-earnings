Feature: EnglishAndMathsBreakInLearningIn

Acceptance tests related to breaks in learning in English and Maths

Scenario: Training provider records a break in learning without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following English and maths request is sent
		| Key           | Value      |
		| StartDate     | 2020-8-1   |
		| EndDate       | 2021-10-1  |
		| Course        | Maths1     |
		| LearnAimRef   | Maths1     |
		| Amount        | 1400       |
		| ActualEndDate | 2021-10-01 |
	And the following English and maths request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | 
		| Maths1 | 100    | 2021         | 1              | Regular | 
		| Maths1 | 100    | 2021         | 2              | Regular | 
		| Maths1 | 100    | 2021         | 3              | Regular | 
		| Maths1 | 100    | 2021         | 4              | Regular | 
		| Maths1 | 100    | 2021         | 5              | Regular | 
		| Maths1 | 100    | 2021         | 6              | Regular | 
		| Maths1 | 100    | 2021         | 7              | Regular | 

Scenario: Training provider corrects a previously recorded break in learning (moved later)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    | 2021-05-31 |
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

Scenario: Training provider corrects a previously recorded break in learning (moved earlier)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    | 2021-02-15 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | 
		| Maths1 | 100    | 2021         | 1              | Regular | 
		| Maths1 | 100    | 2021         | 2              | Regular | 
		| Maths1 | 100    | 2021         | 3              | Regular | 
		| Maths1 | 100    | 2021         | 4              | Regular | 
		| Maths1 | 100    | 2021         | 5              | Regular | 
		| Maths1 | 100    | 2021         | 6              | Regular | 

Scenario: Training provider removes a previously recorded break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | ActualEndDate | PauseDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | Maths1      | 1400   | 2021-10-01    |           |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | 
		| Maths1   | 100    | 2021         | 1              | Regular | 
		| Maths1   | 100    | 2021         | 2              | Regular | 
		| Maths1   | 100    | 2021         | 3              | Regular | 
		| Maths1   | 100    | 2021         | 4              | Regular | 
		| Maths1   | 100    | 2021         | 5              | Regular | 
		| Maths1   | 100    | 2021         | 6              | Regular | 
		| Maths1   | 100    | 2021         | 7              | Regular | 
		| Maths1   | 100    | 2021         | 8              | Regular | 
		| Maths1   | 100    | 2021         | 9              | Regular | 
		| Maths1   | 100    | 2021         | 10             | Regular | 
		| Maths1   | 100    | 2021         | 11             | Regular | 
		| Maths1   | 100    | 2021         | 12             | Regular | 
		| Maths1   | 100    | 2122         | 1              | Regular | 
		| Maths1   | 100    | 2122         | 2              | Regular | 

Scenario: Pausing an English course does not effect a maths course
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course   | LearnAimRef | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1   | Maths1      | 1400   | 2021-10-01    |
		| 2020-8-1  | 2021-10-1 | English1 | English1    | Z1400   | 2021-10-01    |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course   | LearnAimRef | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1   | Maths1      | 1400   | 2021-10-01    |            |
		| 2020-8-1  | 2021-10-1 | English1 | English1    | 1400   | 2021-10-01    | 2021-03-15 |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | 
		| Maths1   | 100    | 2021         | 1              | Regular | 
		| Maths1   | 100    | 2021         | 2              | Regular | 
		| Maths1   | 100    | 2021         | 3              | Regular | 
		| Maths1   | 100    | 2021         | 4              | Regular | 
		| Maths1   | 100    | 2021         | 5              | Regular | 
		| Maths1   | 100    | 2021         | 6              | Regular | 
		| Maths1   | 100    | 2021         | 7              | Regular | 
		| Maths1   | 100    | 2021         | 8              | Regular | 
		| Maths1   | 100    | 2021         | 9              | Regular | 
		| Maths1   | 100    | 2021         | 10             | Regular | 
		| Maths1   | 100    | 2021         | 11             | Regular | 
		| Maths1   | 100    | 2021         | 12             | Regular | 
		| Maths1   | 100    | 2122         | 1              | Regular | 
		| Maths1   | 100    | 2122         | 2              | Regular | 
		| English1 | 100    | 2021         | 1              | Regular | 
		| English1 | 100    | 2021         | 2              | Regular | 
		| English1 | 100    | 2021         | 3              | Regular | 
		| English1 | 100    | 2021         | 4              | Regular | 
		| English1 | 100    | 2021         | 5              | Regular | 
		| English1 | 100    | 2021         | 6              | Regular | 
		| English1 | 100    | 2021         | 7              | Regular | 

Scenario: Training provider records a break in learning followed by a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following English and maths request is sent
		| Key           | Value      |
		| StartDate     | 2020-08-1  |
		| EndDate       | 2021-10-1  |
		| Course        | Maths1     |
		| LearnAimRef   | Maths1     |
		| Amount        | 1400       |
		| ActualEndDate | 2021-10-01 |
		| PauseDate     | 2021-03-15 |
	When the following English and maths request is sent
		| Key               | Value                                                                        |
		| PauseDate         | null                                                                         |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2021-03-15, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-06-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 |    100 |         2021 |              1 | Regular |
		| Maths1 |    100 |         2021 |              2 | Regular |
		| Maths1 |    100 |         2021 |              3 | Regular |
		| Maths1 |    100 |         2021 |              4 | Regular |
		| Maths1 |    100 |         2021 |              5 | Regular |
		| Maths1 |    100 |         2021 |              6 | Regular |
		| Maths1 |    100 |         2021 |              7 | Regular |
		| Maths1 |    175 |         2021 |             11 | Regular |
		| Maths1 |    175 |         2021 |             12 | Regular |
		| Maths1 |    175 |         2122 |              1 | Regular |
		| Maths1 |    175 |         2122 |              2 | Regular |


Scenario: Training provider removes a previously recorded return from a break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following English and maths request is sent
		| Key           | Value      |
		| StartDate     | 2020-08-1  |
		| EndDate       | 2021-10-1  |
		| Course        | Maths1     |
		| LearnAimRef   | Maths1     |
		| Amount        | 1400       |
		| ActualEndDate | 2021-10-01 |
		| PauseDate         | null                                                                         |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2021-03-15, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-06-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	When the following English and maths request is sent
		| Key           | Value      |
		| StartDate     | 2020-08-1  |
		| EndDate       | 2021-10-1  |
		| Course        | Maths1     |
		| LearnAimRef   | Maths1     |
		| Amount        | 1400       |
		| ActualEndDate | 2021-10-01 |
		| PauseDate         | null                                                                         |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
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