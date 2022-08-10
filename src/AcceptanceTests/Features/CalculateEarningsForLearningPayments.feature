Feature: Calculate earnings for learning payments

Scenario: Simple Earnings Generation
	Given An apprenticeship has been created as part of the approvals journey
	Then Earnings are generated with the correct learning amounts

Scenario: As a Training provider I want the completion earnings (Forecast) so that they feed into payment calculations and I get paid
	Given the apprenticeship commitment is approved
	When the adjusted price has been calculated
	Then the total completion payment amount of 20% of the adjusted price must be calculated