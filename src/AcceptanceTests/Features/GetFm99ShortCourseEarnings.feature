Feature: Get short course earnings via API

Scenario: Short course earnings are returned for a given learning key and ukprn
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-01-01 | 2021-06-25      |       2000 |
	When I request the short course earnings for year 2021
	Then the earnings response contains
		| CollectionYear | CollectionPeriod | Amount | Type                          |
		| 2021           | 7                | 600    | ThirtyPercentLearningComplete |
		| 2021           | 11               | 1400   | LearningComplete              |

Scenario: Only current year instalments are returned when short course spans two academic years (first year)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-07-01 | 2021-09-30      |       2000 |
	When I request the short course earnings for year 2021
	Then the earnings response contains
		| CollectionYear | CollectionPeriod | Amount | Type                          |
		|           2021 |               12 |    600 | ThirtyPercentLearningComplete |

Scenario: Only current year instalments are returned when short course spans two academic years (second year)
	Given a short course has been created with the following information
		| StartDate  | ExpectedEndDate | TotalPrice |
		| 2021-07-01 | 2021-09-30      |       2000 |
	When I request the short course earnings for year 2122
	Then the earnings response contains
		| CollectionYear | CollectionPeriod | Amount | Type             |
		| 2122           | 2                | 1400   | LearningComplete |