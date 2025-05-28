Feature: MathsAndEnglishPayments

Validates maths and english payments are correctly calculated

Scenario: Maths and English earnings for a brand new course
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-8-1  | 2021-11-1  | Maths1      | 1500   |
	Then Maths and english instalments are persisted as follows
		| Course | Type            | Amount | AcademicYear | DeliveryPeriod |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 1              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 2              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 3              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 4              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 5              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 6              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 7              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 8              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 9              |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 10             |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 11             |
		| Maths1 | MathsAndEnglish | 100    | 2021         | 12             |
		| Maths1 | MathsAndEnglish | 100    | 2122         | 1              |
		| Maths1 | MathsAndEnglish | 100    | 2122         | 2              |
		| Maths1 | MathsAndEnglish | 100    | 2122         | 3              |

Scenario: Maths and English earnings past the end of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2021-1-1  | 2021-12-31 | LateEnglish | 1200   |
	Then Maths and english instalments are persisted as follows
		| Course  | Type            | Amount | AcademicYear | DeliveryPeriod |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 6              |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 7              |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 8              |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 9              |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 10             |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 11             |
		| LateEnglish | MathsAndEnglish | 100    | 2021         | 12             |
		| LateEnglish | MathsAndEnglish | 100    | 2122         | 1              |
		| LateEnglish | MathsAndEnglish | 100    | 2122         | 2              |
		| LateEnglish | MathsAndEnglish | 100    | 2122         | 3              |
		| LateEnglish | MathsAndEnglish | 100    | 2122         | 4              |
		| LateEnglish | MathsAndEnglish | 100    | 2122         | 5              |
	
Scenario: Maths and English before the start of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-1-1  | 2020-10-31 | EarlyMaths  | 500    |
	Then Maths and english instalments are persisted as follows
		| Course     | Type            | Amount | AcademicYear | DeliveryPeriod |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 6              |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 7              |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 8              |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 9              |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 10             |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 11             |
		| EarlyMaths | MathsAndEnglish | 50     | 1920         | 12             |
		| EarlyMaths | MathsAndEnglish | 50     | 2021         | 1              |
		| EarlyMaths | MathsAndEnglish | 50     | 2021         | 2              |
		| EarlyMaths | MathsAndEnglish | 50     | 2021         | 3              |