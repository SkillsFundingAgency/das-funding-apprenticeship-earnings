Feature: Calculate earnings for learning payments

Scenario: Simple Earnings Generation
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-09-30 | 17500 |
	When earnings are calculated
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 1000   | 2021         | 1              |
		| 1000   | 2021         | 2              |
		| 1000   | 2021         | 3              |
		| 1000   | 2021         | 4              |
		| 1000   | 2021         | 5              |
		| 1000   | 2021         | 6              |
		| 1000   | 2021         | 7              |
		| 1000   | 2021         | 8              |
		| 1000   | 2021         | 9              |
		| 1000   | 2021         | 10             |
		| 1000   | 2021         | 11             |
		| 1000   | 2021         | 12             |
		| 1000   | 2122         | 1              |
		| 1000   | 2122         | 2              |

Scenario: As a Training provider I want the completion earnings (Forecast) so that they feed into payment calculations and I get paid
	Given the apprenticeship commitment is approved
	When the adjusted price has been calculated
	Then the total completion payment amount of 20% of the adjusted price must be calculated
	
Scenario: Funding Band Maximum Cap
	Given an apprenticeship has been created with the following information
		| Age |
		| 18  |
	And a funding band maximum of 3750
	And the following Price Episodes
		| StartDate  | EndDate    | Price |
		| 2020-08-01 | 2021-01-31 | 8000  |
	When earnings are calculated
	Then On programme earnings are persisted as follows
		| Amount | AcademicYear | DeliveryPeriod |
		| 500   | 2021         | 1              |
		| 500   | 2021         | 2              |
		| 500   | 2021         | 3              |
		| 500   | 2021         | 4              |
		| 500   | 2021         | 5              |
		| 500   | 2021         | 6              |

Scenario: As a Finance Officer I want to know the funding line type for earnings So that I can estimate the correct forecasted funding (under 19)
	Given an apprenticeship has been created with the following information
		| Age |
		| 17  |
	When the apprenticeship commitment is approved 
	Then the funding line type 16-18 must be used in the calculation 

Scenario: As a Finance Officer I want to know the funding line type for earnings So that I can estimate the correct forecasted funding (19+)
	Given an apprenticeship has been created with the following information
		| Age |
		| 20  |
	When the apprenticeship commitment is approved 
	Then the funding line type 19 plus must be used in the calculation 

Scenario: Non Pilot Apprenticeship
	Given An apprenticeship not on the pilot has been created as part of the approvals journey
	Then Earnings are not generated for that apprenticeship