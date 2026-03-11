Feature: Withdraw

Tests withdraw functionality for short courses


Scenario: Learner withdrawn - 30% milestone reached (and subsequently removed)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given that an apprenticeship unit has been approved by an employer
	And the training provider recorded that the 30% milestone has been reached
	When SLD inform us that the learner has withdrawn
	And SLD also inform us that the 30% milestone was removed
	Then remove all earnings for that apprenticeship unit

Scenario: Learner withdrawn - 30% milestone reached (and retained despite the withdrawal)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given that an apprenticeship unit has been approved by an employer
	And the training provider recorded that the 30% milestone has been reached
	When SLD inform us that the learner has withdrawn
	And SLD also inform us that the 30% milestone was not removed
	Then remove the remaining completion earning
	And retain the 30% milestone earning

Scenario: Learner withdrawn - milestone(s) not reached 
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given that an apprenticeship unit has been approved by an employer
	And the training provider did not record that the 30% milestone was reached
	When SLD inform us that the learner has withdrawn
	Then remove all earnings for that apprenticeship unit

Scenario: Learner recorded as “Completed” and subsequently withdrawn
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given that an apprenticeship unit has been approved by an employer
	And the training provider recorded that the 30% milestone has been reached
	And the training provider also recorded that the learner completed
	When SLD inform us that the learner has withdrawn
	Then remove the completion earning
	And either retain or remove the 30% milestone earning based on whether the 30% milestone is still recorded (AC1 / AC2)

Scenario: Learner removed - remove all earnings
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given that an apprenticeship unit has been approved by an employer
	When SLD inform us that the learner record has been removed
	Then remove all earnings for that apprenticeship unit