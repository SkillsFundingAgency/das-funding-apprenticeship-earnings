Feature: BreakInLearningPaymentsIntegration

When a break in learning, or a return from a break in learning, is recorded, changed or removed against an approved apprenticeship,
payments are sent the updated approved earnings. A break in learning may be recorded retrospectively, so earnings previously sent
for periods the learner was on a break must be cleared.

Scenario: Break in learning recorded after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "BreakInLearning"
	And the payments event is sent to pv2 with the following on-programme earnings
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |
		| 400    | 2021         | 3              |
		| 400    | 2021         | 4              |
		| 400    | 2021         | 5              |
		| 400    | 2021         | 6              |
		| 400    | 2021         | 7              |

Scenario: Updated break in learning after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	And the earnings are released to payments
	When the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-05-31 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "BreakInLearning"
	And the payments event is sent to pv2 with the following on-programme earnings
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

Scenario: Removed break in learning after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2021-03-15 |
	And the earnings are released to payments
	When the following on-programme request is sent
		| Key       | Value |
		| PauseDate | null  |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "Continuing"
	And the payments event is sent to pv2 with the following on-programme earnings
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

Scenario: Return from a break in learning after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key       | Value      |
		| PauseDate | 2020-10-15 |
	And the earnings are released to payments
	When the following on-programme request is sent
		| Key               | Value                                                                        |
		| PauseDate         | null                                                                         |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-01-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "Continuing"
	And the payments event is sent to pv2 with the following on-programme earnings
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

Scenario: Updated return from a break in learning after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key               | Value                                                                        |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-05-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	When the following on-programme request is sent
		| Key               | Value                                                                        |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-01-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "Continuing"
	And the payments event is sent to pv2 with the following on-programme earnings
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

Scenario: Removed return from a break in learning after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key               | Value                                                                        |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-01-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	# the learner has not yet returned, so the break is open again from the original pause date
	When the following on-programme request is sent
		| Key               | Value                                                                        |
		| PauseDate         | 2020-10-15                                                                   |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "BreakInLearning"
	And the payments event is sent to pv2 with the following on-programme earnings
		| Amount | AcademicYear | DeliveryPeriod |
		| 400    | 2021         | 1              |
		| 400    | 2021         | 2              |

Scenario: Removed break in learning and return after employer approval is sent for payment
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2020-08-01 | 2021-10-01 | 7000       | 25  |
	And the following learning support payment information is provided
		| StartDate  | EndDate    |
		| 2020-08-01 | 2021-10-01 |
	And a LearningApproved event is received for the apprenticeship
	And the following on-programme request is sent
		| Key               | Value                                                                        |
		| PeriodsInLearning | StartDate:2020-08-01, EndDate:2020-10-14, OriginalExpectedEndDate:2021-10-01 |
		| PeriodsInLearning | StartDate:2021-01-16, EndDate:2021-10-01, OriginalExpectedEndDate:2021-10-01 |
	And the earnings are released to payments
	When the following on-programme request is sent
		| Key       | Value |
		| PauseDate | null  |
	And the earnings are released to payments
	Then the payments event is sent to pv2 with the recalculated earnings for the apprenticeship
	And the payments event is sent to pv2 with a training status of "Continuing"
	And the payments event is sent to pv2 with the following on-programme earnings
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
