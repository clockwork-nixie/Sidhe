using System;
using JetBrains.Annotations;
using SimpleInjector;

namespace DependencyInjection
{
    internal class SimpleInjectorFactory : IFactory, IContainerAdapter
    {
        [NotNull] private readonly Container _container;


        public SimpleInjectorFactory()
        {
            _container = new Container();
        }


        public IFactory Factory => this;
        
        public TAbstract GetInstance<TAbstract>() where TAbstract : class => _container.GetInstance<TAbstract>();

        public TAbstract GetInstance<TAbstract, TArgument>(TArgument argument) where TAbstract : class 
            => GetInstance<IParameterisedMicroFactory<TAbstract, TArgument>>().GetInstance(argument);
        public TAbstract GetInstance<TAbstract, TArgument1, TArgument2>(TArgument1 argument1, TArgument2 argument2) where TAbstract : class 
            => GetInstance<IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2>>().GetInstance(argument1, argument2);
        public TAbstract GetInstance<TAbstract, TArgument1, TArgument2, TArgument3>(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) where TAbstract : class 
            => GetInstance<IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2, TArgument3>>().GetInstance(argument1, argument2, argument3);
        
        public void RegisterTransient<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract => _container.Register<TAbstract, TConcrete>();
        public void RegisterTransient<TAbstract>(Func<TAbstract> factoryMethod) where TAbstract : class => _container.Register(factoryMethod);
        public void RegisterInstance<TAbstract>(TAbstract instance) where TAbstract : class => _container.RegisterInstance(instance);
        public void RegisterSingleton<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract => _container.RegisterSingleton<TAbstract, TConcrete>();
        public void RegisterSingleton<TAbstract>(Func<TAbstract> factoryMethod) where TAbstract : class => _container.RegisterSingleton(factoryMethod);
    }
}
