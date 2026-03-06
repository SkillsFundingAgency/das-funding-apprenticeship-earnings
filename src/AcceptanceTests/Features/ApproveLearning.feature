Feature: Approve Learning

Scenario: Short course earnings are approved when LearningApproved event is received
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And the short course earnings profile is not yet approved
	When a LearningApproved event is received for the short course
	Then the short course earnings profile is marked as approved
