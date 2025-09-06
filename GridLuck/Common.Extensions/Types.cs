using System.Collections.Immutable;
using System.Reflection;

namespace GridLuck.Common.Extensions
{
    public static class Types
    {
        #region Attributes

        public static bool HasAttribute(
            this Type type,
            Type attributeType)
            => type.GetCustomAttribute(attributeType) is not null;

        public static bool HasAttribute<TAtt>(
            this Type type)
            where TAtt : Attribute
            => type.GetCustomAttribute<TAtt>() is not null;

        public static bool HasAttribute(
            this MemberInfo member,
            Type attributeType)
            => member.GetCustomAttribute(attributeType) is not null;

        public static bool HasAttribute<TAtt>(
            this MemberInfo member)
            where TAtt : Attribute
            => member.GetCustomAttribute<TAtt>() is not null;

        #endregion

        #region Inheritance
        /// <summary>
        /// Checks that the given instance extends any of the given types.
        /// <para/>
        /// TODO: Extend this method to cater for instances where generic type defs are given
        /// </summary>
        /// <param name="instanceType">The type to check</param>
        /// <param name="types">The base types</param>
        /// <returns></returns>
        public static bool ExtendsAny(this Type instanceType, params Type[] types)
        {
            ArgumentNullException.ThrowIfNull(instanceType);
            ArgumentNullException.ThrowIfNull(types);

            var baseType = instanceType.BaseType;

            // If there are no types to test against, only return true if the instance has no base type
            if (types.Length == 0)
                return baseType is null;

            return types
                .ToHashSet()
                .Contains(baseType!);
        }

        /// <summary>
        /// Checks that the given <paramref name="ancestor"/> is somewhere up the concrete-inheritance chain of <paramref name="type"/>
        /// <para/>
        /// TODO: Extend this method to cater for instances where generic type defs are given
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="ancestor">The potential ancestor</param>
        /// <returns>True if <paramref name="ancestor"/> is an ancestor of <paramref name="type"/></returns>
        public static bool IsDescendantOf(this Type type, Type ancestor)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(ancestor);

            while (type.BaseType is not null)
            {
                if (ancestor.Equals(type.BaseType))
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Checks that the given type implements any of the interfaces in the non-empty list '<paramref name="interfaces"/>'. 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="interfaces">The non-empty list of interface types</param>
        /// <returns>True if any of the types is implemented by <paramref name="type"/>, false otherwise</returns>
        public static bool ImplementsAny(this Type type, params Type[] interfaces)
        {
            ArgumentNullException.ThrowIfNull(type);

            var interfaceMap = interfaces
                .ThrowIfNull(() => new ArgumentNullException(nameof(interfaces)))
                .ThrowIf(
                    arr => arr.Length == 0,
                    arr => new ArgumentException($"Invalid interface list: empty"))
                .ThrowIfAny(
                    t => !t.IsInterface,
                    t => new ArgumentException($"Invalid type [type: {t}, message: not an interface]"))
                .GroupBy(t => t.IsGenericTypeDefinition)
                .ToImmutableDictionary(g => g.Key, g => g.ToImmutableHashSet());

            var actualInterfaces = type.IsGenericTypeDefinition switch
            {
                true => type
                    .GetInterfaces()
                    .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                    .ToImmutableArray(),

                false => [.. type.GetInterfaces()]
            };

            return actualInterfaces.Any(t =>
            {
                if (t.IsGenericTypeDefinition)
                    return interfaceMap.TryGetValue(true, out var gtdInterface)
                        && gtdInterface.Contains(t);

                else return
                    interfaceMap.TryGetValue(false, out var @interface)
                    && @interface.Contains(t);
            });
        }

        /// <summary>
        /// Checks that the given type implements all of the interfaces in the non-empty list '<paramref name="interfaces"/>'. 
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="interfaces">The non-empty list of interface types</param>
        /// <returns>True if all of the types is implemented by <paramref name="type"/>, false otherwise</returns>
        public static bool ImplementsAll(this Type type, params Type[] interfaces)
        {
            ArgumentNullException.ThrowIfNull(type);

            var interfaceMap = interfaces
                .ThrowIfNull(() => new ArgumentNullException(nameof(interfaces)))
                .ThrowIf(
                    arr => arr.Length == 0,
                    arr => new ArgumentException($"Invalid interface list: empty"))
                .ThrowIfAny(
                    t => !t.IsInterface,
                    t => new ArgumentException($"Invalid type [type: {t}, message: not an interface]"))
                .GroupBy(t => t.IsGenericTypeDefinition)
                .ToImmutableDictionary(g => g.Key, g => g.ToImmutableHashSet());

            var actualInterfaces = type.IsGenericTypeDefinition switch
            {
                true => type
                    .GetInterfaces()
                    .Select(i => i.IsGenericType ? i.GetGenericTypeDefinition() : i)
                    .ToImmutableArray(),

                false => [.. type.GetInterfaces()]
            };

            return actualInterfaces.Length > 0 && actualInterfaces.Any(t =>
            {
                if (t.IsGenericTypeDefinition)
                    return interfaceMap.TryGetValue(true, out var gtdInterface)
                        && gtdInterface.Contains(t);

                else return
                    interfaceMap.TryGetValue(false, out var @interface)
                    && @interface.Contains(t);
            });
        }
        #endregion
    }
}