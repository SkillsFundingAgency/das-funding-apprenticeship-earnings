Feature: Approved short course sent for payment

Covers the sending of correct events to pv2 for approved short courses

Scenario: Approved short course with both milestones - all instalments payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                                      |
		| 2021-01-01 | 2021-06-25      |       2000 | ThirtyPercentLearningComplete, LearningComplete |
	And a LearningApproved event is received for the short course
	Then the payments event is sent to pv2 with the correct information
