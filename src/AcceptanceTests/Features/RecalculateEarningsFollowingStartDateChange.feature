Feature: Recalculate earnings following the approval of a start date change



Scenario: Start date moves earlier, stays in-year
	Given earnings have been calculated for an apprenticeship in the pilot
	And a start date change request was sent before the end of R14 of the current academic year
	And the price change request is for a new start date is earlier than the current start date
	When the start date change is approved by the other party before the end of year
	Then the earnings are recalculated based on the new start date
	And the history of old and new earnings is maintained


Scenario: Price change approved in the year it was requested, above funding band max; recalc earnings
	Given earnings have been calculated for an apprenticeship in the pilot 
	And a price change request was sent before the end of R14 of the current academic year
	And the price change request is for a new total price above the funding band maximum
	When the start date change is approved by the other party before the end of year
	Then the earnings are recalculated based on the lower of: the new total price and the funding band maximum
	And the history of old and new earnings is maintained


Scenario: earnings and instalments calculation
	Given new earnings are to be calculated following a price change
	When the earnings are calculated
	Then the earnings prior to the effective-from date are 'frozen' and do not change as part of this calculation
	And the number of instalments is determined by the number of census dates passed between the effective-from date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: newPriceLessCompletion - earningsBeforeTheEffectiveFromDate / numberOfInstalments
	And a new earnings profile id is set