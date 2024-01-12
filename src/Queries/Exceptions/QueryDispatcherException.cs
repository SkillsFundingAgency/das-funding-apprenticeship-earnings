using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public sealed class QueryDispatcherException : Exception
{
    public QueryDispatcherException()
    {
    }

    public QueryDispatcherException(string message)
        : base(message)
    {
    }

    public QueryDispatcherException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}