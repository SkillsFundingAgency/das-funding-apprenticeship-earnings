Feature: UpdateResponse

Verifies the response from a short course update, this information is used in the outer to build the 
event sent to payments

Scenario: Short course updated with one milestone reached, only one instalment is payable
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | Milestones                    |
		| 2021-01-01 | 2021-06-25      | 2000       |  |
	And a LearningApproved event is received for the short course
	When Short Course Update OnProgramme is triggered with
		| Key            | Value                         |
		| Milestones     | ThirtyPercentLearningComplete |
	Then the following data is returned from the put request
		| Amount | CollectionYear | CollectionPeriod | Type                          | IsPayable |
		| 600    | 2021           | 7                | ThirtyPercentLearningComplete | True      |
		| 1400   | 2021           | 11               | LearningComplete              | False     |

