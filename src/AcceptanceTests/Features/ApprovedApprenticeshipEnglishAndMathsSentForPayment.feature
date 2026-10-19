Feature: Approved apprenticeship English and Maths sent for payment

English and Maths earnings are sent to payments once the apprenticeship has been approved by the employer.
Each English and Maths course is sent with its own earnings version, and after approval only the courses
that have changed are released.

Scenario: AC1 - English and Maths course added before approval is sent for payment on approval
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	When a LearningApproved event is received for the apprenticeship
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
	And the english and maths earnings are released for the changed courses
	Then only the english and maths course "4A" is sent to pv2 in the release
	And the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: AC3 - English and Maths course details updated after approval are sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course    | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4   | 4A          |   4000 |
		| 2021-01-01 | 2021-12-31 | English 2 | 5B          |   3000 |
	And a LearningApproved event is received for the apprenticeship
	When the following english and maths course information is provided
		| StartDate  | EndDate    | Course    | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4   | 4A          |   5000 |
		| 2021-01-01 | 2021-12-31 | English 2 | 5B          |   3000 |
	And the english and maths earnings are released for the changed courses
	Then only the english and maths course "4A" is sent to pv2 in the release
	And the english and maths course "4A" is sent to pv2 with a new earnings version
	And the english and maths course "5B" is sent to pv2 with its previous earnings version
	And the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: English and Maths course completed early is sent with instalment details based on regular instalments only
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount | CompletionDate |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 | 2021-06-15     |
	And the english and maths earnings are released for the changed courses
	Then only the english and maths course "4A" is sent to pv2 in the release
	And the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: English and Maths is not sent when only on-programme earnings are released
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And a LearningApproved event is received for the apprenticeship
	When the earnings are released with a release type of OnProgramme
	Then no english and maths payments event is sent to pv2 in the release

Scenario: A course with no instalments (e.g. withdrawn before the qualifying period) is still sent
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount | WithdrawalDate |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 | 2021-01-05     |
	And the english and maths earnings are released for the changed courses
	Then only the english and maths course "4A" is sent to pv2 in the release
	And the payments event is sent to pv2 for the english and maths course with the correct information

Scenario: English and Maths is not sent when the apprenticeship has no LearnerRef
	Given the learner has no LearnerRef
	And an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	When a LearningApproved event is received for the apprenticeship
	Then no payments event is sent to pv2
