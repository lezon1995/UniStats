using System.Numerics;

namespace UniStats
{
    public static partial class Mod
    {
#if NET7_0_OR_GREATER
        public static NumMod<T> Add<T>(T v, string name = null) where T : INumber<T>
        {
            return Add(Property<T>.Get(v), name);
        }

        public static NumMod<T> Add<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<T>.Get(v, Operator.Add, name);
        }

        public static NumMod<T> Mul<T>(T v, string name = null) where T : INumber<T>
        {
            return Mul(Property<T>.Get(v), name);
        }

        public static NumMod<T> Mul<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<T>.Get(v, Operator.Mul, name);
        }

        public static NumMod<T> Sub<T>(T v, string name = null) where T : INumber<T>
        {
            return Sub(Property<T>.Get(v), name);
        }

        public static NumMod<T> Sub<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<T>.Get(v, Operator.Sub, name);
        }

        public static NumMod<T> Div<T>(T v, string name = null) where T : INumber<T>
        {
            return Div(Property<T>.Get(v), name);
        }

        public static NumMod<T> Div<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<T>.Get(v, Operator.Div, name);
        }

        public static NumMod<T> Set<T>(T v, string name = null) where T : INumber<T>
        {
            return Set(Property<T>.Get(v), name);
        }

        public static NumMod<T> Set<T>(IValue<T> v, string name = null) where T : INumber<T>
        {
            return NumMod<T>.Get(v, Operator.Set, name);
        }
#endif
    }
}