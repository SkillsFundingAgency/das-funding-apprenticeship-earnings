Feature: MathsAndEnglishRemoval

Validates english and maths payments are correctly calculated after removal/withdrawal back to start of a specific english and maths course

Scenario: english and maths withdrawal back to start
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following english and maths course information is provided
		| StartDate | EndDate   | Course | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   |
	When the following english and maths withdrawal is sent
		| LastDayOfLearning | Course |
		| 2020-8-1          | Maths1 |
	Then all english and maths earnings are soft deleted
	And the earnings history is maintained