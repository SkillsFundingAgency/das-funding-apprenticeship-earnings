Feature: Approved apprenticeship sent for payment

Covers the sending of correct events to pv2 for approved apprenticeships (FLP-2003, FLP-2030)

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

Scenario: Approved apprenticeship for a 16-18 apprentice sends incentive earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 18  |
	And a LearningApproved event is received for the apprenticeship
	Then the payments event is sent to pv2 with the correct 16-18 incentive earnings for the apprenticeship

Scenario: Approved apprenticeship for a 19-24 disadvantaged apprentice does not send 16-18 incentive types
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 20  |
	And the following on-programme request is sent
		| Key                            | Value |
		| CareLeaverEmployerConsentGiven | false |
		| IsCareLeaver                   | true  |
		| HasEHCP                        | false |
	And a LearningApproved event is received for the apprenticeship
	Then the payments event is sent to pv2 with no 16-18 incentive earnings for the apprenticeship
