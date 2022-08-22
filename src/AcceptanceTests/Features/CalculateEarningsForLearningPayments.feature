Feature: Calculate earnings for learning payments

Scenario: Simple Earnings Generation
	Given An apprenticeship has been created as part of the approvals journey
	Then Earnings are generated with the correct learning amounts

Scenario: Funding Band Maximum Cap
	Given An apprenticeship learner event comes in from approvals with a funding band maximum lower than the agreed price
	Then Earnings are generated with the correct learning amounts