Feature: RemoveLearner

Remove Learner

Scenario: Remove learner by doing a withdrawal back to start date; remove all incentive payments
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the learner is removed
	Then no Additional Payments are persisted
	And the earnings history is maintained
