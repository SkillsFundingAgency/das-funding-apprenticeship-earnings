Feature: Recalculate earnings following start date change

Scenario: New start date earlier than current date and in the same current academic year
	Given earnings have been calculated for an apprenticeship in the pilot
	And a start date change request was sent before the end of R14 of the current academic year
	And the new start date is earlier than, and in the same academic year, as the current start date
	When the start date change is approved
	Then the earnings are recalculated based on the new start date
	And the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments
	And a new earnings profile id is set
	And the history of old and new earnings is maintained

Scenario: New start date later than current date and in the same current academic year
	Given earnings have been calculated for an apprenticeship in the pilot
	And a start date change request was sent before the end of R14 of the current academic year
	And the new start date is later than, and in the same academic year, as the current start date
	When the start date change is approved
	Then the earnings are recalculated based on the new start date
	And the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments
	And a new earnings profile id is set
	And the history of old and new earnings is maintained

Scenario: New start date in the next academic year
	Given earnings have been calculated for an apprenticeship in the pilot
	And a start date change request was sent before the end of R14 of the current academic year
	And the new start date is in the next academic year to the current start date
	When the start date change is approved
	Then the earnings are recalculated based on the new start date
	And the number of instalments is determined by the number of census dates passed between the new start date and the planned end date of the apprenticeship
	And the amount of each instalment is determined as: totalPriceLessCompletion / newNumberOfInstalments
	And a new earnings profile id is set
	And the history of old and new earnings is maintained