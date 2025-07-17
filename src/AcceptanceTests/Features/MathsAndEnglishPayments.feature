Feature: MathsAndEnglishPayments

Validates maths and english payments are correctly calculated

Scenario: Maths and English earnings for a brand new course
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-8-1  | 2021-11-1  | Maths1      | 1500   |
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

Scenario: Maths and English earnings past the end of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2021-1-1  | 2021-12-31 | LateEnglish | 1200   |
	Then Maths and english instalments are persisted as follows
		| Course      | Amount | AcademicYear | DeliveryPeriod |
		| LateEnglish | 100    | 2021         | 6              |
		| LateEnglish | 100    | 2021         | 7              |
		| LateEnglish | 100    | 2021         | 8              |
		| LateEnglish | 100    | 2021         | 9              |
		| LateEnglish | 100    | 2021         | 10             |
		| LateEnglish | 100    | 2021         | 11             |
		| LateEnglish | 100    | 2021         | 12             |
		| LateEnglish | 100    | 2122         | 1              |
		| LateEnglish | 100    | 2122         | 2              |
		| LateEnglish | 100    | 2122         | 3              |
		| LateEnglish | 100    | 2122         | 4              |
		| LateEnglish | 100    | 2122         | 5              |
	
Scenario: Maths and English before the start of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course     | Amount |
		| 2020-1-1  | 2021-1-15 | EarlyMaths | 600    |
	Then Maths and english instalments are persisted as follows
		| Course     | Amount | AcademicYear | DeliveryPeriod |
		| EarlyMaths | 50     | 1920         | 6              |
		| EarlyMaths | 50     | 1920         | 7              |
		| EarlyMaths | 50     | 1920         | 8              |
		| EarlyMaths | 50     | 1920         | 9              |
		| EarlyMaths | 50     | 1920         | 10             |
		| EarlyMaths | 50     | 1920         | 11             |
		| EarlyMaths | 50     | 1920         | 12             |
		| EarlyMaths | 50     | 2021         | 1              |
		| EarlyMaths | 50     | 2021         | 2              |
		| EarlyMaths | 50     | 2021         | 3              |
		| EarlyMaths | 50     | 2021         | 4              |
		| EarlyMaths | 50     | 2021         | 5              |

Scenario: Maths and English earnings for a course which does not span a census date
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate  | EndDate    | Course | Amount |
		| 2021-02-01 | 2021-02-26 | Maths1 | 900   |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 900    | 2021         | 7              |

Scenario: Maths and English completion earnings
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-8-1  | 2021-11-1  | Maths1      | 1500   |
	When the following maths and english completion change request is sent
		| StartDate | EndDate   | Course | Amount | ActualEndDate |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   | 2020-11-01    |
	Then Maths and english instalments are persisted as follows
		| Course | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | 100    | 2021         | 1              |
		| Maths1 | 100    | 2021         | 2              |
		| Maths1 | 100    | 2021         | 3              |
		| Maths1 | 1200   | 2021         | 4              |