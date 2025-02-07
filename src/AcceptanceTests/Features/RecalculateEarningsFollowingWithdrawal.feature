Feature: Recalculate earnings following withdrawal

Scenario: Withdrawal made partway through apprenticeship; recalc earnings
	Given an apprenticeship has been created
	And the total price is below or at the funding band maximum
	And the earnings for the apprenticeship are calculated
	When a withdrawal was sent partway through the apprenticeship
	Then the number of instalments is determined by the number of census dates passed between the start date and the withdrawal date
	And a new earnings profile id is set
	And the history of old and new earnings is maintained