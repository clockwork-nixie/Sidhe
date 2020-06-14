using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DependencyInjection
{
    public class FactoryBuilder : IFactoryBuilder
    {
        [NotNull] private readonly IContainerAdapter _container;
        [NotNull] private readonly IDictionary<Type, Action<IContainerAdapter, Type, object>> _intercepts = new ConcurrentDictionary<Type, Action<IContainerAdapter, Type, object>>();


        public FactoryBuilder(IContainerAdapter container = null)
        {
            _container = container ?? new SimpleInjectorFactory();
            Factory = _container.Factory;
            RegisterInstance(Factory);
        }

        
        public IFactory Factory { get; }


        public IFactoryBuilder InterceptRegistrationFor<TClass>(
            Action<IContainerAdapter, Type, object> interceptor = null)
            where TClass : class
        {
            _intercepts[typeof(TClass)] = interceptor;

            return this;
        }


        public IFactoryBuilder Register<TAbstract, TConcrete>()
            where TAbstract : class
            where TConcrete : class, TAbstract
        {
            if (!_intercepts.TryGetValue(typeof(TAbstract), out var interceptor))
            {
                _container.RegisterTransient<TAbstract, TConcrete>();
            }
            else
            {
                interceptor?.Invoke(_container, typeof(TAbstract), typeof(TConcrete));
            }

            return this;
        }


        public IFactoryBuilder Register<TAbstract>(Func<TAbstract> factoryMethod)
            where TAbstract : class
        {
            if (_intercepts.TryGetValue(typeof(TAbstract), out var interceptor))
            {
                interceptor?.Invoke(_container, typeof(TAbstract), factoryMethod);
            }
            else
            {
                _container.RegisterTransient(factoryMethod);
            }

            return this;
        }


        public IFactoryBuilder Register<TAbstract, TArgument>(Func<TArgument, TAbstract> factoryMethod) 
            where TAbstract : class
        {
            RegisterInstance<IParameterisedMicroFactory<TAbstract, TArgument>>(
                new ParameterisedMicroFactory<TAbstract, TArgument>(factoryMethod));

            return this;
        }


        public IFactoryBuilder Register<TAbstract, TArgument1, TArgument2>(Func<TArgument1, TArgument2, TAbstract> factoryMethod) 
            where TAbstract : class
        {
            RegisterInstance<IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2>>(
                new ParameterisedMicroFactory<TAbstract, TArgument1, TArgument2>(factoryMethod));

            return this;
        }


        public IFactoryBuilder Register<TAbstract, TArgument1, TArgument2, TArgument3>(Func<TArgument1, TArgument2, TArgument3, TAbstract> factoryMethod) 
            where TAbstract : class
        {
            RegisterInstance<IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2, TArgument3>>(
                new ParameterisedMicroFactory<TAbstract, TArgument1, TArgument2, TArgument3>(factoryMethod));

            return this;
        }


        public IFactoryBuilder RegisterInstance<TAbstract>(TAbstract instance)
            where TAbstract : class
        {
            if (_intercepts.TryGetValue(typeof(TAbstract), out var interceptor))
            {
                interceptor?.Invoke(_container, typeof(TAbstract), instance);
            }
            else
            {
                _container.RegisterInstance(instance);
            }

            return this;
        }


        public IFactoryBuilder RegisterSingleton<TAbstract, TConcrete>() where TAbstract : class where TConcrete : class, TAbstract
        {
            if (_intercepts.TryGetValue(typeof(TAbstract), out var interceptor))
            {
                interceptor?.Invoke(_container, typeof(TAbstract), typeof(TConcrete));
            }
            else
            {
                _container.RegisterSingleton<TAbstract, TConcrete>();
            }

            return this;
        }


        public IFactoryBuilder RegisterSingleton<TAbstract>(Func<TAbstract> factoryMethod)
            where TAbstract : class
        {
            if (_intercepts.TryGetValue(typeof(TAbstract), out var interceptor))
            {
                interceptor?.Invoke(_container, typeof(TAbstract), factoryMethod);
            }
            else
            {
                _container.RegisterSingleton(factoryMethod);
            }

            return this;
        }
    }
}
