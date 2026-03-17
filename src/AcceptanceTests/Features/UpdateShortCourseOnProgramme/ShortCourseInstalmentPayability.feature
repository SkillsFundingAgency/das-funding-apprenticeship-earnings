Feature: Short course instalment payability

Covers the means by which short course instalments become payable or otherwise

Scenario: Unapproved short course - instalments are not payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      | 2000       | ThirtyPercentLearningComplete |
	Then the short course instalment payability is
		| Type                          | IsPayable |
		| ThirtyPercentLearningComplete | false     |
		| LearningComplete              | false     |

Scenario: Approved short course with no milestones - instalments are not payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones |
		| 2021-01-01 | 2021-06-25      | 2000       |            |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key        | Value |
		| Milestones |       |
	Then the short course instalment payability is
		| Type                          | IsPayable |
		| ThirtyPercentLearningComplete | false     |
		| LearningComplete              | false     |

Scenario: Approved short course with 30% milestone - first instalment payable, second not payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones |
		| 2021-01-01 | 2021-06-25      | 2000       |            |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key        | Value                         |
		| Milestones | ThirtyPercentLearningComplete |
	Then the short course instalment payability is
		| Type                          | IsPayable |
		| ThirtyPercentLearningComplete | true      |
		| LearningComplete              | false     |

Scenario: Approved short course with both milestones - all instalments payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones |
		| 2021-01-01 | 2021-06-25      | 2000       |            |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key        | Value                                           |
		| Milestones | ThirtyPercentLearningComplete, LearningComplete |
	Then the short course instalment payability is
		| Type                          | IsPayable |
		| ThirtyPercentLearningComplete | true      |
		| LearningComplete              | true      |
