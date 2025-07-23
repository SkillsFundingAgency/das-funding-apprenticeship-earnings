Feature: Recalculate maths and english earnings following withdrawal

Scenario: Withdrawal made before end of qualifying period; no earnings generated
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-09-10     |
	Then no Maths and English earnings are persisted

Scenario: Withdrawal made after end of qualifying period; only earnings before withdrawal date generated
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-09-11     |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 100    | 2021         | 1              |
	