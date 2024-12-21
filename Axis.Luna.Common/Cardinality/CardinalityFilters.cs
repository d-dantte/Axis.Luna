using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Axis.Luna.Common.Cardinality
{
    public interface ICardinalityFilter<TItem>
    {
        bool IsMatch(TItem item);
    }

    /// <summary>
    /// Filter that matches the value encapsulated
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public readonly struct One<TItem> :
        IDefaultValueProvider<One<TItem>>,
        IEquatable<One<TItem>>,
        ICardinalityFilter<TItem>
    {
        public TItem Item { get; }

        #region DefaultValueProvider
        public bool IsDefault => EqualityComparer<TItem>.Default.Equals(default, Item);

        public static One<TItem> Default => default;
        #endregion

        public One(TItem item)
        {
            Item = item;
        }

        public static One<TItem> Of(TItem item) => new(item);

        public static implicit operator One<TItem>(TItem item) => new(item);

        public bool IsMatch(TItem item) => EqualityComparer<TItem>.Default.Equals(Item, item);

        public bool Equals(One<TItem> other) => IsMatch(other.Item);

        public override bool Equals(
            [NotNullWhen(true)] object? obj)
            => obj is One<TItem> other && Equals(other);

        public override int GetHashCode() => Item?.GetHashCode() ?? 0;

        public override string ToString() => $"One({Item?.ToString() ?? "null"})";

        public static bool operator ==(One<TItem> left, One<TItem> right) => left.Equals(right);

        public static bool operator !=(One<TItem> left, One<TItem> right) => !left.Equals(right);
    }

    /// <summary>
    /// Filter that matches only the values encapsulated
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public readonly struct Any<TItem> :
        IDefaultValueProvider<Any<TItem>>,
        IEquatable<Any<TItem>>,
        ICardinalityFilter<TItem>
    {
        public ImmutableHashSet<TItem> Items { get; }

        #region DefaultValueProvider
        public bool IsDefault => Items is null;

        public static Any<TItem> Default => default;
        #endregion

        public Any(params TItem[] item)
        {
            Items = item?.ToImmutableHashSet()!;
        }

        public static Any<TItem> Of(params TItem[] item) => new(item);

        public static Any<TItem> Of(IEnumerable<TItem> items) => new(items?.ToArray()!);

        public static implicit operator Any<TItem>(TItem[] item) => new(item);

        public bool IsMatch(TItem item) => !IsDefault && Items.Contains(item);

        public bool Equals(Any<TItem> other)
        {
            if (IsDefault && other.IsDefault)
                return true;

            if (IsDefault ^ other.IsDefault)
                return false;

            return Items.SetEquals(other.Items);
        }

        public override bool Equals(
            [NotNullWhen(true)] object? obj)
            => obj is Any<TItem> other && Equals(other);

        public override int GetHashCode() => Items?.Aggregate(0, HashCode.Combine) ?? 0;

        public override string ToString() => IsDefault switch
        {
            true => $"{TypeName()}(null)",
            false => Items
                .OrderBy(x => x)
                .Select(item => item?.ToString() ??  "null")
                .JoinUsing(", ")
                .ApplyTo(items => $"{TypeName()}({items})")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string TypeName() => "Any";

        public static bool operator ==(Any<TItem> left, Any<TItem> right) => left.Equals(right);

        public static bool operator !=(Any<TItem> left, Any<TItem> right) => !left.Equals(right);
    }

    /// <summary>
    /// Filter that matches any value except for those encapsulated
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public readonly struct Exclusion<TItem> :
        IDefaultValueProvider<Exclusion<TItem>>,
        IEquatable<Exclusion<TItem>>,
        ICardinalityFilter<TItem>
    {
        public ImmutableHashSet<TItem> Items { get; }

        #region DefaultValueProvider
        public bool IsDefault => Items is null;

        public static Exclusion<TItem> Default => default;
        #endregion

        public Exclusion(params TItem[] item)
        {
            Items = item?.ToImmutableHashSet()!;
        }

        public static Exclusion<TItem> Of(params TItem[] item) => new(item);

        public static Exclusion<TItem> Of(IEnumerable<TItem> items) => new(items?.ToArray()!);

        public static implicit operator Exclusion<TItem>(TItem[] item) => new(item);

        public bool IsMatch(TItem item) => !IsDefault && !Items.Contains(item);

        public bool Equals(Exclusion<TItem> other)
        {
            if (IsDefault && other.IsDefault)
                return true;

            if (IsDefault ^ other.IsDefault)
                return false;

            return Items.SetEquals(other.Items);
        }

        public override bool Equals(
            [NotNullWhen(true)] object? obj)
            => obj is Exclusion<TItem> other && Equals(other);

        public override int GetHashCode() => Items?.Aggregate(0, HashCode.Combine) ?? 0;

        public override string ToString() => IsDefault switch
        {
            true => $"{TypeName()}(null)",
            false => Items
                .OrderBy(x => x)
                .Select(item => item?.ToString() ?? "null")
                .JoinUsing(", ")
                .ApplyTo(items => $"{TypeName()}({items})")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string TypeName() => "Exclusion";

        public static bool operator ==(Exclusion<TItem> left, Exclusion<TItem> right) => left.Equals(right);

        public static bool operator !=(Exclusion<TItem> left, Exclusion<TItem> right) => !left.Equals(right);
    }

    /// <summary>
    /// Filter that matches all values
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public readonly struct All<TItem> :
        IDefaultValueProvider<All<TItem>>,
        IEquatable<All<TItem>>,
        ICardinalityFilter<TItem>
    {
        #region DefaultValueProvider
        public bool IsDefault => true;

        public static All<TItem> Default => default;
        #endregion

        public bool IsMatch(TItem item) => true;

        public bool Equals(All<TItem> other) => true;

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is All<TItem> other;

        public override int GetHashCode() => 0;

        public override string ToString() => $"All()";

        public static bool operator ==(All<TItem> left, All<TItem> right) => true;

        public static bool operator !=(All<TItem> left, All<TItem> right) => false;
    }

    /// <summary>
    /// Filter that matches no value
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public readonly struct None<TItem> :
        IDefaultValueProvider<None<TItem>>,
        IEquatable<None<TItem>>,
        ICardinalityFilter<TItem>
    {
        #region DefaultValueProvider
        public bool IsDefault => true;

        public static None<TItem> Default => default;
        #endregion

        public None() { }

        public bool IsMatch(TItem item) => false;

        public bool Equals(None<TItem> other) => true;

        public override bool Equals([NotNullWhen(true)] object? obj) => obj is None<TItem> other;

        public override int GetHashCode() => 0;

        public override string ToString() => $"None()";

        public static bool operator ==(None<TItem> left, None<TItem> right) => true;

        public static bool operator !=(None<TItem> left, None<TItem> right) => false;
    }

}
