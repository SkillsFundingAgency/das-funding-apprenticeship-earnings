Feature: IncentivesAfterDaysInLearningChange

Tests the effects of days in learning changes on 16-18 and 19-24 incentive payments

Scenario Outline: AC1 cancel 365 day earning if duration < 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                            | Value                            |
		| WithdrawalDate                 | <WithdrawalDate>                 |
		| CompletionDate                 | <CompletionDate>                 |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |

	Examples:
		| DateChangeType  | WithdrawalDate | CompletionDate |
		| withdrawal date | 2021-07-30     |                |
		| completion date |                | 2021-07-30     |

Scenario Outline: AC1 cancel 365 day earning if duration < 365 days due to Break in Learning
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key               | Value  |
		| PeriodsInLearning | <PIL1> |
		| PeriodsInLearning | <PIL2> |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2021-12-14 |
		| EmployerIncentive | 500    | 2021-12-14 |

	Examples:
		| DateChangeType                    | PIL1                                                                         | PIL2                                                                         |
		| last day in learning before a BIL | StartDate:2020-08-01, EndDate:2020-08-15, OriginalExpectedEndDate:2022-07-31 | StartDate:2021-10-01, EndDate:2022-07-31, OriginalExpectedEndDate:2022-07-31 |

Scenario Outline: AC2 cancel 90 day earning if duration < 90 days
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key                            | Value                            |
		| WithdrawalDate                 | <WithdrawalDate>                 |
		| PriceEndDate                   | <PriceEndDate>                   |
		| PriceStartDate                 | <PriceStartDate>                 |
	Then no Additional Payments are persisted

	Examples:
		| DateChangeType   | WithdrawalDate | PriceEndDate | PriceStartDate |
		| withdrawal date  | 2020-10-25     | 2022-07-31   | 2020-08-01     |
		| planned end date |                | 2020-10-25   | 2020-08-01     |

Scenario Outline: AC3 add 365 day earning if duration increases past 365 days
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And the following Price Episodes
		| StartDate   | EndDate   | Price |
		| <StartDate> | <EndDate> | 17000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key                            | Value                            |
		| PriceStartDate                 | <StartDate>                      |
		| PriceEndDate                   | <EndDate>                        |
	When the following on-programme request is sent
		| Key                            | Value                            |
		| WithdrawalDate                 | <NewWithdrawalDate>              |
		| PriceEndDate                   | <NewPriceEndDate>                |
		| PriceStartDate                 | <StartDate>                      |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |
		| ProviderIncentive | 500    | 2021-07-31 |
		| EmployerIncentive | 500    | 2021-07-31 |

	Examples:
		| DateChangeType   | StartDate  | EndDate    | InitialWithdrawalDate | NewWithdrawalDate | NewPriceEndDate |
		| withdrawal date  | 2020-08-01 | 2022-07-31 | 2021-01-15            |                   | 2022-07-31      |
		| planned end date | 2020-08-01 | 2021-04-01 |                       |                   | 2022-07-31      |

Scenario Outline: AC3 add 365 day earning if duration increases past 365 days due to Break in Learning
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key                            | Value                            |
		| PeriodsInLearning              | <P1Initial>                      |
		| PeriodsInLearning              | <P2Initial>                      |
	When the following on-programme request is sent
		| Key                            | Value                            |
		| PeriodsInLearning              | <P1New>                          |
		| PeriodsInLearning              | <P2New>                          |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-11-28 |
		| EmployerIncentive | 500    | 2020-11-28 |
		| ProviderIncentive | 500    | 2021-08-30 |
		| EmployerIncentive | 500    | 2021-08-30 |

	Examples:
		| DateChangeType            | P1Initial                                                                    | P2Initial                                                                    | P1New                                                                        | P2New                                                                        |
		| first day back from a BIL | StartDate:2020-08-01, EndDate:2020-08-15, OriginalExpectedEndDate:2022-07-31 | StartDate:2021-10-01, EndDate:2022-07-31, OriginalExpectedEndDate:2022-07-31 | StartDate:2020-08-01, EndDate:2020-08-15, OriginalExpectedEndDate:2022-07-31 | StartDate:2020-09-15, EndDate:2022-07-31, OriginalExpectedEndDate:2022-07-31 |

Scenario Outline: AC4 add 90 day earning if duration increases past 90 days
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 17000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key                            | Value                   |
		| CompletionDate                 | <InitialCompletionDate> |
	When the following on-programme request is sent
		| Key                            | Value                            |
		| CompletionDate                 | <NewCompletionDate>              |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |

	Examples:
		| DateChangeType  | InitialCompletionDate | NewCompletionDate |
		| completion date | 2020-10-15            | 2020-11-15        |

Scenario Outline: AC4 add 90 day earning if duration increases past 90 days due to Break in Learning
	Given an apprenticeship has been created with the following information
		| Age |
		|  18 |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2020-11-10 | 17000 |
	And earnings are calculated
	And the following on-programme request is sent
		| Key                            | Value       |
		| PeriodsInLearning              | <P1Initial> |
		| PeriodsInLearning              | <P2Initial> |
	When the following on-programme request is sent
		| Key                            | Value                            |
		| PeriodsInLearning              | <P1New>                          |
		| PeriodsInLearning              | <P2New>                          |
	Then Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-11-03 |
		| EmployerIncentive | 500    | 2020-11-03 |

	Examples:
		| DateChangeType                    | P1Initial                                                                    | P2Initial                                                                    | P1New                                                                        | P2New                                                                        |
		| last day in learning before a BIL | StartDate:2020-08-01, EndDate:2020-08-15, OriginalExpectedEndDate:2020-11-10 | StartDate:2020-10-01, EndDate:2020-11-10, OriginalExpectedEndDate:2020-11-10 | StartDate:2020-08-01, EndDate:2020-09-25, OriginalExpectedEndDate:2020-11-10 | StartDate:2020-10-01, EndDate:2020-11-10, OriginalExpectedEndDate:2020-11-10 |
