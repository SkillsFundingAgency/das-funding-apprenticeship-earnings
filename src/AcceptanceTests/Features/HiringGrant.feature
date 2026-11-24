Feature: Hiring grant

Employer hiring grant earnings are generated on approval for 16-24 year old learners of non-levy employers
starting on or after 1 October 2026, whether or not the provider is opted in. They are earnings only.

Scenario: Hiring grant is generated for an approved 16-24 learner of a non-levy employer
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2026-10-01 | 2028-09-30 | 12000      | 22  |
	And the employer is non-levy
	When a LearningApproved event is received for the apprenticeship
	Then Additional Payments are persisted as follows
		| Type                | Amount | DueDate    |
		| EmployerHiringGrant | 1000   | 2026-12-29 |
		| EmployerHiringGrant | 1000   | 2027-09-30 |

Scenario: Hiring grant is generated for an approved learner when the provider has not opted in
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age | Ukprn    |
		| 2026-10-01 | 2028-09-30 | 12000      | 22  | 99999999 |
	And the employer is non-levy
	When a LearningApproved event is received for the apprenticeship
	Then Additional Payments are persisted as follows
		| Type                | Amount | DueDate    |
		| EmployerHiringGrant | 1000   | 2026-12-29 |
		| EmployerHiringGrant | 1000   | 2027-09-30 |

Scenario: Hiring grant is not generated for a levy employer
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2026-10-01 | 2028-09-30 | 12000      | 22  |
	When a LearningApproved event is received for the apprenticeship
	Then Additional Payments are persisted as follows
		| Type | Amount | DueDate |

Scenario: Hiring grant is not generated for a learner starting before 1 October 2026
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2026-09-30 | 2028-09-29 | 12000      | 22  |
	And the employer is non-levy
	When a LearningApproved event is received for the apprenticeship
	Then Additional Payments are persisted as follows
		| Type | Amount | DueDate |

Scenario: Hiring grant is not generated for a learner aged 25 or over
	Given an apprenticeship has been created as a draft with the following information
		| StartDate  | EndDate    | TotalPrice | Age |
		| 2026-10-01 | 2028-09-30 | 12000      | 25  |
	And the employer is non-levy
	When a LearningApproved event is received for the apprenticeship
	Then Additional Payments are persisted as follows
		| Type | Amount | DueDate |
