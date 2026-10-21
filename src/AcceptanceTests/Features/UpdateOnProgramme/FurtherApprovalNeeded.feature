Feature: Further approval needed

Payments are withheld while a change to an apprenticeship waits for further approval
(e.g. a start date change pending employer approval). The withhold is published by Earnings
and sent to Payments; the send is covered by the WithholdPaymentsForLearningEventHandler unit tests.

Scenario: Further approval becomes required
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key                     | Value |
		| FurtherApprovalRequired | true  |
	Then the episode is stored as needing further approval
	And 1 withhold payments event is published
	And the withhold payments event is published with the correct information for the apprenticeship

Scenario: Further approval is still required
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key                     | Value |
		| FurtherApprovalRequired | true  |
	When the following on-programme request is sent
		| Key                     | Value |
		| FurtherApprovalRequired | true  |
	Then the episode is stored as needing further approval
	And 1 withhold payments event is published

Scenario: Further approval is no longer required
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key                     | Value |
		| FurtherApprovalRequired | true  |
	When the following on-programme request is sent
		| Key                     | Value |
		| FurtherApprovalRequired | false |
	Then the episode is stored as not needing further approval
	And 1 withhold payments event is published
