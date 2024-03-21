using System;
using System.Numerics;

namespace StatMaster
{
    public static class ValueExtensions
    {
        public static IValue<T> Select<S, T>(this IValue<S> v, Func<S, T> selector)
        {
            IValue<T> w = Property.Create(() => selector(v.Value), out var onChange);
            v.OnChanged += (pre, now) => onChange(selector(pre), selector(now));
            return w;
        }

        public static IValue<U> Zip<S, T, U>(this IValue<S> s, IValue<T> t, Func<S, T, U> selector)
        {
            var u = Property.Create(() => selector(s.Value, t.Value), out var onChange);
            s.OnChanged += (pre, now) => onChange(selector(pre, t.Value), selector(now, t.Value));
            t.OnChanged += (pre, now) => onChange(selector(s.Value, pre), selector(s.Value, now));
            return u;
        }

        public static IValue<T> Select<S, T>(this IValue<S> v, Func<S, T> selector, Action<IValue<S>, T> set)
        {
            var w = Property.Create(() => selector(v.Value), x => set(v, x), out var onChange);
            v.OnChanged += (pre, now) => onChange(selector(pre), selector(now));
            return w;
        }

        class ActionDisposable : IDisposable
        {
            Action _action;

            public ActionDisposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

        /// <summary>
        /// Subscribes to the property change events of an object and executes the specified action.
        /// </summary>
        /// <typeparam name="T">The type of the object implementing <see cref="IValueChanged"/>.</typeparam>
        /// <param name="v">The object to subscribe to.</param>
        /// <param name="action">The action to execute on property change.</param>
        /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
        public static IDisposable OnChange<T>(this T v, Action<T> action) where T : IValueChanged
        {
            v.OnChanged += PropertyChange;
            return new ActionDisposable(() => v.OnChanged -= PropertyChange);
            void PropertyChange() => action(v);
        }

        /// <summary>
        /// Casts an <see cref="IMod{T}"/> to an <see cref="IMod{T}"/>.
        /// </summary>
        /// <typeparam name="X">The source value type.</typeparam>
        /// <typeparam name="Y">The target value type.</typeparam>
        /// <param name="m">The modifier to cast.</param>
        /// <returns>The casted modifier.</returns>
        public static IMod<Y> Cast<X, Y>(this IMod<X> m)
#if NET7_0_OR_GREATER
            where X : INumber<X>
            where Y : INumber<Y>
#endif
        {
            return CastMod<X, Y>.Get(m, null);
        }
    }
}