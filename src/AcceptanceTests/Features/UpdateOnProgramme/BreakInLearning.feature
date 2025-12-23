Feature: BreakInLearning

Acceptance tests related to breaks in learning

Scenario: Training provider records a Break in Learning followed by a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2020-10-15 |
	When the following on-programme request is sent
		| Key              | Value                                                                            |
		| PauseDate        | null                                                                             |
		| BreaksInLearning | StartDate:2020-10-15, EndDate:2021-01-15, PriorPeriodExpectedEndDate: 2021-10-01 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 533.33 | 2021         | 6              |
		| 533.33 | 2021         | 7              |
		| 533.33 | 2021         | 8              |
		| 533.33 | 2021         | 9              |
		| 533.33 | 2021         | 10             |
		| 533.33 | 2021         | 11             |
		| 533.33 | 2021         | 12             |
		| 533.33 | 2122         | 1              |
		| 533.33 | 2122         | 2              |

Scenario: Training provider records a break in learning and the return at the same time
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When the following on-programme request is sent
		| Key              | Value                                                                            |
		| BreaksInLearning | StartDate:2020-10-15, EndDate:2021-01-15, PriorPeriodExpectedEndDate: 2021-10-01 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 533.33 | 2021         | 6              |
		| 533.33 | 2021         | 7              |
		| 533.33 | 2021         | 8              |
		| 533.33 | 2021         | 9              |
		| 533.33 | 2021         | 10             |
		| 533.33 | 2021         | 11             |
		| 533.33 | 2021         | 12             |
		| 533.33 | 2122         | 1              |
		| 533.33 | 2122         | 2              |

Scenario: Training provider corrects a previously recorded return from a break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   | 
		| 2020-8-1  | 2021-10-1 |
	And the following on-programme request is sent
		| Key              | Value                                                                            |
		| BreaksInLearning | StartDate:2020-10-15, EndDate:2021-05-15, PriorPeriodExpectedEndDate: 2021-10-01 |
	When the following on-programme request is sent
		| Key              | Value                                                                            |
		| BreaksInLearning | StartDate:2020-10-15, EndDate:2021-01-15, PriorPeriodExpectedEndDate: 2021-10-01 |
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 533.33 | 2021         | 6              |
		| 533.33 | 2021         | 7              |
		| 533.33 | 2021         | 8              |
		| 533.33 | 2021         | 9              |
		| 533.33 | 2021         | 10             |
		| 533.33 | 2021         | 11             |
		| 533.33 | 2021         | 12             |
		| 533.33 | 2122         | 1              |
		| 533.33 | 2122         | 2              |

Scenario: Training provider removes a previously recorded return from a break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	And the following on-programme request is sent
		| Key              | Value                                                                            |
		| BreaksInLearning | StartDate:2020-10-15, EndDate:2021-06-15, PriorPeriodExpectedEndDate: 2021-10-01 |
	When the following on-programme request is sent
		| Key              | Value |
		| BreaksInLearning |       |
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

Scenario: Training provider records a return from a break in learning with an updated price
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
 		| 2020-08-01 | 2021-10-01 | 7000  |
 	And the apprenticeship commitment is approved
 	And the following learning support payment information is provided
 		| StartDate | EndDate   |
 		| 2020-8-1  | 2021-10-1 |
 	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2020-10-15 |
	When the following on-programme request is sent
		| Key                | Value                                                                            |
		| PriceStartDate     | 2021-05-01                                                                       |
		| PriceEndDate       | 2021-10-01                                                                       |
		| NewTrainingPrice   | 7500                                                                             |
		| NewAssessmentPrice | 500                                                                              |
		| PauseDate          | null                                                                             |
		| BreaksInLearning   | StartDate:2020-10-15, EndDate:2021-01-15, PriorPeriodExpectedEndDate: 2021-10-01 |
    Then On programme earnings are persisted as follows
 		| Amount | AcademicYear | DeliveryPeriod |
 		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 533.33 | 2021         | 6              |
		| 533.33 | 2021         | 7              |
		| 533.33 | 2021         | 8              |
		| 533.33 | 2021         | 9              |
		| 693.33 | 2021         | 10             |
		| 693.33 | 2021         | 11             |
		| 693.33 | 2021         | 12             |
		| 693.33 | 2122         | 1              |
		| 693.33 | 2122         | 2              |

Scenario: Apprenticeship duration is increased after BIL with no price change
    Given an apprenticeship has been created with the following information
        | StartDate   | EndDate     | Price |
        | 2023-10-01  | 2024-09-30  | 15000 |
    And the apprenticeship commitment is approved
    And the following learning support payment information is provided
        | StartDate   | EndDate     |
        | 2023-10-01  | 2024-09-30  |
    And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2024-02-01 |
    When the following on-programme request is sent
		| Key                | Value                                                                            |
		| PriceStartDate     | 2024-05-01                                                                       |
		| PriceEndDate       | 2025-03-31                                                                       |
		| NewTrainingPrice   | 14500                                                                            |
		| NewAssessmentPrice | 500                                                                              |
		| PauseDate          | null                                                                             |
		| BreaksInLearning   | StartDate:2024-02-01, EndDate:2024-04-30, PriorPeriodExpectedEndDate: 2024-09-30 |
    Then On programme earnings are persisted as follows
        | Amount        | AcademicYear | DeliveryPeriod |
        | 1000          | 2324         | 3              |
        | 1000          | 2324         | 4              |
        | 1000          | 2324         | 5              |
        | 1000          | 2324         | 6              |
        | 727.2727273   | 2324         | 10             |
        | 727.2727273   | 2324         | 11             |
        | 727.2727273   | 2324         | 12             |
        | 727.2727273   | 2425         | 1              |
        | 727.2727273   | 2425         | 2              |
        | 727.2727273   | 2425         | 3              |
        | 727.2727273   | 2425         | 4              |
        | 727.2727273   | 2425         | 5              |
        | 727.2727273   | 2425         | 6              |
        | 727.2727273   | 2425         | 7              |
        | 727.2727273   | 2425         | 8              |

Scenario: Apprenticeship duration is increased after BIL with a price increase
    Given an apprenticeship has been created with the following information
        | StartDate   | EndDate     | Price |
        | 2023-10-01  | 2024-09-30  | 15000 |
    And the apprenticeship commitment is approved
    And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2024-02-01 |
    When the following on-programme request is sent
		| Key                | Value                                                                            |
		| PriceStartDate     | 2024-03-01                                                                       |
		| PriceEndDate       | 2025-03-31                                                                       |
		| NewTrainingPrice   | 17500                                                                            |
		| NewAssessmentPrice | 500                                                                              |
		| PauseDate          | null                                                                             |
		| BreaksInLearning   | StartDate:2024-02-01, EndDate:2024-04-30, PriorPeriodExpectedEndDate: 2024-09-30 |
    Then On programme earnings are persisted as follows
        | Amount        | AcademicYear | DeliveryPeriod |
        | 1000          | 2324         | 3              |
        | 1000          | 2324         | 4              |
        | 1000          | 2324         | 5              |
        | 1000          | 2324         | 6              |
        | 945.4545455   | 2324         | 10             |
        | 945.4545455   | 2324         | 11             |
        | 945.4545455   | 2324         | 12             |
        | 945.4545455   | 2425         | 1              |
        | 945.4545455   | 2425         | 2              |
        | 945.4545455   | 2425         | 3              |
        | 945.4545455   | 2425         | 4              |
        | 945.4545455   | 2425         | 5              |
        | 945.4545455   | 2425         | 6              |
        | 945.4545455   | 2425         | 7              |
        | 945.4545455   | 2425         | 8              |

Scenario: End date is pushed back to account for BIL with no price change
    Given an apprenticeship has been created with the following information
        | StartDate   | EndDate     | Price |
        | 2023-10-01  | 2024-09-30  | 15000 |
    And the apprenticeship commitment is approved
    And the following learning support payment information is provided
        | StartDate   | EndDate     |
        | 2023-10-01  | 2024-09-30  |
    And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2024-02-01 |
    When the following on-programme request is sent
		| Key                | Value                                                                            |
		| PriceStartDate     | 2024-05-01                                                                       |
		| PriceEndDate       | 2024-12-31                                                                       |
		| NewTrainingPrice   | 15000                                                                            |
		| NewAssessmentPrice | 0                                                                                |
		| PauseDate          | null                                                                             |
		| BreaksInLearning   | StartDate:2024-02-01, EndDate:2024-04-30, PriorPeriodExpectedEndDate: 2024-09-30 |
    Then On programme earnings are persisted as follows
        | Amount | AcademicYear | DeliveryPeriod |
        | 1000   | 2324         | 3              |
        | 1000   | 2324         | 4              |
        | 1000   | 2324         | 5              |
        | 1000   | 2324         | 6              |
        | 1000   | 2324         | 10             |
        | 1000   | 2324         | 11             |
        | 1000   | 2324         | 12             |
        | 1000   | 2425         | 1              |
        | 1000   | 2425         | 2              |
        | 1000   | 2425         | 3              |
        | 1000   | 2425         | 4              |
        | 1000   | 2425         | 5              |