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

# Apprenticeship withdrawn - English Oct 2026 to 15 May 2027 worth £700 earns £100 a month R03 to R09

Scenario: AC1 - Apprenticeship withdrawn; ongoing english and maths course stops at the apprenticeship withdrawal date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |
	And the english and maths earnings history is maintained

Scenario: AC1 - Apprenticeship withdrawn; english and maths learning support stops at the apprenticeship withdrawal date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2026-10-01 | 2027-05-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	Then 4 english and maths learning support payments are persisted
	And the english and maths earnings history is maintained

Scenario: AC2 - Apprenticeship withdrawn after the english and maths course completed early; english and maths unchanged
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount | CompletionDate |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    | 2027-01-20     |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type      |
		| English1 | 100    | 2627         | 3              | Regular   |
		| English1 | 100    | 2627         | 4              | Regular   |
		| English1 | 100    | 2627         | 5              | Regular   |
		| English1 | 400    | 2627         | 6              | Balancing |

Scenario: AC3 - Apprenticeship withdrawn retrospectively before the english and maths course completed; english and maths clawed back
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount | CompletionDate |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    | 2027-05-15     |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2026-10-01 | 2027-05-15 |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |
	And 4 english and maths learning support payments are persisted
	And the english and maths earnings history is maintained

Scenario: Apprenticeship withdrawn on a census date with the english and maths qualifying period met; one instalment earned
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2027-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course | LearnAimRef | Amount |
		| 2027-01-17 | 2027-05-05 | Maths1 | Maths1      | 400    |
	When the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-01-31 |
	Then english and maths instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod | Type    |
		| Maths1 | 100    | 2627         | 6              | Regular |

Scenario Outline: Apprenticeship withdrawn with the english and maths qualifying period not met or no census date reached; no english and maths earnings
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-08-01 | 2027-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate   | EndDate    | Course | LearnAimRef | Amount |
		| <StartDate> | 2027-05-05 | Maths1 | Maths1      | 400    |
	And the english and maths course has the following learning support
		| StartDate   | EndDate    |
		| <StartDate> | 2027-05-05 |
	When the following on-programme request is sent
		| Key            | Value            |
		| WithdrawalDate | <WithdrawalDate> |
	Then no english and maths earnings are persisted
	And 0 english and maths learning support payments are persisted

	Examples:
		| Description                                     | StartDate  | WithdrawalDate |
		| withdrawn on a census date, qualifying not met  | 2027-01-19 | 2027-01-31     |
		| withdrawn before a census date, qualifying met  | 2027-01-01 | 2027-01-15     |

Scenario: Apprenticeship withdrawal removed; english and maths instalments restored
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	When the following on-programme request is sent
		| Key            | Value |
		| WithdrawalDate |       |
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |
		| English1 | 100    | 2627         | 7              | Regular |
		| English1 | 100    | 2627         | 8              | Regular |
		| English1 | 100    | 2627         | 9              | Regular |
	And the english and maths earnings history is maintained

Scenario: Apprenticeship withdrawal removed; english and maths learning support restored when the english and maths details are resent
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2026-10-01 | 2027-05-15 |
	And the following on-programme request is sent
		| Key            | Value      |
		| WithdrawalDate | 2027-02-10 |
	And the following on-programme request is sent
		| Key            | Value |
		| WithdrawalDate |       |
	When the english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2026-10-01 | 2027-05-15 |
	Then 7 english and maths learning support payments are persisted
	And the english and maths earnings history is maintained

Scenario: AC1 - Unapproved apprenticeship withdrawn; ongoing english and maths course stops at the apprenticeship withdrawal date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the unapproved apprenticeship has the following english and maths courses
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And an unapproved apprenticeship has been submitted
	When the unapproved apprenticeship is withdrawn on 2027-02-10
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |

Scenario: AC1 - Unapproved apprenticeship withdrawn; english and maths learning support stops at the apprenticeship withdrawal date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the unapproved apprenticeship has the following english and maths courses
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the unapproved english and maths course has the following learning support
		| StartDate  | EndDate    |
		| 2026-10-01 | 2027-05-15 |
	And an unapproved apprenticeship has been submitted
	When the unapproved apprenticeship is withdrawn on 2027-02-10
	Then 4 english and maths learning support payments are persisted

Scenario: AC1 - Unapproved apprenticeship withdrawn; english and maths course already withdrawn by SLD keeps its own withdrawal date
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the unapproved apprenticeship has the following english and maths courses
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount | WithdrawalDate |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    | 2027-01-10     |
	And an unapproved apprenticeship has been submitted
	When the unapproved apprenticeship is withdrawn on 2027-02-10
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |

Scenario: AC1 - Unapproved apprenticeship withdrawn then approved; english and maths earnings stay stopped
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the unapproved apprenticeship has the following english and maths courses
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the unapproved apprenticeship is withdrawn on 2027-02-10
	When the apprenticeship commitment is approved
	Then the apprenticeship earnings profile is marked as approved
	And english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |

Scenario: Unapproved apprenticeship withdrawal removed; english and maths instalments restored
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2026-10-01 | 2027-07-31 | 5000  |
	And the unapproved apprenticeship has the following english and maths courses
		| StartDate  | EndDate    | Course   | LearnAimRef | Amount |
		| 2026-10-01 | 2027-05-15 | English1 | English1    | 700    |
	And the unapproved apprenticeship is withdrawn on 2027-02-10
	When the unapproved apprenticeship withdrawal is removed
	Then english and maths instalments are persisted as follows
		| Course   | Amount | AcademicYear | DeliveryPeriod | Type    |
		| English1 | 100    | 2627         | 3              | Regular |
		| English1 | 100    | 2627         | 4              | Regular |
		| English1 | 100    | 2627         | 5              | Regular |
		| English1 | 100    | 2627         | 6              | Regular |
		| English1 | 100    | 2627         | 7              | Regular |
		| English1 | 100    | 2627         | 8              | Regular |
		| English1 | 100    | 2627         | 9              | Regular |
