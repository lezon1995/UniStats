using System;
using System.Collections.Generic;

namespace StatMaster
{
    public static partial class Mod
    {
        /// <summary>
        /// Creates a target for modifying a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="index">The index of the value in the list.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the list value.</returns>
        public static ITarget<IList<IModValue<T>>, T> TargetList<T>(this IMod<T> mod, int index, string name = null)
        {
            return new ListTarget<T>
            {
                Mod = mod,
                Context = index,
                Name = name
            };
        }

        /// <summary>
        /// Creates a target for modifying a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="key">The key of the value in the dictionary.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the dictionary value.</returns>
        public static ITarget<IDictionary<K, IModValue<T>>, T> TargetDictionary<K, T>(this IMod<T> mod, K key, string name = null)
        {
            return new DictionaryTarget<K, T>
            {
                Mod = mod,
                Context = key,
                Name = name
            };
        }

        /// <summary>
        /// Creates a target for modifying a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="mod">The modifier to apply to the value.</param>
        /// <param name="target">The function that provides the target value.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the value based on the custom target function.</returns>
        public static ITarget<S, T> Target<S, T>(this IMod<T> mod, Func<S, IModValue<T>> target, string name = null)
        {
            return new FuncTarget<S, T>
            {
                Mod = mod,
                Context = target,
                Name = name
            };
        }

        /// <summary>
        /// Represents a base class for targets that apply modifications to values.
        /// </summary>
        /// <typeparam name="R">The type of the target context.</typeparam>
        /// <typeparam name="S">The type of the target object.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal abstract class BaseTarget<R, S, T> : ITarget<S, T>
        {
            /// <summary>
            /// Gets or sets the name of the target.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the context of the target.
            /// </summary>
            public R Context { get; set; }

            /// <summary>
            /// Gets or sets the modifier to be applied to the value.
            /// </summary>
            public IMod<T> Mod { get; set; }

            /// <summary>
            /// Gets the default name of the target.
            /// </summary>
            public virtual string DefaultName => Context.ToString();

            /// <summary>
            /// Applies the target to the specified object.
            /// </summary>
            /// <param name="bag">The object to apply the target to.</param>
            /// <returns>The modifiable value that the target applies to.</returns>
            public abstract IModListValue<T> AppliesTo(S bag);

            /// <summary>
            /// Returns the string representation of the target.
            /// </summary>
            /// <returns>The string representation of the target.</returns>
            public override string ToString()
            {
                return Name ?? DefaultName;
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class FuncTarget<S, T> : BaseTarget<Func<S, IModValue<T>>, S, T>
        {
            public override IModListValue<T> AppliesTo(S bag)
            {
                return Context(bag);
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class ListTarget<T> : BaseTarget<int, IList<IModValue<T>>, T>
        {
            public override IModListValue<T> AppliesTo(IList<IModValue<T>> bag)
            {
                return bag[Context];
            }
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class DictionaryTarget<K, T> : BaseTarget<K, IDictionary<K, IModValue<T>>, T>
        {
            public override IModListValue<T> AppliesTo(IDictionary<K, IModValue<T>> bag)
            {
                return bag[Context];
            }
        }
    }

    public static class TargetedModifierExtensions
    {
        /// <summary>
        /// Adds the modifier associated with the applicator to the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to which the modifier will be added.</param>
        public static void AddToBag<S, T>(this ITarget<S, T> applicator, S bag)
        {
            applicator.AppliesTo(bag).Add(applicator.Mod);
        }

        /// <summary>
        /// Removes the modifier associated with the applicator from the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag from which the modifier will be removed.</param>
        /// <returns>True if the modifier was successfully removed, otherwise false.</returns>
        public static bool RemoveFromBag<S, T>(this ITarget<S, T> applicator, S bag)
        {
            return applicator.AppliesTo(bag).Remove(applicator.Mod);
        }

        /// <summary>
        /// Checks if the modifier associated with the applicator is contained in the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to check for the presence of the modifier.</param>
        /// <returns>True if the modifier is contained in the bag, otherwise false.</returns>
        public static bool ContainedInBag<S, T>(this ITarget<S, T> applicator, S bag)
        {
            return applicator.AppliesTo(bag).Contains(applicator.Mod);
        }
    }
}