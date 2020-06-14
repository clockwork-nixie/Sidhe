using JetBrains.Annotations;

namespace Sidhe.Utilities.Interfaces
{
    public interface IDispatcher<in TRequest>
    {
        void Dispatch([NotNull] TRequest model);
    }


    public interface IDispatcher<in TRequest, out TResponse>
    {
        TResponse Dispatch([NotNull] TRequest model);
    }
}
