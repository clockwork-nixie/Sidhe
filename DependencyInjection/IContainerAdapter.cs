using System;
using JetBrains.Annotations;

namespace DependencyInjection
{
    public interface IContainerAdapter
    {
        [NotNull] IFactory Factory { get; }

        void RegisterTransient<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract;
        void RegisterTransient<TAbstract>([NotNull] Func<TAbstract> factoryMethod) where TAbstract : class;
        void RegisterInstance<TAbstract>([NotNull] TAbstract instance) where TAbstract : class;
        void RegisterSingleton<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract;
        void RegisterSingleton<TAbstract>([NotNull] Func<TAbstract> factoryMethod) where TAbstract : class;
    }
}