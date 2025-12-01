Feature: AgeAtStartChanged

Tests earnings (specifically incentives) are recalculated effectively post a change in the learner's start date and/or date of birth.

Scenario: (change of dob) 90th and 365th day count must be recalculated (always eligible)
	Given an apprenticeship has been created with the following information
		| StartDate  | EndDate    | Price | Age |
		| 2020-08-01 | 2021-10-01 | 7000  | 17  |
	And a training provider has recorded a change of learning start date and/or date of birth
	And the learner was eligible for 16-18 or 19-24 incentives before the change
	And the learner is still eligible for 16-18 or 19-24 incentives after the change
	When the due dates of the incentive earnings are calculated
	Then the 90th and 365th due dates for incentive payments are recalculated based on the latest learning start date
	And earnings based on the previous learning start date are removed

Scenario: (change of dob) 90th and 365th day count must be recalculated (was eligible, now ineligible)
	Given a training provider has recorded a change of learning start date and/or date of birth
	And the learner was eligible for 16-18 or 19-24 incentives before the change
	And the learner is now ineligible for 16-18 or 19-24 incentives after the change
	When the due dates of the incentive earnings are calculated
	Then incentive earnings based on the previous learning start date are removed
	And the future incentive earnings based on the latest learning start date are removed

Scenario: (change of dob) 90th and 365th day count must be recalculated (was ineligible, now eligible)
	Given a training provider has recorded a change of learning start date and/or date of birth
	And the learner was ineligible for 16-18 or 19-24 incentives before the change
	And the learner is now eligible for 16-18 or 19-24 incentives after the change
	When the due dates of the incentive earnings are calculated
	Then update the incentive earnings based on the latest learning start date

Scenario: (change of dob) No incentives generated (always ineligible)
	Given a training provider has recorded a change of learning start date and/or date of birth
	And the learner was ineligible for 16-18 or 19-24 incentives before the change
	And the learner is still ineligible for 16-18 or 19-24 incentives after the change
	Then do not record/update incentive earnings for that apprentice


#(change of start date)
#(change of start date and dob)
