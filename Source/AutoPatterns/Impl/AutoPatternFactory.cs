using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace AutoPatterns.Impl
{
    public sealed class AutoPatternFactory
    {
        private readonly object _syncRoot = new object();
        private readonly AutoPatternLibrary _library;
        private readonly string _namespaceName; //=> this.GetType().Name.TrimSuffix("Factory");
        private readonly Func<TypeKey, string> _onGetClassName;
        private readonly Action<TypeKey, Type> _onTypeBound;
        private ImmutableDictionary<TypeKey, TypeEntry> _typeEntryByKey;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternFactory(
            AutoPatternLibrary library, 
            string namespaceName, 
            Func<TypeKey, string> onGetClassName = null, 
            Action<TypeKey, Type> onTypeBound = null)
        {
            _library = library;
            _namespaceName = namespaceName;
            _onGetClassName = onGetClassName ?? (key => key.ToString());
            _onTypeBound = onTypeBound;
            _typeEntryByKey = ImmutableDictionary.Create<TypeKey, TypeEntry>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance(TypeKey key, int constructorIndex)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1>(TypeKey key, int constructorIndex, T1 arg1)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3, T4>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, T4, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3, arg4);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3, T4, T5>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, T4, T5, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3, arg4, arg5);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3, T4, T5, T6>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, T4, T5, T6, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3, T4, T5, T6, T7>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, T4, T5, T6, T7, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object CreateInstance<T1, T2, T3, T4, T5, T6, T7, T8>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            var typeEntry = GetOrAddTypeEntry(key);
            var factoryMethod = (Func<T1, T2, T3, T4, T5, T6, T7, T8, object>)typeEntry.FactoryMethods[constructorIndex];
            return factoryMethod(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetClassName(TypeKey key)
        {
            return _onGetClassName(key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName => _namespaceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeEntry GetOrAddTypeEntry(TypeKey key)
        {
            TypeEntry entry;

            // ReSharper disable once InconsistentlySynchronizedField
            if (!_typeEntryByKey.TryGetValue(key, out entry))
            {
                lock (_syncRoot)
                {
                    if (!_typeEntryByKey.TryGetValue(key, out entry))
                    {
                        var type = GetTypeFromLibraryOrThrow(key);
                        entry = new TypeEntry(key, type.GetTypeInfo());
                        _typeEntryByKey = _typeEntryByKey.Add(key, entry);

                        _onTypeBound?.Invoke(entry.Key, entry.Type);
                    }
                }
            }

            return entry;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetTypeFromLibraryOrThrow(TypeKey key)
        {
            var type = TryGetTypeFromLibrary(key);

            if (type != null)
            {
                return type;
            }

            _library.CompileAddedSyntaxes();
            type = TryGetTypeFromLibrary(key);

            if (type != null)
            {
                return type;
            }

            throw new AggregateException($"Type '{_namespaceName}.{GetClassName(key)}' cannot be found in any of library assemblies.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type TryGetTypeFromLibrary(TypeKey key)
        {
            var assemblies = _library.Assemblies;

            for (int i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var typeString = (
                    string.IsNullOrEmpty(_namespaceName) ?
                    $"{GetClassName(key)}" :
                    $"{_namespaceName}.{GetClassName(key)}");
                var type = assembly.GetType(typeString);

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TypeEntry
        {
            internal TypeEntry(TypeKey key, TypeInfo type)
            {
                this.Key = key;
                this.Type = type;
                this.FactoryMethods = DiscoverFactoryMethods(type);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey Key { get; private set; }
            public TypeInfo Type { get; private set; }
            public IReadOnlyList<Delegate> FactoryMethods { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static IReadOnlyList<Delegate> DiscoverFactoryMethods(TypeInfo type)
            {
                var methods = type.DeclaredMethods.Where(IsFactoryMethod).OrderBy(GetFactoryMethodIndex).ToArray();
                var delegates = new Delegate[methods.Length];

                for (int index = 0 ; index < delegates.Length ; index++)
                {
                    var method = methods[index];
                    var delegateType = CreateDelegateType(method.GetParameters());

                    delegates[index] = method.CreateDelegate(delegateType);
                }

                return delegates;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static bool IsFactoryMethod(MethodInfo method)
            {
                return (
                    method.IsStatic && 
                    method.IsPublic && 
                    method.Name.StartsWith(AutoPatternCompiler.FactoryMethodNamePrefix));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static int GetFactoryMethodIndex(MethodInfo method)
            {
                var suffix = method.Name.Substring(AutoPatternCompiler.FactoryMethodNamePrefix.Length);
                int index;
                if (Int32.TryParse(suffix, out index))
                {
                    return index;
                }

                throw new ArgumentException($"Invalid factory method name: '{method.Name}'.", nameof(method));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static Type CreateDelegateType(ParameterInfo[] parameters)
            {
                switch (parameters.Length)
                {
                    case 0:
                        return typeof(Func<object>);
                    case 1:
                        return typeof(Func<,>).MakeGenericType(parameters[0].ParameterType, typeof(object));
                    case 2:
                        return typeof(Func<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, typeof(object));
                    case 3:
                        return typeof(Func<,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            typeof(object));
                    case 4:
                        return typeof(Func<,,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            parameters[3].ParameterType,
                            typeof(object));
                    case 5:
                        return typeof(Func<,,,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            parameters[3].ParameterType,
                            parameters[4].ParameterType,
                            typeof(object));
                    case 6:
                        return typeof(Func<,,,,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            parameters[3].ParameterType,
                            parameters[4].ParameterType,
                            parameters[5].ParameterType,
                            typeof(object));
                    case 7:
                        return typeof(Func<,,,,,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            parameters[3].ParameterType,
                            parameters[4].ParameterType,
                            parameters[5].ParameterType,
                            parameters[6].ParameterType,
                            typeof(object));
                    case 8:
                        return typeof(Func<,,,,,,,,>).MakeGenericType(
                            parameters[0].ParameterType,
                            parameters[1].ParameterType,
                            parameters[2].ParameterType,
                            parameters[3].ParameterType,
                            parameters[4].ParameterType,
                            parameters[5].ParameterType,
                            parameters[6].ParameterType,
                            parameters[7].ParameterType,
                            typeof(object));
                }

                throw new NotSupportedException("Constructors with more than 8 parameters are not supproted.");
            }
        }
    }
}
