Feature: BreakInLearningPauseStarted

Acceptance tests related to breaks in learning that have started but not yet ended.

Scenario: Training provider records a break in learning without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |

Scenario: Training provider records a break in learning (last day of month) without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-31 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |
		| 400    | 2021         | 8              |

Scenario: Training provider corrects a previously recorded break in learning (moved later)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-05-31 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |
		| 400    | 2021         | 8              |
		| 400    | 2021         | 9              |
		| 400    | 2021         | 10             |

Scenario: Training provider corrects a previously recorded break in learning (moved earlier)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-02-15 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |

Scenario: Training provider removes a previously recorded break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	When the following on-programme request is sent
		| Key       | Value |
		| PauseDate | null  |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |
		| 400    | 2021         | 8              |
		| 400    | 2021         | 9              |
		| 400    | 2021         | 10             |
		| 400    | 2021         | 11             |
		| 400    | 2021         | 12             |
		| 400    | 2122         | 1              |
		| 400    | 2122         | 2              |