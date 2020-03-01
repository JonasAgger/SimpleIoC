using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIoC
{
    public sealed class SimpleContainer
    {
        private Dictionary<string, RegisteredDependency> dependencyDictionary = new Dictionary<string, RegisteredDependency>();
        private ContainerScope currentScope = new ContainerScope();

        #region Scoped Dependency

        public IContainerScope BeginScope()
        {
            currentScope = ContainerScope.BeginScope(currentScope, scope => currentScope = scope);
            return currentScope;
        }

        public void RegisterScoped<T>(string name = null, Func<T> factory = null) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), Lifestyle.Scoped, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(T).Name, dependency);
        }

        public void RegisterScoped<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Scoped, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        #endregion

        #region Register Dependencies

        public void RegisterTransient<T>(string name = null, Func<T> factory = null) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), Lifestyle.Transient, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(T).Name, dependency);
        }

        public void RegisterTransient<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Transient, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        public void RegisterInstance<T>(T singletonInstance) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), singletonInstance);
            dependencyDictionary.Add(typeof(T).Name, dependency);
        }

        public void RegisterSingleton<T>(string name = null, Func<T> factory = null) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), Lifestyle.Singleton, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(T).Name, dependency);
        }

        public void RegisterSingleton<TInterface, TImplementation>(TInterface singletonInstance, string name = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), singletonInstance);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        public void RegisterSingleton<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Singleton, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        #endregion

        #region Resolve

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            var registeredDependencies = dependencyDictionary.Values.Where(x => x.InterfaceType == typeof(T));

            foreach (var registeredDependency in registeredDependencies)
                yield return (T)ResolveRegisteredDependency(typeof(T).Name, registeredDependency);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public T Resolve<T>(string name)
        {
            return (T)ResolveNamed(name);
        }

        public object Resolve(Type type)
        {
            var registeredDependency = dependencyDictionary.Values.FirstOrDefault(x => x.InterfaceType == type);
            return registeredDependency == null ? null : ResolveRegisteredDependency(type.Name, registeredDependency);
        }

        #endregion

        #region Resolve Logic

        private object ResolveNamed(string name)
        {
            if (!dependencyDictionary.ContainsKey(name))
                return null;

            var registeredDependency = dependencyDictionary[name];
            return ResolveRegisteredDependency(name, registeredDependency);
        }

        private object ResolveRegisteredDependency(string name, RegisteredDependency registeredDependency) => registeredDependency.Lifestyle switch
        {
            Lifestyle.Singleton => ResolveSingleton(registeredDependency),
            Lifestyle.Transient => ResolveTransient(registeredDependency),
            Lifestyle.Scoped => ResolveScoped(name, registeredDependency),
            _ => throw new ArgumentOutOfRangeException("Received not supported lifestyle")
        };

        private object ResolveScoped(string name, RegisteredDependency registeredDependency)
        {
            var scopedResolvedObject = currentScope.GetResolvedScopedObject(name);
            if (scopedResolvedObject == null)
            {
                scopedResolvedObject = registeredDependency.HasFactory ?
                registeredDependency.FactoryConstruct() :
                ConstructObject(registeredDependency.ImplementationType);

                currentScope.SetResovedScopedObject(name, scopedResolvedObject);
            }

            return scopedResolvedObject;
        }

        private object ResolveTransient(RegisteredDependency registeredDependency)
        {
            return registeredDependency.HasFactory ?
                registeredDependency.FactoryConstruct() :
                ConstructObject(registeredDependency.ImplementationType);
        }

        private object ResolveSingleton(RegisteredDependency registeredDependency)
        {
            if (registeredDependency.SingletonObject is null)
            {
                var constructedObject = registeredDependency.HasFactory ?
                    registeredDependency.FactoryConstruct() :
                    ConstructObject(registeredDependency.ImplementationType);

                registeredDependency.singletonObject = constructedObject;
            }

            return registeredDependency.SingletonObject;
        }

        private object ConstructObject(Type implementationType)
        {
            var constructor = implementationType.GetConstructors().OrderBy(x => x.GetParameters().Length).First();
            
            var constructorArguments = constructor.GetParameters().Select(x => Resolve(x.ParameterType)).ToArray();

            var constructedObject = constructorArguments.Any() ?
                Activator.CreateInstance(implementationType, constructorArguments) :
                Activator.CreateInstance(implementationType);

            return constructedObject;
        }

        #endregion
    }
}