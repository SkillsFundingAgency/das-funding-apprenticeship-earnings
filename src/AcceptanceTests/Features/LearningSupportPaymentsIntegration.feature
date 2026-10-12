Feature: LearningSupportPaymentsIntegration

Learning support earnings are sent to payments once the apprenticeship has been approved by the employer

Scenario: Learning support added before employer approval is sent for payment on approval
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	Then the payments event is sent to pv2 with 3 learning support earnings matching the stored learning support

Scenario: Learning support added after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with 3 learning support earnings matching the stored learning support

Scenario: Updated learning support after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	When the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-02-01 | 2021-02-28 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with 1 learning support earnings matching the stored learning support

Scenario: Removed learning support after employer approval is sent for payment as no learning support earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	When the following learning support payment information is provided
		| StartDate | EndDate |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with no learning support earnings

Scenario: English and maths learning support added before employer approval is sent for payment on approval
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	Then the english and maths payments event is sent to pv2 with 3 learning support earnings matching the stored learning support

Scenario: English and maths learning support added after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And a LearningApproved event is received for the apprenticeship
	When the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And the earnings are released to payments
	Then the english and maths payments event is sent to pv2 with 3 learning support earnings matching the stored learning support

Scenario: Updated english and maths learning support after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	When the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2021-02-01 | 2021-02-28 |
	And the earnings are released to payments
	Then the english and maths payments event is sent to pv2 with 1 learning support earnings matching the stored learning support

Scenario: Removed english and maths learning support after employer approval is sent for payment as no learning support earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course  | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4 | 4A          |   4000 |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	When the english and maths course has the following learning support
		| StartDate | EndDate |
	And the earnings are released to payments
	Then the english and maths payments event is sent to pv2 with 0 learning support earnings matching the stored learning support
