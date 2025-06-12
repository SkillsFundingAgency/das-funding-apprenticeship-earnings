#Feature: Calculate earnings for learning payments
#
#Scenario: Simple Earnings Generation
#	Given An apprenticeship has been created as part of the approvals journey
#	Then Earnings are generated with the correct learning amounts
#
#Scenario: As a Training provider I want the completion earnings (Forecast) so that they feed into payment calculations and I get paid
#	Given the apprenticeship commitment is approved
#	When the adjusted price has been calculated
#	Then the total completion payment amount of 20% of the adjusted price must be calculated
#	
#Scenario: Funding Band Maximum Cap
#	Given An apprenticeship has been created as part of the approvals journey with a funding band maximum lower than the agreed price
#	Then Earnings are generated with the correct learning amounts
#
#Scenario: As a Finance Officer I want to know the funding line type for earnings So that I can estimate the correct forecasted funding (under 19)
#	Given the apprenticeship learner is 16-18 at the start of the apprenticeship 
#	When the apprenticeship commitment is approved 
#	Then the funding line type 16-18 must be used in the calculation 
#
#Scenario: As a Finance Officer I want to know the funding line type for earnings So that I can estimate the correct forecasted funding (19+)
#	Given the apprenticeship learner is 19 plus at the start of the apprenticeship
#	When the apprenticeship commitment is approved 
#	Then the funding line type 19 plus must be used in the calculation 
#
#Scenario: Non Pilot Apprenticeship
#	Given An apprenticeship not on the pilot has been created as part of the approvals journey
#	Then Earnings are not generated for that apprenticeship