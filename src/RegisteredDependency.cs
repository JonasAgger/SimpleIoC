using System;

namespace SimpleIoC
{
    internal class RegisteredDependency
    {
        public Type ImplementationType { get; private set; }
        public Type InterfaceType { get; private set; }
        public Lifestyle Lifestyle { get; private set; }
        public object SingletonObject { get; private set; } = null;
        public bool HasFactory => !(factory is null);
        public object FactoryConstruct() => factory();

        private readonly Func<object> factory = null;

        // Default Constructor. 
        public RegisteredDependency(Type implementationType, Type interfaceType, Lifestyle lifestyle)
        {
            this.ImplementationType = implementationType;
            this.InterfaceType = interfaceType;
            this.Lifestyle = lifestyle;
        }

        #region Derived Constructors
        // Derived constructors..
        public RegisteredDependency(Type implementationType, Lifestyle lifestyle) : this(implementationType, implementationType, lifestyle)
        {  }

        public RegisteredDependency(Type implementationType, object singletonObject) : this(implementationType, implementationType, Lifestyle.Singleton)
        {
            this.SingletonObject = singletonObject;
        }

        public RegisteredDependency(Type implementationType, Type interfaceType, object singletonObject) : this(implementationType, interfaceType, Lifestyle.Singleton)
        {
            this.SingletonObject = singletonObject;
        }

        public RegisteredDependency(Type implementationType, Lifestyle lifestyle, Func<object> factory) : this(implementationType, lifestyle)
        {
            this.factory = factory;
        }

        public RegisteredDependency(Type implementationType, Type interfaceType, Lifestyle lifestyle, Func<object> factory) : this(implementationType, interfaceType, lifestyle)
        {
            this.factory = factory;
        }
        
        #endregion
    }
}
