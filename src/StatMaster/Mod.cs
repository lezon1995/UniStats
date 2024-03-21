using System;
using System.Collections.Generic;
using System.Text;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace StatMaster
{
    public static partial class Mod
    {
        public static IMod<T> Create<T>(Func<T, T> func, out Action onChange, string name = null)
        {
            return FuncMod<T>.Get(func, out onChange, name);
        }

        public static IMod<T> Create<T>(Func<T, T> func, string name = null)
        {
            return FuncMod<T>.Get(func, name);
        }

        public static IMod<S, T> WithContext<S, T>(this IMod<T> mod, S context, string name = null)
        {
            return WrapMod<S, T>.Get(context, mod, name);
        }
    }

    enum Operator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Set,
    }

    public abstract class AMod<S, T> : IMod<S, T>, IDisposable
    {
        public string Name { get; internal set; }

        bool _enabled = true;

        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnChange();
                }
            }
        }

        public S Context { get; internal set; }

        public event ChangeHandler OnChanged;

        protected AMod(S context)
        {
            switch (context)
            {
                case IValueChanged notify:
                    notify.OnChanged += Chain;
                    break;
                case IValueChanged<T> notifyT:
                    notifyT.OnChanged += Chain;
                    break;
            }

            Context = context;
        }

        public abstract T Modify(T given);

        protected void OnChange()
        {
            OnChanged?.Invoke();
        }

        internal void Chain()
        {
            OnChange();
        }

        void Chain(T pre, T now)
        {
            OnChange();
        }

        public void Dispose()
        {
            switch (Context)
            {
                case IValueChanged notify:
                    notify.OnChanged -= Chain;
                    break;
                case IValueChanged<T> notifyT:
                    notifyT.OnChanged -= Chain;
                    break;
            }
        }

        public abstract void Release();

        public virtual void OnRelease()
        {
            Name = null;
            _enabled = true;
            switch (Context)
            {
                case IValueChanged notify:
                    notify.OnChanged -= Chain;
                    break;
                case IValueChanged<T> notifyT:
                    notifyT.OnChanged -= Chain;
                    break;
            }

            if (Context is IPool pool)
            {
                pool.Release();
            }

            Context = default;
            OnChanged = null;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Name != null)
            {
                sb.Append('"');
                sb.Append(Name);
                sb.Append('"');
                sb.Append(' ');
            }

            sb.Append(Context);
            return sb.ToString();
        }
    }

    internal partial class NumMod<S, T> : AMod<S, T> where S : IValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        Operator Op { get; set; }

        NumMod(S context) : base(context)
        {
        }

#if NET7_0_OR_GREATER
        public override T Modify(T given)
        {
            T v = Context.Value;
            return Op switch
            {
                Operator.Add => given + v,
                Operator.Subtract => given - v,
                Operator.Multiply => given * v,
                Operator.Divide => given / v,
                Operator.Set => v,
                _ => given
            };
        }
#else
        public override T Modify(T given)
        {
            var t = Mod.GetOperator<T>();
            T v = Context.Value;
            return Op switch
            {
                Operator.Add => t.Sum(given, v),
                Operator.Subtract => t.Sum(given, t.Negate(v)),
                Operator.Multiply => t.Times(given, v),
                Operator.Divide => t.Divide(given, v),
                Operator.Set => v,
                _ => given
            };
        }
#endif


        public override void OnRelease()
        {
            Op = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            // builder.Append("ref ");
            if (Name != null)
            {
                // Append the name enclosed in double quotes
                sb.Append('"');
                sb.Append(Name);
                sb.Append('"');
                sb.Append(' ');
            }

            switch (Op)
            {
                case Operator.Add:
                    sb.Append('+');
                    break;
                case Operator.Subtract:
                    sb.Append('-');
                    break;
                case Operator.Multiply:
                    sb.Append('*');
                    break;
                case Operator.Divide:
                    sb.Append('/');
                    break;
                case Operator.Set:
                    sb.Append('=');
                    break;
            }

            sb.Append(Context.Value);

            return sb.ToString();
        }
    }

    internal partial class NumMod<S, T>
    {
        static Queue<NumMod<S, T>> pool = new Queue<NumMod<S, T>>();

        public static NumMod<S, T> Get(S context, Operator op, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                value.Context = context;
                value.Op = op;
                value.Name = name;
                return value;
            }

            value = new NumMod<S, T>(context);
            value.Op = op;
            value.Name = name;
            return value;
        }

        static void Release(NumMod<S, T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override void Release()
        {
            Release(this);
        }
    }

    internal partial class FuncMod<T> : AMod<Func<T, T>, T>
    {
        FuncMod(Func<T, T> func) : base(func)
        {
        }

        FuncMod(Func<T, T> func, out Action onChange) : this(func)
        {
            onChange = OnChange;
        }

        public override T Modify(T given)
        {
            return Context(given);
        }

        public override string ToString()
        {
            return Name ?? "?f()";
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }
    }

    internal partial class FuncMod<T>
    {
        static Queue<FuncMod<T>> pool = new Queue<FuncMod<T>>();

        public static FuncMod<T> Get(Func<T, T> context, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                value.Context = context;
                value.Name = name;
                return value;
            }

            value = new FuncMod<T>(context);
            value.Name = name;
            return value;
        }

        public static FuncMod<T> Get(Func<T, T> context, out Action onChange, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                value.Context = context;
                value.Name = name;
                onChange = value.OnChange;
                return value;
            }

            value = new FuncMod<T>(context, out onChange);
            value.Name = name;
            return value;
        }

        static void Release(FuncMod<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override void Release()
        {
            Release(this);
        }
    }

    internal partial class CastMod<S, T> : AMod<IMod<S>, T>
#if NET7_0_OR_GREATER
        where S : INumber<S>
        where T : INumber<T>
#endif
    {
        CastMod(IMod<S> context) : base(context)
        {
        }

#if NET7_0_OR_GREATER
        public override T Modify(T given)
        {
            return T.CreateChecked(Context.Modify(S.CreateChecked(given)));
        }
#else
        public override T Modify(T given)
        {
            var s = Mod.GetOperator<S>();
            var t = Mod.GetOperator<T>();
            return t.Create(Context.Modify(s.Create(given)));
        }
#endif

        public override void OnRelease()
        {
            base.OnRelease();
        }
    }

    internal partial class CastMod<S, T>
    {
        static Queue<CastMod<S, T>> pool = new Queue<CastMod<S, T>>();

        public static CastMod<S, T> Get(IMod<S> context, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                value.Context = context;
                value.Name = name;
                return value;
            }

            value = new CastMod<S, T>(context);
            value.Name = name;
            return value;
        }

        static void Release(CastMod<S, T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override void Release()
        {
            Release(this);
        }
    }

    internal partial class WrapMod<S, T> : AMod<S, T>, IDecorator<IMod<T>>
    {
        public IMod<T> Decorated { get; set; }

        public override bool Enabled
        {
            get => Decorated.Enabled;
            set => Decorated.Enabled = value;
        }

        WrapMod(S context, IMod<T> inner) : base(context)
        {
            Decorated = inner;
            Decorated.OnChanged += Chain;
        }

        public override T Modify(T given)
        {
            return Decorated.Modify(given);
        }

        public override void OnRelease()
        {
            Decorated.OnChanged -= Chain;
            Decorated = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            return Decorated.ToString();
        }
    }

    internal partial class WrapMod<S, T>
    {
        static Queue<WrapMod<S, T>> pool = new Queue<WrapMod<S, T>>();

        public static WrapMod<S, T> Get(S context, IMod<T> inner, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                value.Context = context;
                value.Decorated = inner;
                value.Name = name;
                return value;
            }

            value = new WrapMod<S, T>(context, inner);
            value.Name = name;
            return value;
        }

        static void Release(WrapMod<S, T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override void Release()
        {
            Release(this);
        }
    }
}