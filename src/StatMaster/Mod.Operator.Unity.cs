using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace UniStats
{
    public static partial class Mod
    {
#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Represents an operator for performing mathematical operations on a generic type.
        /// Here is the alternative to having a nice INumber<T> type like .NET7 will have.
        /// </summary>
        /// <typeparam name="X">The type on which the operator performs operations.</typeparam>
        public interface IOperator<X>
        {
            X Create<T>(T other);
            X Add(X a, X b);
            X Sub(X a, X b);
            X Mul(X a, X b);
            X Div(X a, X b);
            X Negate(X a);
            X Max(X a, X b);
            X Min(X a, X b);
            X Zero { get; }
            X One { get; }
        }

        internal class OpFloat : IOperator<float>
        {
            public static IOperator<float> Instance = new OpFloat();
            public float Create<T>(T other) => Convert.ToSingle(other);
            public float Add(float a, float b) => a + b;
            public float Sub(float a, float b) => a - b;
            public float Mul(float a, float b) => a * b;
            public float Div(float a, float b) => a / b;
            public float Negate(float a) => -a;
            public float Max(float a, float b) => Math.Max(a, b);
            public float Min(float a, float b) => Math.Min(a, b);
            public float Zero => 0f;
            public float One => 1f;
        }

        internal class OpDouble : IOperator<double>
        {
            public static IOperator<double> Instance = new OpDouble();
            public double Create<T>(T other) => Convert.ToDouble(other);
            public double Add(double a, double b) => a + b;
            public double Sub(double a, double b) => a - b;
            public double Mul(double a, double b) => a * b;
            public double Div(double a, double b) => a / b;
            public double Negate(double a) => -a;
            public double Max(double a, double b) => Math.Max(a, b);
            public double Min(double a, double b) => Math.Min(a, b);
            public double Zero => 0.0;
            public double One => 1.0;
        }

        internal class OpInt : IOperator<int>
        {
            public static IOperator<int> Instance = new OpInt();
            public int Create<T>(T other) => Convert.ToInt32(other);
            public int Add(int a, int b) => a + b;
            public int Sub(int a, int b) => a - b;
            public int Mul(int a, int b) => a * b;
            public int Div(int a, int b) => a / b;
            public int Negate(int a) => -a;
            public int Max(int a, int b) => Math.Max(a, b);
            public int Min(int a, int b) => Math.Min(a, b);
            public int Zero => 0;
            public int One => 1;
        }

#if UNITY_5_3_OR_NEWER
        internal class OpVector3 : IOperator<Vector3>
        {
            public static IOperator<Vector3> Instance = new OpVector3();

            public Vector3 Create<T>(T other)
            {
                var op = GetOperator<float>();
                var o = op.Create(other);
                return new Vector3(o, o, o);
            }

            public Vector3 Add(Vector3 a, Vector3 b) => a + b;
            public Vector3 Sub(Vector3 a, Vector3 b) => a - b;
            public Vector3 Mul(Vector3 a, Vector3 b) => Vector3.Scale(a, b);
            public Vector3 Div(Vector3 a, Vector3 b) => Vector3.Scale(a, new Vector3(1f / b.x, 1f / b.y, 1f / b.z));
            public Vector3 Negate(Vector3 a) => -a;
            public Vector3 Max(Vector3 a, Vector3 b) => Vector3.Max(a, b);
            public Vector3 Min(Vector3 a, Vector3 b) => Vector3.Min(a, b);
            public Vector3 Zero => Vector3.zero;
            public Vector3 One => Vector3.one;
        }
#endif

        public static IOperator<S> GetOperator<S>()
        {
            switch (Type.GetTypeCode(typeof(S)))
            {
                case TypeCode.Double:
                    return OpDouble.Instance as IOperator<S>;
                case TypeCode.Single:
                    return OpFloat.Instance as IOperator<S>;
                case TypeCode.Int32:
                    return OpInt.Instance as IOperator<S>;
                case TypeCode.Object:
#if UNITY_5_3_OR_NEWER
                    if (typeof(S) == typeof(Vector3))
                        return OpVector3.Instance as IOperator<S>;
                    goto default;
#endif
                default:
                    throw new NotImplementedException($"No IOperator<T> implementation for type {typeof(S)}.");
            }
        }

        public static IMod<IValue<S>, S> Add<S>(S v, string name = null) where S : struct
        {
            return Add(Property<S>.Get(v), name);
        }

        public static IMod<IValue<S>, S> Add<S>(IValue<S> v, string name = null)
        {
            return NumMod<IValue<S>, S>.Get(v, Operator.Add, name);
        }

        public static IMod<IValue<S>, S> Mul<S>(S v, string name = null) where S : struct
        {
            return Mul(Property<S>.Get(v), name);
        }

        public static IMod<IValue<S>, S> Mul<S>(IValue<S> v, string name = null)
        {
            return NumMod<IValue<S>, S>.Get(v, Operator.Multiply, name);
        }

        public static IMod<IValue<S>, S> Sub<S>(S v, string name = null) where S : struct
        {
            return Sub(Property<S>.Get(v), name);
        }

        public static IMod<IValue<S>, S> Sub<S>(IValue<S> v, string name = null)
        {
            return NumMod<IValue<S>, S>.Get(v, Operator.Subtract, name);
        }

        public static IMod<IValue<S>, S> Div<S>(S v, string name = null) where S : struct
        {
            return Div(Property<S>.Get(v), name);
        }

        public static IMod<IValue<S>, S> Div<S>(IValue<S> v, string name = null)
        {
            return NumMod<IValue<S>, S>.Get(v, Operator.Divide, name);
        }

        public static IMod<IValue<S>, S> Set<S>(S v, string name = null) where S : struct
        {
            return Set(Property<S>.Get(v), name);
        }

        public static IMod<IValue<S>, S> Set<S>(IValue<S> v, string name = null)
        {
            return NumMod<IValue<S>, S>.Get(v, Operator.Set, name);
        }

#endif
    }
}