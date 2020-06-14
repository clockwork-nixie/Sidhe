using System;
using JetBrains.Annotations;

namespace DependencyInjection
{
    public abstract class Dependency : IFactory
    {
        protected Dependency([NotNull] IFactory factory) => Factory = factory ?? throw new ArgumentNullException(nameof(factory));

        [NotNull] protected IFactory Factory { get; }

        public TAbstract GetInstance<TAbstract>() where TAbstract : class => Factory.GetInstance<TAbstract>();

        public TAbstract GetInstance<TAbstract, TArgument>(TArgument argument) where TAbstract : class
            => Factory.GetInstance<TAbstract, TArgument>(argument);

        public TAbstract GetInstance<TAbstract, TArgument1, TArgument2>(TArgument1 argument1, TArgument2 argument2) where TAbstract : class 
            => Factory.GetInstance<TAbstract, TArgument1, TArgument2>(argument1, argument2);

        public TAbstract GetInstance<TAbstract, TArgument1, TArgument2, TArgument3>(TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) where TAbstract : class 
            => Factory.GetInstance<TAbstract, TArgument1, TArgument2, TArgument3>(argument1, argument2, argument3);
    }
}
