Feature: Recalculate incentive earnings following date or duration change

Scenario: Apprenticeship start date change results in less than 365 days
	Given a learner is 17
	And an apprenticeship has been created
	And the earnings for the apprenticeship are calculated
	When a start date change is approved resulting in a duration of 364 days
	Then a first incentive payment is generated
	And no second incentive payment is generated

Scenario: Apprenticeship start date change results in less than 90 days
	Given a learner is 17
	And an apprenticeship has been created
	And the earnings for the apprenticeship are calculated
	When a start date change is approved resulting in a duration of 89 days
	Then no first incentive payment is generated
	And no second incentive payment is generated

    #expecting R09 (April) getting R08
Scenario: Apprenticeship start date change results in 365 days
	Given a learner is 17
	And an apprenticeship has been created
	And the earnings for the apprenticeship are calculated
	When a start date change is approved resulting in a duration of 365 days
	Then a first incentive payment is generated
	And a second incentive payment is generated