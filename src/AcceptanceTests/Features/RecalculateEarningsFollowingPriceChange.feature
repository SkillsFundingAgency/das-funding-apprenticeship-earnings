Feature: Recalculate earnings following price change

Scenario: Price change approved in the year it was requested, below or at funding band max; recalc earnings
	Given an apprenticeship has been created
	And the total price is below or at the funding band maximum
	And the earnings for the apprenticeship are calculated
	And a price change request was sent before the end of R14 of the current academic year
	And the price change request is for a new total price up to or at the funding band maximum
	When the price change is approved by the other party before the end of year
	Then the earnings are recalculated based on the new price
	And the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation
	And the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments
	And a new earnings profile id is set
	And the history of old and new earnings is maintained


Scenario: Price change approved in the year it was requested, above funding band max; recalc earnings
	Given an apprenticeship has been created
	And the earnings for the apprenticeship are calculated
	And a price change request was sent before the end of R14 of the current academic year
	And the price change request is for a new total price above the funding band maximum
	When the price change is approved by the other party before the end of year
	Then the earnings are recalculated based on the funding band maximum
	And the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation
	And the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments
	And a new earnings profile id is set
	And the history of old and new earnings is maintained