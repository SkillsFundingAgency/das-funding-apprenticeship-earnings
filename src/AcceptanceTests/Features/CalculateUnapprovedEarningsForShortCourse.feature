Feature: Calculate unapproved earnings for short course

Scenario: Short course, unapproved earnings generation
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|    600 |         2021 |              7 |
		|   1400 |         2021 |             11 |
	And Calculation Data is serialised

Scenario: Duplicate short course submitted with change of price, unapproved earnings generation
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       4000 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|   1200 |         2021 |              7 |
		|   2800 |         2021 |             11 |
	And Calculation Data is serialised
	And the short course earnings history is maintained

Scenario: Duplicate short course submitted with change of dates, unapproved earnings generation
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	And a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2022-01-01 | 2022-06-25      |       2000 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|    600 |         2122 |              7 |
		|   1400 |         2122 |             11 |
	And Calculation Data is serialised
	And the short course earnings history is maintained

Scenario: Short course, unapproved earnings generation with completion
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | CompletionDate |
		| 2021-01-01 | 2021-06-25      |       2000 | 2021-05-25     |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|    600 |         2021 |              7 |
		|   1400 |         2021 |             10 |
	And Calculation Data is serialised

Scenario: Duplicate short course submitted with change of completion date, unapproved earnings generation
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice | CompletionDate |
		| 2021-01-01 | 2021-06-25      |       2000 | 2021-05-25     |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|    600 |         2021 |              7 |
		|   1400 |         2021 |             10 |
	And Calculation Data is serialised