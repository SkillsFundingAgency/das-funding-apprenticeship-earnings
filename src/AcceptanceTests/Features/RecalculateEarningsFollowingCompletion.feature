﻿Feature: Recalculate earnings following completion

Scenario: Early Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following completion is sent
		| CompletionDate |
		| 2021-02-15     |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 6000   | 2021         | 7              | Balancing  |
		| 3000   | 2021         | 7              | Completion |

Scenario: Late Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following completion is sent
		| CompletionDate |
		| 2021-09-15     |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Regular    |
		| 3000   | 2122         | 2              | Completion |

Scenario: On Time Completion
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price | FundingBandMaximum |
		| 2020-08-15 | 2021-07-31 | 15000 | 25000              |
	And earnings are calculated
	When the following completion is sent
		| CompletionDate |
		| 2021-07-31     |
	Then the instalments are balanced as follows
		| Amount | AcademicYear | DeliveryPeriod | Type       |
		| 1000   | 2021         | 1              | Regular    |
		| 1000   | 2021         | 2              | Regular    |
		| 1000   | 2021         | 3              | Regular    |
		| 1000   | 2021         | 4              | Regular    |
		| 1000   | 2021         | 5              | Regular    |
		| 1000   | 2021         | 6              | Regular    |
		| 1000   | 2021         | 7              | Regular    |
		| 1000   | 2021         | 8              | Regular    |
		| 1000   | 2021         | 9              | Regular    |
		| 1000   | 2021         | 10             | Regular    |
		| 1000   | 2021         | 11             | Regular    |
		| 1000   | 2021         | 12             | Regular    |
		| 3000   | 2021         | 12             | Completion |
