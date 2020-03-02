using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SimpleIoC
{
    public sealed class SimpleContainer
    {
        private readonly Dictionary<string, RegisteredDependency> dependencyDictionary = new Dictionary<string, RegisteredDependency>();
        private ContainerScope currentScope = new ContainerScope();


        #region Scoped Dependency

        public IContainerScope BeginScope()
        {
            currentScope = ContainerScope.BeginScope(currentScope, scope => currentScope = scope);
            return currentScope;
        }

        #endregion

        #region Register Dependencies

        #region Transient

        public void RegisterTransient<T>() where T : class => RegisterTransient<T>(null, null);
        public void RegisterTransient<T>(Func<T> factory = null) where T : class => RegisterTransient<T>(null, factory);
        public void RegisterTransient<T>(string name = null, Func<T> factory = null) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), Lifestyle.Transient, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(T).Name, dependency);
        }

        public void RegisterTransient<TInterface, TImplementation>() where TImplementation : class, TInterface => RegisterTransient<TInterface, TImplementation>(null, null);
        public void RegisterTransient<TInterface, TImplementation>(Func<TInterface> factory = null) where TImplementation : class, TInterface => RegisterTransient<TInterface, TImplementation>(null, factory);
        public void RegisterTransient<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Transient, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        #endregion

        #region Instance

        public void RegisterInstance<T>(T singletonInstance) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), singletonInstance);
            dependencyDictionary.Add(typeof(T).Name, dependency);
        }

        #endregion

        #region Singleton

        public void RegisterSingleton<T>() where T : class => RegisterSingleton<T>(null, null);
        public void RegisterSingleton<T>(Func<T> factory = null) where T : class => RegisterSingleton<T>(null, factory);
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

        public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface => RegisterSingleton<TInterface, TImplementation>(null, null);
        public void RegisterSingleton<TInterface, TImplementation>(Func<TInterface> factory = null) where TImplementation : class, TInterface => RegisterSingleton<TInterface, TImplementation>(null, factory);
        public void RegisterSingleton<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Singleton, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }


        #endregion

        #region Scoped

        public void RegisterScoped<T>() where T : class => RegisterScoped<T>(null,null);
        public void RegisterScoped<T>(Func<T> factory = null) where T : class => RegisterScoped<T>(null, factory);
        public void RegisterScoped<T>(string name = null, Func<T> factory = null) where T : class
        {
            var dependency = new RegisteredDependency(typeof(T), Lifestyle.Scoped, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(T).Name, dependency);
        }

        public void RegisterScoped<TInterface, TImplementation>() where TImplementation : class, TInterface => RegisterScoped<TInterface, TImplementation>(null, null);
        public void RegisterScoped<TInterface, TImplementation>(Func<TInterface> factory = null) where TImplementation : class, TInterface => RegisterScoped<TInterface, TImplementation>(null, factory);
        public void RegisterScoped<TInterface, TImplementation>(string name = null, Func<TInterface> factory = null) where TImplementation : class, TInterface
        {
            var dependency = new RegisteredDependency(typeof(TImplementation), typeof(TInterface), Lifestyle.Scoped, factory as Func<object>);
            dependencyDictionary.Add(name ?? typeof(TInterface).Name + typeof(TImplementation).Name, dependency);
        }

        #endregion

        #endregion

        #region Resolve

        public void Build()
        {
            var failedDependencies = CheckForCircularDependency();

            if (failedDependencies.Any())
            {
                throw new Exception($"Failed to resolve the following types: {Environment.NewLine}{string.Join(Environment.NewLine, failedDependencies.Select(x => x.ToString()))}{Environment.NewLine}");
            }
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            var registeredDependencies = dependencyDictionary.Values.Where(x => x.InterfaceType == typeof(T));

            foreach (var registeredDependency in registeredDependencies)
                yield return (T)ResolveRegisteredDependency(typeof(T).Name, registeredDependency);
        }

        public T Resolve<T>()
        {
            T resolvedItem = (T)ResolveDependency(typeof(T));

            if (resolvedItem is null)
                throw new Exception($"Could not resolve an item of type {typeof(T)}");

            return resolvedItem;
        }

        public T Resolve<T>(string name)
        {
            T resolvedItem = (T)ResolveNamed(name);

            if (resolvedItem is null)
                throw new Exception($"Could not resolve an item of type: <{typeof(T)}> with name: <{name}>");

            return resolvedItem;
        }

        public object Resolve(Type type)
        {
            var resolvedItem = ResolveDependency(type);

            if (resolvedItem is null)
                throw new Exception($"Could not resolve an item of type {type}");

            return resolvedItem;
        }

        #endregion

        #region Resolve Logic

        private object ResolveDependency(Type type)
        {
            var registeredDependency = dependencyDictionary.Values.FirstOrDefault(x => x.InterfaceType == type);
            return registeredDependency == null ? null : ResolveRegisteredDependency(type.Name, registeredDependency);
        }

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

                registeredDependency.SingletonObject = constructedObject;
            }

            return registeredDependency.SingletonObject;
        }

        private object ConstructObject(Type implementationType)
        {
            var constructor = implementationType.GetConstructors().OrderBy(x => x.GetParameters().Length).First();
            
            var constructorArguments = constructor.GetParameters().Select(x => ResolveDependency(x.ParameterType)).ToArray();
            
            var constructedObject = constructorArguments.Any() ?
                Activator.CreateInstance(implementationType, constructorArguments) :
                Activator.CreateInstance(implementationType);

            return constructedObject;
        }

        #endregion

        #region CheckFirCircularDependencies

        private static Type checkingType = null;
        private List<ResolveFailed> CheckForCircularDependency()
        {
            var registeredTypesWithConstructorParametersMoreThanOne
                = dependencyDictionary
                    .Values
                    .Select(x => (Type: x.InterfaceType, Constructor: x.ImplementationType.GetConstructors().OrderBy(x => x.GetParameters().Length).First()))
                    .Where(x => x.Constructor.GetParameters().Length > 0)
                    .ToList();

            var failedToResolveTypes = new List<ResolveFailed>();

            foreach (var typeToCheckForMisconfiguredRegistration in registeredTypesWithConstructorParametersMoreThanOne)
            {
                checkingType = typeToCheckForMisconfiguredRegistration.Type;

                object obj = null;
                try
                {
                    obj = ResolveDependency_Config(checkingType);

                    if (obj is null)
                        failedToResolveTypes.Add(new ResolveFailed() { Type = checkingType, Reason = $"Could not construct type: {checkingType}" });
                }
                catch (Exception e)
                {
                    failedToResolveTypes.Add(new ResolveFailed() { Type = checkingType, Reason = e.Message });
                }
            }

            return failedToResolveTypes;
        }


        private object ResolveDependency_Config(Type type)
        {
            var registeredDependency = dependencyDictionary.Values.FirstOrDefault(x => x.InterfaceType == type);
            return registeredDependency == null ? null : ResolveRegisteredDependency_Config(type.Name, registeredDependency);
        }

        private object ResolveRegisteredDependency_Config(string name, RegisteredDependency registeredDependency) => registeredDependency.Lifestyle switch
        {
            Lifestyle.Singleton => ResolveSingleton_Config(registeredDependency),
            Lifestyle.Transient => ResolveTransient_Config(registeredDependency),
            Lifestyle.Scoped => ResolveScoped_Config(name, registeredDependency),
            _ => throw new ArgumentOutOfRangeException("Received not supported lifestyle")
        };

        private object ResolveScoped_Config(string name, RegisteredDependency registeredDependency)
        {
            var scopedResolvedObject = currentScope.GetResolvedScopedObject(name);
            if (scopedResolvedObject == null)
            {
                scopedResolvedObject = registeredDependency.HasFactory ?
                registeredDependency.FactoryConstruct() :
                ConstructObject_Config(registeredDependency.ImplementationType);

                currentScope.SetResovedScopedObject(name, scopedResolvedObject);
            }

            return scopedResolvedObject;
        }

        private object ResolveTransient_Config(RegisteredDependency registeredDependency)
        {
            return registeredDependency.HasFactory ?
                registeredDependency.FactoryConstruct() :
                ConstructObject_Config(registeredDependency.ImplementationType);
        }

        private object ResolveSingleton_Config(RegisteredDependency registeredDependency)
        {
            if (registeredDependency.SingletonObject is null)
            {
                var constructedObject = registeredDependency.HasFactory ?
                    registeredDependency.FactoryConstruct() :
                    ConstructObject_Config(registeredDependency.ImplementationType);

                registeredDependency.SingletonObject = constructedObject;
            }

            return registeredDependency.SingletonObject;
        }

        private object ConstructObject_Config(Type implementationType)
        {
            if (implementationType == checkingType) throw new Exception($"Reached circular dependency");

            var constructor = implementationType.GetConstructors().OrderBy(x => x.GetParameters().Length).First();

            var constructorArguments = constructor.GetParameters().Select(x => ResolveDependency_Config(x.ParameterType)).ToArray();

            var constructedObject = constructorArguments.Any() ?
                Activator.CreateInstance(implementationType, constructorArguments) :
                Activator.CreateInstance(implementationType);

            return constructedObject;
        }

        private class ResolveFailed
        {
            public string Reason { get; set; }
            public Type Type { get; set; }


            public override string ToString()
            {
                return $"Could not resolve {nameof(Type)}: {Type} - {nameof(Reason)}: {Reason}";
            }
        }

        #endregion
    }
}