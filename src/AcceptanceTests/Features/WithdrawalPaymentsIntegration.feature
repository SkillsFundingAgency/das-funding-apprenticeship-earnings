Feature: WithdrawalPaymentsIntegration

When an approved apprenticeship is withdrawn, has its withdrawal updated or has its withdrawal removed, payments are informed of the updated earnings

Scenario: Withdrawn approved apprenticeship is sent for payment with earnings up to the withdrawal date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 3 learning earnings

Scenario: Withdrawn approved apprenticeship with learning support is sent for payment with learning support up to the withdrawal date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-06-30 |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 3 learning earnings
	And the payments event is sent to pv2 with 3 learning support earnings matching the stored learning support

Scenario: Withdrawn approved apprenticeship for a 16-18 apprentice is sent for payment with only the incentive earnings due before the withdrawal date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 18  |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-05-31 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 5 learning earnings
	And the payments event is sent to pv2 with the correct 16-18 incentive earnings for the apprenticeship
	And the latest payments event is sent to pv2 with no "Second16To18EmployerIncentive" earnings
	And the latest payments event is sent to pv2 with no "Second16To18ProviderIncentive" earnings

Scenario: Updated withdrawal of an approved apprenticeship is sent for payment with earnings up to the updated withdrawal date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-05-31 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 5 learning earnings

Scenario: Updated withdrawal of an approved apprenticeship with learning support is sent for payment with learning support up to the updated withdrawal date
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-02-28 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-02-28 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 2 learning earnings
	And the payments event is sent to pv2 with 2 learning support earnings matching the stored learning support

Scenario: Updated withdrawal of an approved apprenticeship for a 16-18 apprentice to before the first incentive date is sent for payment with no incentive earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 18  |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-05-31 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-02-28 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 2 learning earnings
	And the latest payments event is sent to pv2 with no "First16To18EmployerIncentive" earnings
	And the latest payments event is sent to pv2 with no "First16To18ProviderIncentive" earnings

Scenario: Removed withdrawal of an approved apprenticeship is sent for payment with all earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	When the following on-programme request is sent
		| Key            | Value |
		| WithdrawalDate |       |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Continuing" and 12 learning earnings

Scenario: Removed withdrawal of an approved apprenticeship with learning support is sent for payment with all learning support earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-06-30 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-03-31 |
	When the following on-programme request is sent
		| Key            | Value |
		| WithdrawalDate |       |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2021-01-01 | 2021-06-30 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Continuing" and 12 learning earnings
	And the payments event is sent to pv2 with 6 learning support earnings matching the stored learning support

Scenario: Removed withdrawal of an approved apprenticeship for a 16-18 apprentice is sent for payment with all incentive earnings
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2021-01-01 | 2021-12-31 |      12000 | 18  |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-02-28 |
	When the following on-programme request is sent
		| Key            | Value |
		| WithdrawalDate |       |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Continuing" and 12 learning earnings
	And the payments event is sent to pv2 with the correct 16-18 incentive earnings for the apprenticeship

Scenario: Updated withdrawal date within the same delivery period of an approved apprenticeship is still sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-10 |
	And the earnings are released to payments
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-20 |
	And the earnings are released to payments
	Then the latest payments event is sent to pv2 with a training status of "Withdrawn" and 2 learning earnings
	And 3 payments events are sent to pv2 for the apprenticeship each with a different earnings id

Scenario: Withdrawn apprenticeship which has not been approved is not sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice |
		| 2021-01-01 | 2021-12-31 |      12000 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-03-31 |
	And the earnings are released to payments
	Then no payments event is sent to pv2
