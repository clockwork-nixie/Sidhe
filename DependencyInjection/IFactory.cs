using JetBrains.Annotations;

namespace DependencyInjection
{
    public interface IFactory
    {
        [NotNull] TAbstract GetInstance<TAbstract>() where TAbstract : class;
        [NotNull] TAbstract GetInstance<TAbstract, TArgument>(TArgument argument) where TAbstract : class;
        [NotNull] TAbstract GetInstance<TAbstract, TArgument1, TArgument2>(TArgument1 argument1, TArgument2 argument2) where TAbstract : class;
        [NotNull] TAbstract GetInstance<TAbstract, TArgument1, TArgument2, TArgument3>(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) where TAbstract : class;
    }
}
