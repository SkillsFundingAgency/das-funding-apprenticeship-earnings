Feature: Approve Learning

Scenario: Short course earnings are approved when LearningApproved event is received
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And the short course earnings profile is not yet approved
	When a LearningApproved event is received for the short course
	Then the short course earnings profile is marked as approved

Scenario: Apprenticeship earnings are approved when LearningApproved event is received
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2025-08-01 | 2027-07-31 |      12000 |
	And the apprenticeship earnings profile is not yet approved
	When a LearningApproved event is received for the apprenticeship
	Then the apprenticeship earnings profile is marked as approved
