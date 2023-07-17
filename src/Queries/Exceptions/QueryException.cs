using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.Exceptions;
[ExcludeFromCodeCoverage]

[Serializable]
public sealed class QueryException : Exception
{
    public QueryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}