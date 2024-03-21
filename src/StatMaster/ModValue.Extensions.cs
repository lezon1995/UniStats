using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StatMaster
{
    public static class ModValueExtensions
    {
        /// <summary>
        /// Collects how a particular modifier changes the value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modValue">The modifiable value.</param>
        /// <param name="mod">The modifier to probe.</param>
        /// <returns>An enumerable of before and after values.</returns>
        public static IEnumerable<(T before, T after)> ProbeAffects<T>(this IModValue<IValue<T>, T> modValue, IMod<T> mod)
        {
            T before = modValue.Initial.Value;
            var mods = modValue.Mods;
            for (var i = 0; i < mods.Count; i++)
            {
                var m = mods[i];
                T after = before;
                if (m.Enabled)
                {
                    after = m.Modify(before);
                }

                if (mod == m)
                {
                    yield return (before, after);
                }

                before = after;
            }
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Returns the delta a modifier (may be multiple) does.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modValue">The modifiable value.</param>
        /// <param name="mod">The modifier to probe.</param>
        /// <returns>The accumulated delta.</returns>
        public static T ProbeDelta<T>(this IModValue<IValue<T>, T> modValue, IMod<T> mod) where T : INumber<T>
        {
            T accum = T.Zero;
            foreach (T delta in modValue.ProbeAffects(mod).Select(x => x.after - x.before))
            {
                accum += delta;
            }

            return accum;
        }
#else
        public static T ProbeDelta<T>(this IModValue<IValue<T>, T> modValue, IMod<T> mod)
        {
            var op = Mod.GetOperator<T>();
            T accum = op.Zero;
            foreach (
                var delta in modValue
                    .ProbeAffects(mod)
                    .Select(x => op.Sum(x.after, op.Negate(x.before)))
            )
                accum = op.Sum(accum, delta);
            return accum;
        }
#endif

        /// <summary>
        /// Removes all occurrences of an item from a collection. Returns the number of items removed.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAll<T>(this ICollection<T> collection, T item)
        {
            int count = 0;
            while (collection.Remove(item))
            {
                count++;
            }

            return count;
        }
    }
}