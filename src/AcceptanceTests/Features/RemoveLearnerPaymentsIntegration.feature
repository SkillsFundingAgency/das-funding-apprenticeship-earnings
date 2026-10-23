Feature: RemoveLearnerPaymentsIntegration

When an approved apprenticeship is removed from the ILR, payments are informed that all of its earnings have been removed

Scenario: Removed approved apprenticeship is sent for payment with no earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the learner is removed
	And the earnings are released to payments
	Then the payments event is sent to pv2 with no earnings for the apprenticeship

Scenario: Removed approved apprenticeship with english and maths is sent for payment with no english and maths earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course    | LearnAimRef | Amount |
		| 2021-01-01 | 2021-12-31 | Maths 4   | 4A          |   4000 |
		| 2021-01-01 | 2021-12-31 | English 2 | 5B          |   3000 |
	And a LearningApproved event is received for the apprenticeship
	And payments had previously been sent earnings for each english and maths course
	When the learner is removed
	And the earnings are released to payments
	Then the payments event is sent to pv2 with no earnings for the apprenticeship
	And the english and maths payments event is sent to pv2 with no earnings

Scenario: Removed approved apprenticeship with learning support is sent for payment with no learning support earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	And payments had previously been sent "LearningSupport" earnings for the apprenticeship
	When the learner is removed
	And the earnings are released to payments
	Then the payments event is sent to pv2 with no earnings for the apprenticeship

Scenario: Removed approved apprenticeship with english and maths learning support is sent for payment with no learning support earnings
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
	And payments had previously been sent "LearningSupport" earnings for each english and maths course
	When the learner is removed
	And the earnings are released to payments
	Then the english and maths payments event is sent to pv2 with no earnings

Scenario: Removed approved apprenticeship for a 16-18 apprentice is sent for payment with no incentive earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 18  |
	And a LearningApproved event is received for the apprenticeship
	And payments had previously been sent "First16To18EmployerIncentive" earnings for the apprenticeship
	And payments had previously been sent "First16To18ProviderIncentive" earnings for the apprenticeship
	When the learner is removed
	And the earnings are released to payments
	Then the payments event is sent to pv2 with no earnings for the apprenticeship

Scenario: Removed apprenticeship which has not been approved is not sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	When the learner is removed
	And the earnings are released to payments
	Then no payments event is sent to pv2

Scenario: Removed manually-added apprenticeship with no LearnerRef is not sent for payment
	Given the learner has no LearnerRef
	And an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the learner is removed
	And the earnings are released to payments
	Then no payments event is sent to pv2
