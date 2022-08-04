Feature: Calculate earnings for learning payments

Scenario: Simple Earnings Generation
	Given An apprenticeship learner event comes in from approvals
	Then An earnings generated event is published with the correct learning amounts

Scenario: As a Training provider I want the completion earnings (Forecast) so that they feed into payment calculations and I get paid
	Given the apprenticeship commitment is approved
	When the adjusted price has been calculated
	Then the total completion payment amount of 20% of the adjusted price must be calculated
	And the total completion payment amount must be paid the following month after the apprenticeship completion end date