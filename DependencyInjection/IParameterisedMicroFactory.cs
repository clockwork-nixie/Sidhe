using JetBrains.Annotations;

namespace DependencyInjection
{
    [UsedImplicitly]
    public interface IParameterisedMicroFactory<out TAbstract, in TArgument> where TAbstract : class
    {
        [NotNull, UsedImplicitly] TAbstract GetInstance(TArgument argument);
    }


    [UsedImplicitly]
    public interface IParameterisedMicroFactory<out TAbstract, in TArgument1, in TArgument2> where TAbstract : class
    {
        [NotNull, UsedImplicitly] TAbstract GetInstance(TArgument1 argument1, TArgument2 argument2);
    }


    [UsedImplicitly]
    public interface IParameterisedMicroFactory<out TAbstract, in TArgument1, in TArgument2, in TArgument3> where TAbstract : class
    {
        [NotNull, UsedImplicitly] TAbstract GetInstance(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3);
    }
}
