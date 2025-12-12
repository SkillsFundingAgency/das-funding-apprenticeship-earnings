Feature: Recalculate english and maths earnings following withdrawal

Scenario: Withdrawal made before end of qualifying period; no earnings generated
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-09-10     |
	Then no english and maths earnings are persisted
	And the earnings history is maintained

Scenario: Withdrawal made after end of qualifying period; only earnings before withdrawal date generated
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-09-11     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 100    | 2021         | 1              | Regular |
	And the earnings history is maintained
	