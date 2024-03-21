using System.Numerics;

namespace UniStats
{
    public static partial class Mod
    {
#if NET7_0_OR_GREATER
        public static IMod<IValue<T>, T> Add<T>(T v, string name = null) where T : INumber<T>
        {
            return Add(Property<T>.Get(v), name);
        }

        public static IMod<IValue<T>, T> Add<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<IValue<T>, T>.Get(v, Operator.Add, name);
        }

        public static IMod<IValue<T>, T> Mul<T>(T v, string name = null) where T : INumber<T>
        {
            return Mul(Property<T>.Get(v), name);
        }

        public static IMod<IValue<T>, T> Mul<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<IValue<T>, T>.Get(v, Operator.Multiply, name);
        }

        public static IMod<IValue<T>, T> Sub<T>(T v, string name = null) where T : INumber<T>
        {
            return Sub(Property<T>.Get(v), name);
        }

        public static IMod<IValue<T>, T> Sub<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<IValue<T>, T>.Get(v, Operator.Subtract, name);
        }

        public static IMod<IValue<T>, T> Div<T>(T v, string name = null) where T : INumber<T>
        {
            return Div(Property<T>.Get(v), name);
        }

        public static IMod<IValue<T>, T> Div<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<IValue<T>, T>.Get(v, Operator.Divide, name);
        }

        public static IMod<IValue<T>, T> Set<T>(T v, string name = null) where T : INumber<T>
        {
            return Set(Property<T>.Get(v), name);
        }

        public static IMod<IValue<T>, T> Set<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<IValue<T>, T>.Get(v, Operator.Set, name);
        }
#endif
    }
}