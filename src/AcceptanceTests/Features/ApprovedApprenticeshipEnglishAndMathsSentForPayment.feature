Feature: Approved apprenticeship English and Maths sent for payment

Scenario: AC1 - English and Maths course added before approval is sent for payment on approval
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And a LearningApproved event is received for the apprenticeship
	Then the payments event is sent to pv2 with the correct information for the apprenticeship
	And the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: AC2 - English and Maths course added after approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And the earnings are released for the apprenticeship
	Then the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: AC3 - English and Maths course details updated after approval are sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And a LearningApproved event is received for the apprenticeship
	When the following English and maths request is sent
		| Key    | Value |
		| Amount | 5000  |
	And the earnings are released for the apprenticeship
	Then the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: A course with no instalments (e.g. withdrawn before the qualifying period) is still sent
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount | WithdrawalDate |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 | 2021-01-05     |
	And the earnings are released for the apprenticeship
	Then the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: English and Maths is not sent when the apprenticeship has no LearnerRef
	Given the learner has no LearnerRef
	And an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And a LearningApproved event is received for the apprenticeship
	Then no payments event is sent to pv2
