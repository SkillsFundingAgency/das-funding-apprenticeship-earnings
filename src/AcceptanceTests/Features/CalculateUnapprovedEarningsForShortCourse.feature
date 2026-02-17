Feature: Calculate unapproved earnings for short course

Scenario: Short course unapproved earnings generation
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	Then On programme short course earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		|    600 |         2021 |              7 |
		|   1400 |         2021 |             11 |
	And Calculation Data is serialised