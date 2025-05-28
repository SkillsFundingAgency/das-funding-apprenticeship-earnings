Feature: MathsAndEnglishPayments

Validates maths and english payments are correctly calculated

Scenario: Maths and English earnings for a brand new course
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-8-1  | 2021-10-1  | Maths1      | 1500   |
	Then Maths and english instalments are persisted for Maths1 as follows
		| Type            | Amount | AcademicYear | DeliveryPeriod |
		| MathsAndEnglish | 100    | 2021         | 1                |
		| MathsAndEnglish | 100    | 2021         | 2                |
		| MathsAndEnglish | 100    | 2021         | 3                |
		| MathsAndEnglish | 100    | 2021         | 4                |
		| MathsAndEnglish | 100    | 2021         | 5                |
		| MathsAndEnglish | 100    | 2021         | 6                |
		| MathsAndEnglish | 100    | 2021         | 7                |
		| MathsAndEnglish | 100    | 2021         | 8                |
		| MathsAndEnglish | 100    | 2021         | 9                |
		| MathsAndEnglish | 100    | 2021         | 10               |
		| MathsAndEnglish | 100    | 2021         | 11               |
		| MathsAndEnglish | 100    | 2021         | 12               |
		| MathsAndEnglish | 100    | 2122         | 1                |
		| MathsAndEnglish | 100    | 2122         | 2                |
		| MathsAndEnglish | 100    | 2122         | 3                |

Scenario: Maths and English earnings past the end of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2021-1-1  | 2021-12-31 | LateEnglish | 1200   |
	Then Maths and english instalments are persisted for LateEnglish as follows
		| Type            | Amount | AcademicYear | DeliveryPeriod |
		| MathsAndEnglish | 100    | 2021         | 6              |
		| MathsAndEnglish | 100    | 2021         | 7              |
		| MathsAndEnglish | 100    | 2021         | 8              |
		| MathsAndEnglish | 100    | 2021         | 9              |
		| MathsAndEnglish | 100    | 2021         | 10             |
		| MathsAndEnglish | 100    | 2021         | 11             |
		| MathsAndEnglish | 100    | 2021         | 12             |
		| MathsAndEnglish | 100    | 2122         | 1              |
		| MathsAndEnglish | 100    | 2122         | 2              |
		| MathsAndEnglish | 100    | 2122         | 3              |
		| MathsAndEnglish | 100    | 2122         | 4              |
		| MathsAndEnglish | 100    | 2122         | 5              |
	
Scenario: Maths and English before the start of the apprenticeship
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate    | Course      | Amount |
		| 2020-1-1  | 2020-10-31 | EarlyMaths  | 500    |
	Then Maths and english instalments are persisted for EarlyMaths as follows
		| Type            | Amount | AcademicYear | DeliveryPeriod |
		| MathsAndEnglish | 50     | 1920         | 6              |
		| MathsAndEnglish | 50     | 1920         | 7              |
		| MathsAndEnglish | 50     | 1920         | 8              |
		| MathsAndEnglish | 50     | 1920         | 9              |
		| MathsAndEnglish | 50     | 1920         | 10             |
		| MathsAndEnglish | 50     | 1920         | 11             |
		| MathsAndEnglish | 50     | 1920         | 12             |
		| MathsAndEnglish | 50     | 2021         | 1              |
		| MathsAndEnglish | 50     | 2021         | 2              |
		| MathsAndEnglish | 50     | 2021         | 3              |