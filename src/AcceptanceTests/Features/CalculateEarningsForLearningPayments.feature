Feature: Calculate earnings for learning payments

Scenario: Simple Earnings Generation
	Given An apprenticeship learner event comes in from approvals
	Then An earnings generated event is published with the correct learning amounts