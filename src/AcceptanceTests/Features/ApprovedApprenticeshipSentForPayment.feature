Feature: Approved apprenticeship sent for payment

Covers the sending of correct events to pv2 for approved apprenticeships (FLP-2003)

Scenario: Approved apprenticeship with on-programme, completion and balancing earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | CompletionDate | AchievementDate |
		| 2021-01-01 | 2021-12-31 |      12000 | 2021-12-15     | 2021-12-01      |
	And a LearningApproved event is received for the apprenticeship
	Then the payments event is sent to pv2 with the correct information for the apprenticeship

Scenario: Manually-added apprenticeship with no LearnerRef is not sent for payment
	Given the learner has no LearnerRef
	And an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	Then no payments event is sent to pv2
