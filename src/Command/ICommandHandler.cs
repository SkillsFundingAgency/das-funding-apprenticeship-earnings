namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface ICommand
{

}

public interface ICommandHandler<in T> where T : ICommand
{
    Task Handle(T command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in T, TResult> where T : ICommand
{
    Task<TResult> Handle(T command, CancellationToken cancellationToken = default);
}
