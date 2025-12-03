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

Scenario: (MathsAndEnglish) Training provider records a break in learning without specifying a return
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	When the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | False                |
		| Maths1 | 100    | 2021         | 8              | Regular | True                 |
		| Maths1 | 100    | 2021         | 9              | Regular | True                 |
		| Maths1 | 100    | 2021         | 10             | Regular | True                 |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: (MathsAndEnglish) Training provider corrects a previously recorded break in learning (moved later)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-05-31 |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | False                |
		| Maths1 | 100    | 2021         | 8              | Regular | False                |
		| Maths1 | 100    | 2021         | 9              | Regular | False                |
		| Maths1 | 100    | 2021         | 10             | Regular | False                |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: (MathsAndEnglish) Training provider corrects a previously recorded break in learning (moved earlier)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-02-15 |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1 | 100    | 2021         | 1              | Regular | False                |
		| Maths1 | 100    | 2021         | 2              | Regular | False                |
		| Maths1 | 100    | 2021         | 3              | Regular | False                |
		| Maths1 | 100    | 2021         | 4              | Regular | False                |
		| Maths1 | 100    | 2021         | 5              | Regular | False                |
		| Maths1 | 100    | 2021         | 6              | Regular | False                |
		| Maths1 | 100    | 2021         | 7              | Regular | True                |
		| Maths1 | 100    | 2021         | 8              | Regular | True                 |
		| Maths1 | 100    | 2021         | 9              | Regular | True                 |
		| Maths1 | 100    | 2021         | 10             | Regular | True                 |
		| Maths1 | 100    | 2021         | 11             | Regular | True                 |
		| Maths1 | 100    | 2021         | 12             | Regular | True                 |
		| Maths1 | 100    | 2122         | 1              | Regular | True                 |
		| Maths1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: (MathsAndEnglish) Training provider removes a previously recorded break in learning
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    | 2021-03-15 |
	When the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate | PauseDate |
		| 2020-8-1  | 2021-10-1 | Maths1 | 1400   | 2021-10-01    |           |
	Then Maths and english instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1   | 100    | 2021         | 1              | Regular | False                |
		| Maths1   | 100    | 2021         | 2              | Regular | False                |
		| Maths1   | 100    | 2021         | 3              | Regular | False                |
		| Maths1   | 100    | 2021         | 4              | Regular | False                |
		| Maths1   | 100    | 2021         | 5              | Regular | False                |
		| Maths1   | 100    | 2021         | 6              | Regular | False                |
		| Maths1   | 100    | 2021         | 7              | Regular | False                |
		| Maths1   | 100    | 2021         | 8              | Regular | False                |
		| Maths1   | 100    | 2021         | 9              | Regular | False                |
		| Maths1   | 100    | 2021         | 10             | Regular | False                |
		| Maths1   | 100    | 2021         | 11             | Regular | False                |
		| Maths1   | 100    | 2021         | 12             | Regular | False                |
		| Maths1   | 100    | 2122         | 1              | Regular | False                |
		| Maths1   | 100    | 2122         | 2              | Regular | False                |

Scenario: (MathsAndEnglish) Pausing an English course does not effect a maths course
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course   | Amount | ActualEndDate |
		| 2020-8-1  | 2021-10-1 | Maths1   | 1400   | 2021-10-01    |
		| 2020-8-1  | 2021-10-1 | English1 | 1400   | 2021-10-01    |
	When the following maths and english course information is provided
		| StartDate | EndDate   | Course   | Amount | ActualEndDate | PauseDate  |
		| 2020-8-1  | 2021-10-1 | Maths1   | 1400   | 2021-10-01    |            |
		| 2020-8-1  | 2021-10-1 | English1 | 1400   | 2021-10-01    | 2021-03-15 |
	Then Maths and english instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    | IsAfterLearningEnded |
		| Maths1   | 100    | 2021         | 1              | Regular | False                |
		| Maths1   | 100    | 2021         | 2              | Regular | False                |
		| Maths1   | 100    | 2021         | 3              | Regular | False                |
		| Maths1   | 100    | 2021         | 4              | Regular | False                |
		| Maths1   | 100    | 2021         | 5              | Regular | False                |
		| Maths1   | 100    | 2021         | 6              | Regular | False                |
		| Maths1   | 100    | 2021         | 7              | Regular | False                |
		| Maths1   | 100    | 2021         | 8              | Regular | False                |
		| Maths1   | 100    | 2021         | 9              | Regular | False                |
		| Maths1   | 100    | 2021         | 10             | Regular | False                |
		| Maths1   | 100    | 2021         | 11             | Regular | False                |
		| Maths1   | 100    | 2021         | 12             | Regular | False                |
		| Maths1   | 100    | 2122         | 1              | Regular | False                |
		| Maths1   | 100    | 2122         | 2              | Regular | False                |
		| English1 | 100    | 2021         | 1              | Regular | False                |
		| English1 | 100    | 2021         | 2              | Regular | False                |
		| English1 | 100    | 2021         | 3              | Regular | False                |
		| English1 | 100    | 2021         | 4              | Regular | False                |
		| English1 | 100    | 2021         | 5              | Regular | False                |
		| English1 | 100    | 2021         | 6              | Regular | False                |
		| English1 | 100    | 2021         | 7              | Regular | False                |
		| English1 | 100    | 2021         | 8              | Regular | True                 |
		| English1 | 100    | 2021         | 9              | Regular | True                 |
		| English1 | 100    | 2021         | 10             | Regular | True                 |
		| English1 | 100    | 2021         | 11             | Regular | True                 |
		| English1 | 100    | 2021         | 12             | Regular | True                 |
		| English1 | 100    | 2122         | 1              | Regular | True                 |
		| English1 | 100    | 2122         | 2              | Regular | True                 |

Scenario: (OnProgramme - Break Completed) Training provider records a return from a break in learning
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

Scenario: (OnProgramme - Break Completed) Training provider records a return from a break in learning with an updated price
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
 		| 2020-08-01 | 2021-10-01 | 7000  |
 	And the apprenticeship commitment is approved
 	And the following learning support payment information is provided
 		| StartDate | EndDate   |
 		| 2020-8-1  | 2021-10-1 |
 	And a pause date of 2020-10-15 is sent
	When the following price change request is sent
		| EffectiveFromDate | EndDate    | NewTrainingPrice | NewAssessmentPrice |
		| 2021-05-01        | 2021-10-01 | 7500             | 500                |
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
		| 693.33 | 2021         | 10             |
		| 693.33 | 2021         | 11             |
		| 693.33 | 2021         | 12             |
		| 693.33 | 2122         | 1              |
		| 693.33 | 2122         | 2              |