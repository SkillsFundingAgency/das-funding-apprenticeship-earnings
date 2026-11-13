Feature: English and maths Withdrawal

Scenario: Withdrawal made before end of qualifying period; no earnings generated
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-09-10     |
	Then no english and maths earnings are persisted
	And the english and maths earnings history is maintained

Scenario: Withdrawal made after end of qualifying period; only earnings before withdrawal date generated
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-09-11     |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 100    | 2021         | 1              | Regular |
	And the english and maths earnings history is maintained
	
Scenario: english and maths withdrawal back to start
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-10-01 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   |
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | LearnAimRef | Amount | WithdrawalDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | Maths1      | 1500   | 2020-8-1       |
	Then no english and maths earnings are persisted
	And the english and maths earnings history is maintained

Scenario Outline: Withdrawal on or after a census date with the qualifying period met; one instalment earned
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2027-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate   | EndDate    | Course | LearnAimRef | Amount | WithdrawalDate   |
		| <StartDate> | 2027-05-05 | Maths1 | Maths1      | 400    | <WithdrawalDate> |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 100    | 2627         | 6              | Regular |
	And the english and maths earnings history is maintained

	Examples:
		| Description                    | StartDate  | WithdrawalDate |
		| withdrawn on a census date     | 2027-01-17 | 2027-01-31     |
		| withdrawn after a census date  | 2027-01-18 | 2027-02-01     |

Scenario Outline: Withdrawal with the qualifying period not met or no census date reached; no earnings generated
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2027-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate   | EndDate    | Course | LearnAimRef | Amount | WithdrawalDate   |
		| <StartDate> | 2027-05-05 | Maths1 | Maths1      | 400    | <WithdrawalDate> |
	Then no english and maths earnings are persisted
	And the english and maths earnings history is maintained

	Examples:
		| Description                                                     | StartDate  | WithdrawalDate |
		| withdrawn on a census date, qualifying period not met           | 2027-01-19 | 2027-01-31     |
		| withdrawn after a census date, qualifying period not met        | 2027-01-20 | 2027-02-01     |
		| withdrawn before a census date, qualifying period not met       | 2027-01-01 | 2027-01-13     |
		| withdrawn before a census date, qualifying period met           | 2027-01-01 | 2027-01-15     |