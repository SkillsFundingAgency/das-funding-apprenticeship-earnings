Feature: BreakInLearning

Acceptance tests related to breaks in learning

Scenario: (OnProgramme) Training provider records a break in learning without specifying a return
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

Scenario: (OnProgramme) Training provider records a break in learning (last day of month) without specifying a return
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

Scenario: (OnProgramme) Training provider corrects a previously recorded break in learning (moved later)
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

Scenario: (OnProgramme) Training provider corrects a previously recorded break in learning (moved earlier)
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

Scenario: (OnProgramme) Training provider removes a previously recorded break in learning
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

Scenario: (OnProgramme - Break Completed) Training provider records a Break in Learning followed by a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	And a pause date of 2020-10-15 is sent
	When SLD informs us that the break in learning was
		| StartDate  | EndDate    |
		| 2020-10-15 | 2021-01-15 |
	And a pause is removed
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

Scenario: (OnProgramme - Break Completed) Training provider records a break in learning and the return at the same time
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When SLD informs us that the break in learning was
		| StartDate  | EndDate    |
		| 2020-10-15 | 2021-01-15 |
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

Scenario: (OnProgramme - Break Completed) Training provider corrects a previously recorded return from a break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When SLD informs us that the break in learning was
		| StartDate  | EndDate    |
		| 2020-10-15 | 2021-05-15 |
	When SLD informs us that the break in learning was
		| StartDate  | EndDate    |
		| 2020-10-15 | 2021-01-15 |
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

Scenario: (OnProgramme - Break Completed) Training provider removes a previously recorded return from a break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate   |
		| 2020-8-1  | 2021-10-1 |
	When SLD informs us that the break in learning was
		| StartDate  | EndDate    |
		| 2021-03-15 | 2021-06-15 |
	And SLD informs us that the break in learning was
		| StartDate  | EndDate    |
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

Scenario: Incentive Payments 16-18 are pushed back due to Break in Learning
Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And SLD informs us that the break in learning was
	| StartDate  | EndDate    | PriorPeriodExpectedEndDate |
	| 2020-09-01 | 2020-09-07 | 2024-07-31                 |
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-11-05 |
	| EmployerIncentive | 500    | 2020-11-05 |
	| ProviderIncentive | 500    | 2021-08-07 |
	| EmployerIncentive | 500    | 2021-08-07 |

Scenario: Incentive Payments 19-24 are pushed back due to Break in Learning
Given an apprenticeship has been created with the following information
	| Age |
	| 20  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And care details are saved with
	| CareLeaverEmployerConsentGiven | IsCareLeaver | HasEHCP |
	| true                           | true         | true    |
	And SLD informs us that the break in learning was
	| StartDate  | EndDate    | PriorPeriodExpectedEndDate |
	| 2020-09-01 | 2020-09-07 | 2024-07-31                 |
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-11-05 |
	| EmployerIncentive | 500    | 2020-11-05 |
	| ProviderIncentive | 500    | 2021-08-07 |
	| EmployerIncentive | 500    | 2021-08-07 |