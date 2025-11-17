Feature: MathsAndEnglishRemoval

Validates maths and english payments are correctly calculated after removal/withdrawal back to start of a specific Maths and English course

Scenario: Maths and English withdrawal back to start
	Given An apprenticeship starts on 2020-08-01 and ends on 2021-10-01
	And the apprenticeship commitment is approved
	And the following maths and english course information is provided
		| StartDate | EndDate   | Course | Amount |
		| 2020-8-1  | 2021-11-1 | Maths1 | 1500   |
	When the following maths and english withdrawal is sent
		| LastDayOfLearning | Course |
		| 2020-8-1          | Maths1 |
	Then no Maths and English earnings are persisted
	And the earnings history is maintained