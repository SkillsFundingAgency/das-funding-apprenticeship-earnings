Feature: Withdraw

Tests withdraw functionality for short courses


Scenario: Learner withdrawn - 30% milestone reached (and subsequently removed)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value      |
		| WithdrawalDate | 2021-03-01 |
		| Milestones     |            |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type |

Scenario: Learner withdrawn - 30% milestone reached (and retained despite the withdrawal)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value                         |
		| WithdrawalDate | 2021-03-01                    |
		| Milestones     | ThirtyPercentLearningComplete |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 7              | ThirtyPercentLearningComplete |

Scenario: Learner withdrawn - milestone(s) not reached 
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones |
		| 2021-01-01 | 2021-06-25      | 2000       |            |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value      |
		| WithdrawalDate | 2021-03-01 |
		| Milestones     |            |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |

Scenario: Learner recorded as “Completed” and subsequently withdrawn
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                                      |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete, LearningComplete |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value                         |
		| WithdrawalDate | 2021-03-01                    |
		| Milestones     | ThirtyPercentLearningComplete |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod | Type                          |
		| 600    | 2021         | 7              | ThirtyPercentLearningComplete |