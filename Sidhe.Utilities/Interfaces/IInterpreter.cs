using JetBrains.Annotations;

namespace Sidhe.Utilities.Interfaces
{
    public interface IInterpreter<in TInput, out TOutput> where TInput : class
    {
        TOutput Translate([NotNull] TInput input);
    }
}
