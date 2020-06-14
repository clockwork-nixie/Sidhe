using System;
using JetBrains.Annotations;

namespace DependencyInjection
{
    internal class ParameterisedMicroFactory<TAbstract, TArgument> : IParameterisedMicroFactory<TAbstract, TArgument>
        where TAbstract : class
    {
        [NotNull] private readonly Func<TArgument, TAbstract> _factoryMethod;


        public ParameterisedMicroFactory(Func<TArgument, TAbstract> factoryMethod) =>
            _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));


        public TAbstract GetInstance(TArgument argument)
        {
            var instance = _factoryMethod.Invoke(argument);

            if (instance == null)
            {
                throw new NullReferenceException($"Parameterised factory method for {typeof(TAbstract).Name} generated a null instance.");
            }

            return instance;
        }
    }


    internal class ParameterisedMicroFactory<TAbstract, TArgument1, TArgument2> : IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2>
        where TAbstract : class
    {
        [NotNull] private readonly Func<TArgument1, TArgument2, TAbstract> _factoryMethod;


        public ParameterisedMicroFactory(Func<TArgument1, TArgument2, TAbstract> factoryMethod) =>
            _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));


        public TAbstract GetInstance(TArgument1 argument1, TArgument2 argument2)
        {
            var instance = _factoryMethod.Invoke(argument1, argument2);

            if (instance == null)
            {
                throw new NullReferenceException($"Parameterised factory method for {typeof(TAbstract).Name} generated a null instance.");
            }

            return instance;
        }
    }

    
    internal class ParameterisedMicroFactory<TAbstract, TArgument1, TArgument2, TArgument3> : IParameterisedMicroFactory<TAbstract, TArgument1, TArgument2, TArgument3>
        where TAbstract : class
    {
        [NotNull] private readonly Func<TArgument1, TArgument2, TArgument3, TAbstract> _factoryMethod;


        public ParameterisedMicroFactory(Func<TArgument1, TArgument2, TArgument3, TAbstract> factoryMethod) =>
            _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));


        public TAbstract GetInstance(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            var instance = _factoryMethod.Invoke(argument1, argument2, argument3);

            if (instance == null)
            {
                throw new NullReferenceException($"Parameterised factory method for {typeof(TAbstract).Name} generated a null instance.");
            }

            return instance;
        }
    }
}