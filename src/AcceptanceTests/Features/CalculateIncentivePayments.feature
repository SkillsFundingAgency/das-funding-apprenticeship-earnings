Feature: Calculate incentive payments

Scenario: 16-18 Incentive Payments Generation
	Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	| EmployerIncentive | 500    | 2021-07-31 |
	#And an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods
	#| Type              | Amount | CalendarMonth | CalendarYear |
	#| EmployerIncentive | 500    | 10            | 2020         |
	#| ProviderIncentive | 500    | 10            | 2020         |
	#| EmployerIncentive | 500    | 7             | 2021         |
	#| ProviderIncentive | 500    | 7             | 2021         |

Scenario: 16-18 Incentive Payments Generation - 90 day apprenticeship
	Given an apprenticeship has been created with the following information
	| Age |
	| 18  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2020-10-30 | 10000 |
	When earnings are calculated
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	#And an EarningsGeneratedEvent is raised with the following incentives as Delivery Periods
	#| Type              | Amount | CalendarMonth | CalendarYear |
	#| EmployerIncentive | 500    | 10            | 2020         |
	#| ProviderIncentive | 500    | 10            | 2020         |

	
Scenario: 16-18 Incentive Payments Generation - learner outside of age range
	Given an apprenticeship has been created with the following information
	| Age |
	| 21  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	Then no Additional Payments are persisted
	#And an EarningsGeneratedEvent is raised with no incentives as Delivery Periods

Scenario: 19-24 Incentive Payments Generation - Provider and employer payments
	Given an apprenticeship has been created with the following information
	| Age |
	| 20  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And care details are saved with
	| CareLeaverEmployerConsentGiven       | IsCareLeaver     | HasEHCP    |
	| <care_leaver_employer_consent_given> | <is_care_leaver> | <has_ehcp> |
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	| EmployerIncentive | 500    | 2021-07-31 |
	And recalculate event is sent with the following incentives
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| EmployerIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	| EmployerIncentive | 500    | 2021-07-31 |
Examples:
	| care_leaver_employer_consent_given | is_care_leaver | has_ehcp |
	| true                               | true           | false    |
	| true                               | false          | true     |
	| false                              | false          | true     |
	| true                               | true           | true     |

Scenario: 19-24 Incentive Payments Generation - Only provider payments
	Given an apprenticeship has been created with the following information
	| StartDate  | EndDate    | Price | Age |
	| 2020-08-01 | 2024-07-31 | 15000 | 20  |
	When earnings are calculated
	And care details are saved with
	| CareLeaverEmployerConsentGiven       | IsCareLeaver     | HasEHCP    |
	| <care_leaver_employer_consent_given> | <is_care_leaver> | <has_ehcp> |
	Then Additional Payments are persisted as follows
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |
	And recalculate event is sent with the following incentives
	| Type              | Amount | DueDate    |
	| ProviderIncentive | 500    | 2020-10-29 |
	| ProviderIncentive | 500    | 2021-07-31 |

Examples:
	| care_leaver_employer_consent_given | is_care_leaver | has_ehcp |
	| false                              | true           | false    |

Scenario: 19-24 Incentive Payments Generation - Is not eligible for incentive
	Given an apprenticeship has been created with the following information
	| Age |
	| 20  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And care details are saved with
	| CareLeaverEmployerConsentGiven | IsCareLeaver | HasEHCP |
	| false                          | false        | false   |
	Then no Additional Payments are persisted
	And Earnings are not recalculated for that apprenticeship

Scenario: Incentives are recalculated twice and history is created without keys clashing
	Given an apprenticeship has been created with the following information
	| Age |
	| 20  |
	And the following Price Episodes
	| StartDate  | EndDate    | Price |
	| 2020-08-01 | 2024-07-31 | 15000 |
	When earnings are calculated
	And care details are saved with
	| CareLeaverEmployerConsentGiven | IsCareLeaver | HasEHCP |
	| false                          | true         | false   |
	And care details are saved with
	| CareLeaverEmployerConsentGiven | IsCareLeaver | HasEHCP |
	| false                          | false        | false   |
	Then there are 3 records in earning profile history