using System;
using JetBrains.Annotations;

namespace DependencyInjection
{
    public interface IFactoryBuilder
    {
        [UsedImplicitly, NotNull] IFactory Factory { get; }

        [UsedImplicitly, NotNull] IFactoryBuilder InterceptRegistrationFor<TClass>(Action<IContainerAdapter, Type, object> interceptor = null) where TClass : class;

        [UsedImplicitly, NotNull] IFactoryBuilder Register<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract;
        [UsedImplicitly, NotNull] IFactoryBuilder Register<TAbstract>([NotNull] Func<TAbstract> factoryMethod) where TAbstract : class;
        [UsedImplicitly, NotNull] IFactoryBuilder Register<TAbstract, TArgument>(Func<TArgument, TAbstract> factoryMethod) where TAbstract : class;
        [UsedImplicitly, NotNull] IFactoryBuilder Register<TAbstract, TArgument1, TArgument2>(Func<TArgument1, TArgument2, TAbstract> factoryMethod) where TAbstract : class;
        [UsedImplicitly, NotNull] IFactoryBuilder Register<TAbstract, TArgument1, TArgument2, TArgument3>(Func<TArgument1, TArgument2, TArgument3, TAbstract> factoryMethod) where TAbstract : class;
        [UsedImplicitly, NotNull] IFactoryBuilder RegisterInstance<TAbstract>([NotNull] TAbstract instance) where TAbstract : class;
        [UsedImplicitly, NotNull] IFactoryBuilder RegisterSingleton<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract;
        [UsedImplicitly, NotNull] IFactoryBuilder RegisterSingleton<TAbstract>([NotNull] Func<TAbstract> factoryMethod) where TAbstract : class;
    }
}