Feature: Withdrawal

Scenario: Withdrawal made partway through apprenticeship; recalc earnings
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-11-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |

Scenario: Withdrawal made back to start of apprenticeship; remove all incentive payments
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-08-15 |
	Then no Additional Payments are persisted
	And the earnings history is maintained

Scenario: Withdrawal date falls before 90 day incentive date
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-10-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
	And the earnings history is maintained
	And no Additional Payments are persisted

Scenario: Withdrawal date falls after 90 day incentive date, but before census date (incentives still due)
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-10-30 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |

Scenario: Withdrawal made before end of 42 day qualifying period; recalc earnings as zero
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-15 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-09-15 |
	Then no on programme earnings are persisted
	And the earnings history is maintained

Scenario: Withdrawal made before end of 14 qualifying period; recalc earnings as zero
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-01-31 | 2020-02-13 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-02-12 |
	Then no on programme earnings are persisted
	And the earnings history is maintained

Scenario: Withdrawal made partway through apprenticeship; no 90 day incentive when withdrawal before incentive date
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2020-10-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
	And the earnings history is maintained
	Then no Additional Payments are persisted

Scenario: Withdrawal made partway through apprenticeship; no 365 day incentive when withdrawal before incentive date
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 25000
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2022-07-31 | 15000 |
	And earnings are calculated
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2021-01-15 |
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 500   | 2021         | 1              |
		| 500   | 2021         | 2              |
		| 500   | 2021         | 3              |
		| 500   | 2021         | 4              |
		| 500   | 2021         | 5              |
	And the earnings history is maintained
	And Additional Payments are persisted as follows
		| Type              | Amount | DueDate    |
		| ProviderIncentive | 500    | 2020-10-29 |
		| EmployerIncentive | 500    | 2020-10-29 |