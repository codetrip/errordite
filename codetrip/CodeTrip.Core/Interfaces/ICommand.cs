namespace CodeTrip.Core.Interfaces
{
    public interface ICommand<in TRequest, out TResponse> : IWorkflow<TRequest, TResponse>
    {}
}