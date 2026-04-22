Feature: PeriodsInLearningQualification

Acceptance tests related to qualifying period logic for periods in learning. We need to make sure qualifying period logic applies to periods in learning (FLP-1424)

Scenario Outline: Qualifying first period, break, returning in second period with withdrawal
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2023-01-01 | 2023-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate    |
		| 2023-01-01| 2023-12-31 |
	When the following on-programme request is sent
		| Key               | Value                                                                                                  |
		| WithdrawalDate    | <WithdrawalDate>                                                                                       |
		| PeriodsInLearning | StartDate:2023-01-01, EndDate:2023-03-15, OriginalExpectedEndDate:2023-12-31                           |
		| PeriodsInLearning | StartDate:<ReturnStartDate>, EndDate:<WithdrawalDate>, OriginalExpectedEndDate:<ReturnPlannedEndDate>  |
    Then <EarningsCount> on programme earnings are persisted

Examples:
    | ReturnStartDate | WithdrawalDate | ReturnPlannedEndDate | EarningsCount | Description                                                              |
    | 2023-04-20      | 2023-05-31     | 2023-10-04           | 4             | Planned duration >= 168 days, withdraws after 42 days (Qualifies)        |
    | 2023-04-20      | 2023-05-30     | 2023-10-04           | 2             | Planned duration >= 168 days, withdraws after 41 days (Fails to qualify) |
    | 2023-04-20      | 2023-05-03     | 2023-10-03           | 3             | Planned duration < 168 days, withdraws after 14 days (Qualifies)         |
    | 2023-04-20      | 2023-05-02     | 2023-10-03           | 2             | Planned duration < 168 days, withdraws after 13 days (Fails to qualify)  |
    | 2023-04-20      | 2023-05-03     | 2023-05-03           | 3             | Planned duration = 14 days, withdraws after 14 days (Qualifies)          |
    | 2023-04-20      | 2023-05-02     | 2023-05-03           | 2             | Planned duration = 14 days, withdraws after 13 days (Fails to qualify)   |
    | 2023-04-30      | 2023-04-30     | 2023-05-12           | 3             | Planned duration < 14 days, withdraws after 1 day (Qualifies)            |
    | 2023-04-30      | 2023-04-30     | 2023-04-30           | 3             | Planned duration = 1 day, withdraws after 1 day (Qualifies)              |

Scenario Outline: Un-qualifying first period, break, returning in second period with withdrawal
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price |
		| 2023-01-01 | 2023-12-31 | 7000  |
	And the apprenticeship commitment is approved
	And the following learning support payment information is provided
		| StartDate | EndDate    |
		| 2023-01-01| 2023-12-31 |
	When the following on-programme request is sent
		| Key               | Value                                                                                                  |
		| WithdrawalDate    | <WithdrawalDate>                                                                                       |
		| PeriodsInLearning | StartDate:2023-01-01, EndDate:2023-02-01, OriginalExpectedEndDate:2023-12-31                           |
		| PeriodsInLearning | StartDate:<ReturnStartDate>, EndDate:<WithdrawalDate>, OriginalExpectedEndDate:<ReturnPlannedEndDate>  |
    Then <EarningsCount> on programme earnings are persisted
	And the total amount of on programme earnings is <ExpectedOnProgramTotal>

Examples:
    | ReturnStartDate | WithdrawalDate | ReturnPlannedEndDate | EarningsCount | ExpectedOnProgramTotal | Description                                                              |
    | 2023-04-20      | 2023-05-31     | 2023-10-04           |             2 |             2177.77779 | Planned duration >= 168 days, withdraws after 42 days (Qualifies)        |
    | 2023-04-20      | 2023-05-30     | 2023-10-04           |             0 |                      0 | Planned duration >= 168 days, withdraws after 41 days (Fails to qualify) |
    | 2023-04-20      | 2023-05-03     | 2023-10-03           |             1 |             1322.22223 | Planned duration < 168 days, withdraws after 14 days (Qualifies)         |
    | 2023-04-20      | 2023-05-02     | 2023-10-03           |             0 |                      0 | Planned duration < 168 days, withdraws after 13 days (Fails to qualify)  |
    | 2023-04-20      | 2023-05-03     | 2023-05-03           |             1 |                   5600 | Planned duration = 14 days, withdraws after 14 days (Qualifies)          |
    | 2023-04-20      | 2023-05-02     | 2023-05-03           |             0 |                      0 | Planned duration = 14 days, withdraws after 13 days (Fails to qualify)   |
    | 2023-04-30      | 2023-04-30     | 2023-05-12           |             1 |                   5600 | Planned duration < 14 days, withdraws after 1 day (Qualifies)            |
    | 2023-04-30      | 2023-04-30     | 2023-04-30           |             1 |                   5600 | Planned duration = 1 day, withdraws after 1 day (Qualifies)              |