
namespace Errordite.Core.Interfaces
{
    public interface IWorkflow<in TRequest, out TResponse> : IWantToBeProfiled, ICaptureMethodInfo
    {
        TResponse Invoke(TRequest request);
    }
}
