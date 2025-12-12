Feature: EnglishAndMathsBreakInLearningIn

Acceptance tests related to breaks in learning in English and Maths

Scenario: Training provider records a break in learning without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | False                |
		| Maths1 | 100    | 2021         | 8              | Regular | True                 |
		| Maths1 | 100    | 2021         | 9              | Regular | True                 |
		| Maths1 | 100    | 2021         | 10             | Regular | True                 |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: Training provider corrects a previously recorded break in learning (moved later)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-05-31 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | False                |
		| Maths1 | 100    | 2021         | 8              | Regular | False                |
		| Maths1 | 100    | 2021         | 9              | Regular | False                |
		| Maths1 | 100    | 2021         | 10             | Regular | False                |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: Training provider corrects a previously recorded break in learning (moved earlier)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-02-15 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | True                |
		| Maths1 | 100    | 2021         | 8              | Regular | True                 |
		| Maths1 | 100    | 2021         | 9              | Regular | True                 |
		| Maths1 | 100    | 2021         | 10             | Regular | True                 |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: Training provider removes a previously recorded break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |           |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1   | 100    | 2021         | 1              | Regular | False                |
		| Maths1   | 100    | 2021         | 2              | Regular | False                |
		| Maths1   | 100    | 2021         | 3              | Regular | False                |
		| Maths1   | 100    | 2021         | 4              | Regular | False                |
		| Maths1   | 100    | 2021         | 5              | Regular | False                |
		| Maths1   | 100    | 2021         | 6              | Regular | False                |
		| Maths1   | 100    | 2021         | 7              | Regular | False                |
		| Maths1   | 100    | 2021         | 8              | Regular | False                |
		| Maths1   | 100    | 2021         | 9              | Regular | False                |
		| Maths1   | 100    | 2021         | 10             | Regular | False                |
		| Maths1   | 100    | 2021         | 11             | Regular | False                |
		| Maths1   | 100    | 2021         | 12             | Regular | False                |
		| Maths1   | 100    | 2122         | 1              | Regular | False                |
		| Maths1   | 100    | 2122         | 2              | Regular | False                |

Scenario: Pausing an English course does not effect a maths course
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course   | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1   | 1400   | 2021-10-01    |
		| 2020-8-1  | 2021-10-1 | English1 | 1400   | 2021-10-01    |
	When the following english and maths course information is provided
		| StartDate | EndDate   | Course   | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1   | 1400   | 2021-10-01    |            |
		| 2020-8-1  | 2021-10-1 | English1 | 1400   | 2021-10-01    | 2021-03-15 |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1   | 100    | 2021         | 1              | Regular | False                |
		| Maths1   | 100    | 2021         | 2              | Regular | False                |
		| Maths1   | 100    | 2021         | 3              | Regular | False                |
		| Maths1   | 100    | 2021         | 4              | Regular | False                |
		| Maths1   | 100    | 2021         | 5              | Regular | False                |
		| Maths1   | 100    | 2021         | 6              | Regular | False                |
		| Maths1   | 100    | 2021         | 7              | Regular | False                |
		| Maths1   | 100    | 2021         | 8              | Regular | False                |
		| Maths1   | 100    | 2021         | 9              | Regular | False                |
		| Maths1   | 100    | 2021         | 10             | Regular | False                |
		| Maths1   | 100    | 2021         | 11             | Regular | False                |
		| Maths1   | 100    | 2021         | 12             | Regular | False                |
		| Maths1   | 100    | 2122         | 1              | Regular | False                |
		| Maths1   | 100    | 2122         | 2              | Regular | False                |
		| English1 | 100    | 2021         | 1              | Regular | False                |
		| English1 | 100    | 2021         | 2              | Regular | False                |
		| English1 | 100    | 2021         | 3              | Regular | False                |
		| English1 | 100    | 2021         | 4              | Regular | False                |
		| English1 | 100    | 2021         | 5              | Regular | False                |
		| English1 | 100    | 2021         | 6              | Regular | False                |
		| English1 | 100    | 2021         | 7              | Regular | False                |
		| English1 | 100    | 2021         | 8              | Regular | True                 |
		| English1 | 100    | 2021         | 9              | Regular | True                 |
		| English1 | 100    | 2021         | 10             | Regular | True                 |
		| English1 | 100    | 2021         | 11             | Regular | True                 |
		| English1 | 100    | 2021         | 12             | Regular | True                 |
		| English1 | 100    | 2122         | 1              | Regular | True                 |
		| English1 | 100    | 2122         | 2              | Regular | True                 |
