## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# das-funding-apprenticeships-earnings

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">


[![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/das-funding-apprenticeships-earnings?branchName=master)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=2856&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-funding-apprenticeship-earnings&metric=alert_status)](https://sonarcloud.io/dashboard?id=SkillsFundingAgency_das-funding-apprenticeship-earnings)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/jira/software/c/projects/FLP/boards/753)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3480354918/Flexible+Payments+Models)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

das-funding-apprenticeships-earnings is an Azure function which subscribes and publishes various events. Its purpose is to store and manage earnings records raised by the creation of new apprenticeships and updated via change of circumstances events.

## How It Works

There are 2 applications within this repository, the Azure Function and an InnerApi.
The Function handles events, and the InnerApi responds to queries and also has some synchronous api endpoints.
Data is stored in its own database (MS Sql Server).

## 🚀 Installation

### Pre-Requisites

* A clone of this repository
* A code editor that supports .Net8
* Azure Storage Emulator (Azureite)
* Sql Server DB
* a instance of Azure Service bus you can use for local development

### Config

Most of the application configuration is taken from the [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config) and the default values can be used in most cases.  The config json will need to be added to the local Azure Storage instance with a a PartitionKey of LOCAL and a RowKey of SFA.DAS.Funding.ApprenticeshipEarnings_1.0.


# local.settings.json
```
{
  "IsEncrypted": false,
  "AzureWebJobsStorage": "UseDevelopmentStorage=true",
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsServiceBus__fullyQualifiedNamespace": "<your service bus instance>",
    "EnvironmentName": "LOCAL",
    "NServiceBusConnectionString": "UseLearningEndpoint=true",
    "ConfigNames": "SFA.DAS.Funding.ApprenticeshipEarnings",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

## 🔗 External Dependencies

* Azure Storage Emulator (Azureite)
* Sql Server DB
* a instance of Azure Service bus you can use for local development

The innerApi can be queried directly via a tool such as postman. The azure function requires an instance of Azure Service bus for local development to handle the events.
