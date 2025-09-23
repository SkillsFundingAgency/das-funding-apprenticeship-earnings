Feature: ChangeOfMathsAndEnglishCompletionDate

Validates maths and english payments are correctly calculated when the completion date changes

Scenario: Maths and English completion moved later
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	When the following maths and english completion change request is sent
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-12-01    |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 100    | 2021         | 1              |
		| Maths1 | 100    | 2021         | 2              |
		| Maths1 | 100    | 2021         | 3              |
		| Maths1 | 100    | 2021         | 4              |
		| Maths1 | 1100   | 2021         | 5              |
	And the earnings history is maintained

Scenario: Maths and English completion moved earlier
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	When the following maths and english completion change request is sent
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-10-01    |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 100    | 2021         | 1              |
		| Maths1 | 100    | 2021         | 2              |
		| Maths1 | 1300   | 2021         | 3              |
	And the earnings history is maintained

Scenario: Maths and English completion removed
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	When the following maths and english completion change request is sent
		| StartDate | EndDate   | Course | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 100    | 2021         | 1              |
		| Maths1 | 100    | 2021         | 2              |
		| Maths1 | 100    | 2021         | 3              |
		| Maths1 | 100    | 2021         | 4              |
		| Maths1 | 100    | 2021         | 5              |
		| Maths1 | 100    | 2021         | 6              |
		| Maths1 | 100    | 2021         | 7              |
		| Maths1 | 100    | 2021         | 8              |
		| Maths1 | 100    | 2021         | 9              |
		| Maths1 | 100    | 2021         | 10             |
		| Maths1 | 100    | 2021         | 11             |
		| Maths1 | 100    | 2021         | 12             |
		| Maths1 | 100    | 2122         | 1              |
		| Maths1 | 100    | 2122         | 2              |
		| Maths1 | 100    | 2122         | 3              |
	And the earnings history is maintained