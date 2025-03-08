using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Architecture
{
    public abstract class Injector<Target> where Target : class
    {
        private const BindingFlags KBindingFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<Type, object> registry = new();

        /// <summary>
        /// Collect class instances, which can be used for provide-inject cycle
        /// </summary>
        /// <returns> Array of Targets </returns>
        protected abstract Target[] CollectTargets();

        /// <summary>
        /// Loop throw the targets, register providers and inject all dependencies
        /// </summary>
        public void Setup()
        {
            var targets = CollectTargets();

            // Find all modules implementing IDependencyProvider and register the dependencies they provide
            var providers = targets.OfType<IDependencyProvider>();
            foreach (var provider in providers) Register(provider);

            // Find all injectable objects and inject their dependencies
            var injectables = targets.Where(IsInjectable);
            foreach (var injectable in injectables) Inject(injectable);
        }

        /// <summary>
        /// Register an instance of a type outside the normal dependency injection process
        /// </summary>
        public void Register<T>(T instance)
        {
            registry[typeof(T)] = instance;
        }

        /// <summary>
        /// Set all methods, properties and fields under the InjectAttribute to NULL
        /// </summary>
        public void NullifyDependencies()
        {
            foreach (var target in CollectTargets())
            {
                var type = target.GetType();
                var injectableFields = type.GetFields(KBindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (var injectableField in injectableFields) injectableField.SetValue(target, null);
            }
        }

        private void Inject(object instance)
        {
            var type = instance.GetType();

            // Inject into fields
            var injectableFields = type.GetFields(KBindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableField in injectableFields)
            {
                if (injectableField.GetValue(instance) != null) continue;
                var fieldType = injectableField.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                    throw new Exception(
                        $"Failed to inject dependency into field '{injectableField.Name}' of class '{type.Name}'.");

                injectableField.SetValue(instance, resolvedInstance);
            }

            // Inject into methods
            var injectableMethods = type.GetMethods(KBindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (var injectableMethod in injectableMethods)
            {
                var requiredParameters = injectableMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                var resolvedInstances = requiredParameters.Select(Resolve).ToArray();
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null))
                    throw new Exception(
                        $"Failed to inject dependencies into method '{injectableMethod.Name}' of class '{type.Name}'.");

                injectableMethod.Invoke(instance, resolvedInstances);
            }

            // Inject into properties
            var injectableProperties = type.GetProperties(KBindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
            foreach (var injectableProperty in injectableProperties)
            {
                var propertyType = injectableProperty.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                    throw new Exception(
                        $"Failed to inject dependency into property '{injectableProperty.Name}' of class '{type.Name}'.");

                injectableProperty.SetValue(instance, resolvedInstance);
            }
        }

        private void Register(IDependencyProvider provider)
        {
            var methods = provider.GetType().GetMethods(KBindingFlags);

            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                var returnType = method.ReturnType;
                var providedInstance = method.Invoke(provider, null);
                if (providedInstance != null)
                    registry.Add(returnType, providedInstance);
                else
                    throw new Exception(
                        $"Provider method '{method.Name}' in class '{provider.GetType().Name}' returned null when providing type '{returnType.Name}'.");
            }
        }

        private void ValidateDependencies()
        {
            var targets = CollectTargets();
            var providers = targets.OfType<IDependencyProvider>();
            var providedDependencies = GetProvidedDependencies(providers);

            var invalidDependencies = targets
                .SelectMany(mb => mb.GetType().GetFields(KBindingFlags), (mb, field) => new { mb, field })
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name}");

            var invalidDependencyList = invalidDependencies.ToList();

            if (invalidDependencyList.Any())
                throw new Exception($"Invalid dependency list: {string.Join(", ", invalidDependencyList)}");
        }

        private HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers)
        {
            var providedDependencies = new HashSet<Type>();
            foreach (var provider in providers)
            {
                var methods = provider.GetType().GetMethods(KBindingFlags);

                foreach (var method in methods)
                {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                    var returnType = method.ReturnType;
                    providedDependencies.Add(returnType);
                }
            }

            return providedDependencies;
        }

        private object Resolve(Type type)
        {
            registry.TryGetValue(type, out var resolvedInstance);
            return resolvedInstance;
        }

        private bool IsInjectable(Target target)
        {
            var members = target.GetType().GetMembers(KBindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }
    }
}