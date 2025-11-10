Feature: BreakInLearning

Acceptance tests related to breaks in learning

Scenario: Training provider records a break in learning without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When a pause date of 2021-03-15 is sent
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    | IsAfterLearningEnded |
		| LearningSupport | 150    | 2020-8-31  | false                |
		| LearningSupport | 150    | 2020-9-30  | false                |
		| LearningSupport | 150    | 2020-10-31 | false                |
		| LearningSupport | 150    | 2020-11-30 | false                |
		| LearningSupport | 150    | 2020-12-31 | false                |
		| LearningSupport | 150    | 2021-1-31  | false                |
		| LearningSupport | 150    | 2021-2-28  | false                |
		| LearningSupport | 150    | 2021-3-31  | true                 |
		| LearningSupport | 150    | 2021-4-30  | true                 |
		| LearningSupport | 150    | 2021-5-31  | true                 |
		| LearningSupport | 150    | 2021-6-30  | true                 |
		| LearningSupport | 150    | 2021-7-31  | true                 |
		| LearningSupport | 150    | 2021-8-31  | true                 |
		| LearningSupport | 150    | 2021-9-30  | true                 |

Scenario: Training provider records a break in learning (last day of month) without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When a pause date of 2021-03-31 is sent
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
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    | IsAfterLearningEnded |
		| LearningSupport | 150    | 2020-8-31  | false                |
		| LearningSupport | 150    | 2020-9-30  | false                |
		| LearningSupport | 150    | 2020-10-31 | false                |
		| LearningSupport | 150    | 2020-11-30 | false                |
		| LearningSupport | 150    | 2020-12-31 | false                |
		| LearningSupport | 150    | 2021-1-31  | false                |
		| LearningSupport | 150    | 2021-2-28  | false                |
		| LearningSupport | 150    | 2021-3-31  | false                |
		| LearningSupport | 150    | 2021-4-30  | true                 |
		| LearningSupport | 150    | 2021-5-31  | true                 |
		| LearningSupport | 150    | 2021-6-30  | true                 |
		| LearningSupport | 150    | 2021-7-31  | true                 |
		| LearningSupport | 150    | 2021-8-31  | true                 |
		| LearningSupport | 150    | 2021-9-30  | true                 |

Scenario: Training provider corrects a previously recorded break in learning (moved later)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
    When a pause date of 2021-03-15 is sent
    And a pause date of 2021-05-31 is sent
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
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    | IsAfterLearningEnded |
		| LearningSupport | 150    | 2020-8-31  | false                |
		| LearningSupport | 150    | 2020-9-30  | false                |
		| LearningSupport | 150    | 2020-10-31 | false                |
		| LearningSupport | 150    | 2020-11-30 | false                |
		| LearningSupport | 150    | 2020-12-31 | false                |
		| LearningSupport | 150    | 2021-1-31  | false                |
		| LearningSupport | 150    | 2021-2-28  | false                |
		| LearningSupport | 150    | 2021-3-31  | false                |
		| LearningSupport | 150    | 2021-4-30  | false                |
		| LearningSupport | 150    | 2021-5-31  | false                |
		| LearningSupport | 150    | 2021-6-30  | true                 |
		| LearningSupport | 150    | 2021-7-31  | true                 |
		| LearningSupport | 150    | 2021-8-31  | true                 |
		| LearningSupport | 150    | 2021-9-30  | true                 |

Scenario: Training provider corrects a previously recorded break in learning (moved earlier)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
    When a pause date of 2021-03-15 is sent
    And a pause date of 2021-02-15 is sent
    Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    | IsAfterLearningEnded |
		| LearningSupport | 150    | 2020-8-31  | false                |
		| LearningSupport | 150    | 2020-9-30  | false                |
		| LearningSupport | 150    | 2020-10-31 | false                |
		| LearningSupport | 150    | 2020-11-30 | false                |
		| LearningSupport | 150    | 2020-12-31 | false                |
		| LearningSupport | 150    | 2021-1-31  | false                |
		| LearningSupport | 150    | 2021-2-28  | true                 |
		| LearningSupport | 150    | 2021-3-31  | true                 |
		| LearningSupport | 150    | 2021-4-30  | true                 |
		| LearningSupport | 150    | 2021-5-31  | true                 |
		| LearningSupport | 150    | 2021-6-30  | true                 |
		| LearningSupport | 150    | 2021-7-31  | true                 |
		| LearningSupport | 150    | 2021-8-31  | true                 |
		| LearningSupport | 150    | 2021-9-30  | true                 |

Scenario: Training provider removes a previously recorded break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
    And a pause date of 2021-03-15 is sent
	When a pause is removed
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
	Then Additional Payments are persisted as follows
		| Type            | Amount | DueDate    | IsAfterLearningEnded |
		| LearningSupport | 150    | 2020-8-31  | false                |
		| LearningSupport | 150    | 2020-9-30  | false                |
		| LearningSupport | 150    | 2020-10-31 | false                |
		| LearningSupport | 150    | 2020-11-30 | false                |
		| LearningSupport | 150    | 2020-12-31 | false                |
		| LearningSupport | 150    | 2021-1-31  | false                |
		| LearningSupport | 150    | 2021-2-28  | false                |
		| LearningSupport | 150    | 2021-3-31  | false                |
		| LearningSupport | 150    | 2021-4-30  | false                |
		| LearningSupport | 150    | 2021-5-31  | false                |
		| LearningSupport | 150    | 2021-6-30  | false                |
		| LearningSupport | 150    | 2021-7-31  | false                |
		| LearningSupport | 150    | 2021-8-31  | false                |
		| LearningSupport | 150    | 2021-9-30  | false                |
