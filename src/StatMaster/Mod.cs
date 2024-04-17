using System;
using System.Collections.Generic;
using System.Text;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
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

        public static WrapMod<S, T> WithContext<S, T>(this IMod<T> mod, S context, string name = null)
        {
            return WrapMod<S, T>.Get(context, mod, name);
        }
    }

    public enum Operator
    {
        Add,
        Sub,
        Mul,
        Div,
        Set,
    }

    [Serializable]
    public abstract class AMod<T> : IMod<T>, IDisposable
    {
        public Key Key { get; set; }

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        protected string _name;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
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

        public event ChangeHandler OnChanged;

        public abstract T Modify(T given);

        protected void OnChange()
        {
            OnChanged?.Invoke();
        }

        protected void OnChange(T pre, T now)
        {
            OnChanged?.Invoke();
        }

        public void Dispose()
        {
        }

        public abstract void Release();

        public virtual void OnRelease()
        {
            _name = null;
            _enabled = true;
            OnChanged = null;
        }
    }

    [Serializable]
    public partial class NumMod<T> : AMod<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        IValue<T> _value;
        public IValue<T> Value => _value;

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        Operator _op;

        NumMod(IValue<T> context, Operator op, string name)
        {
            ConstructContext(context);
            _op = op;
            _name = name;
        }

        NumMod<T> Build(IValue<T> context, Operator op, string name)
        {
            ConstructContext(context);
            _op = op;
            _name = name;
            return this;
        }

        void ConstructContext(IValue<T> context)
        {
            context.OnChanged += OnChange;
            _value = context;
        }

        protected void DeconstructContext()
        {
            _value.Release();
            _value.OnChanged -= OnChange;
            _value = null;
        }

#if NET7_0_OR_GREATER
        public override T Modify(T given)
        {
            T v = _value.Value;
            return _op switch
            {
                Operator.Add => given + v,
                Operator.Sub => given - v,
                Operator.Mul => given * v,
                Operator.Div => given / v,
                Operator.Set => v,
                _ => given
            };
        }
#else
        public override T Modify(T given)
        {
            var t = Mod.GetOperator<T>();
            T v = _value.Value;
            return _op switch
            {
                Operator.Add => t.Add(given, v),
                Operator.Sub => t.Sub(given, v),
                Operator.Mul => t.Mul(given, v),
                Operator.Div => t.Div(given, v),
                Operator.Set => v,
                _ => given
            };
        }
#endif


        public override void OnRelease()
        {
            DeconstructContext();
            _op = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Num[");
            sb.Append(_name);
            sb.Append("]");

            switch (_op)
            {
                case Operator.Add:
                    sb.Append('+');
                    break;
                case Operator.Sub:
                    sb.Append('-');
                    break;
                case Operator.Mul:
                    sb.Append('*');
                    break;
                case Operator.Div:
                    sb.Append('/');
                    break;
                case Operator.Set:
                    sb.Append('=');
                    break;
            }

            sb.Append(' ');
            sb.Append(_value.Value);

            return sb.ToString();
        }
    }

    public partial class NumMod<T>
    {
        static Queue<NumMod<T>> pool = new Queue<NumMod<T>>();

        public static NumMod<T> Get(IValue<T> context, Operator op, string name)
        {
            if (pool.TryDequeue(out var mod))
            {
                return mod.Build(context, op, name);
            }

            return new NumMod<T>(context, op, name);
        }

        static void Release(NumMod<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override void Release()
        {
            Release(this);
        }
    }

    [Serializable]
    public partial class FuncMod<T> : AMod<T>
    {
        Func<T, T> _context;

        FuncMod(Func<T, T> func, string name)
        {
            _context = func;
            _name = name;
        }

        FuncMod(Func<T, T> func, string name, out Action onChange) : this(func, name)
        {
            onChange = OnChange;
        }

        FuncMod<T> Build(Func<T, T> func, string name)
        {
            _context = func;
            _name = name;
            return this;
        }

        FuncMod<T> Build(Func<T, T> func, string name, out Action onChange)
        {
            _context = func;
            _name = name;
            onChange = OnChange;
            return this;
        }

        public override T Modify(T given)
        {
            return _context(given);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Func[");
            sb.Append(_name);
            sb.Append("]");
            return sb.ToString();
        }

        public override void OnRelease()
        {
            _context = null;
            base.OnRelease();
        }
    }

    public partial class FuncMod<T>
    {
        static Queue<FuncMod<T>> pool = new Queue<FuncMod<T>>();

        public static FuncMod<T> Get(Func<T, T> context, string name)
        {
            if (pool.TryDequeue(out var mod))
            {
                return mod.Build(context, name);
            }

            return new FuncMod<T>(context, name);
        }

        public static FuncMod<T> Get(Func<T, T> context, out Action onChange, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                return value.Build(context, name, out onChange);
            }

            return new FuncMod<T>(context, name, out onChange);
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

    [Serializable]
    public partial class CastMod<S, T> : AMod<T>
#if NET7_0_OR_GREATER
        where S : INumber<S>
        where T : INumber<T>
#endif
    {
        IMod<S> _mod;

        CastMod(IMod<S> context, string name)
        {
            ConstructContext(context);
            _name = name;
        }

        CastMod<S, T> Build(IMod<S> mod, string name)
        {
            ConstructContext(mod);
            _name = name;
            return this;
        }

        public override T Modify(T given)
        {
#if NET7_0_OR_GREATER
            return T.CreateChecked(_mod.Modify(S.CreateChecked(given)));
#else
            var s = Mod.GetOperator<S>();
            var t = Mod.GetOperator<T>();
            return t.Create(_mod.Modify(s.Create(given)));
#endif
        }

        void ConstructContext(IMod<S> context)
        {
            context.OnChanged += OnChange;
            _mod = context;
        }

        protected void DeconstructContext()
        {
            _mod.Release();
            _mod.OnChanged -= OnChange;
            _mod = null;
        }

        public override void OnRelease()
        {
            DeconstructContext();
            base.OnRelease();
        }
    }

    public partial class CastMod<S, T>
    {
        static Queue<CastMod<S, T>> pool = new Queue<CastMod<S, T>>();

        public static CastMod<S, T> Get(IMod<S> context, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                return value.Build(context, name);
            }

            return new CastMod<S, T>(context, name);
        }

        static void Release(CastMod<S, T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Cast[");
            sb.Append(_name);
            sb.Append("]");
            return sb.ToString();
        }

        public override void Release()
        {
            Release(this);
        }
    }

    [Serializable]
    public partial class WrapMod<S, T> : AMod<T>, IDecorator<IMod<T>>
    {
        S _context;
        public IMod<T> Decorated { get; set; }

        public override bool Enabled
        {
            get => Decorated.Enabled;
            set => Decorated.Enabled = value;
        }

        WrapMod(S context, IMod<T> inner, string name)
        {
            ConstructContext(context);
            Decorated = inner;
            Decorated.OnChanged += OnChange;
            _name = name;
        }

        WrapMod<S, T> Build(S context, IMod<T> inner, string name)
        {
            ConstructContext(context);
            Decorated = inner;
            Decorated.OnChanged += OnChange;
            _name = name;
            return this;
        }

        public override T Modify(T given)
        {
            return Decorated.Modify(given);
        }

        void ConstructContext(S context)
        {
            switch (context)
            {
                case IMod<T> notifyT:
                    notifyT.OnChanged += OnChange;
                    break;
                case IValue<T> notifyT:
                    notifyT.OnChanged += OnChange;
                    break;
            }

            _context = context;
        }

        protected void DeconstructContext()
        {
            switch (_context)
            {
                case IMod<T> mod:
                    mod.Release();
                    mod.OnChanged -= OnChange;
                    break;
                case IValue<T> value:
                    value.Release();
                    value.OnChanged -= OnChange;
                    break;
            }

            _context = default;
        }

        public override void OnRelease()
        {
            DeconstructContext();
            Decorated.OnChanged -= OnChange;
            Decorated = default;
            base.OnRelease();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Cast[");
            sb.Append(_name);
            sb.Append("]");
            sb.Append(Decorated);
            return sb.ToString();
        }
    }

    public partial class WrapMod<S, T>
    {
        static Queue<WrapMod<S, T>> pool = new Queue<WrapMod<S, T>>();

        public static WrapMod<S, T> Get(S context, IMod<T> inner, string name)
        {
            if (pool.TryDequeue(out var value))
            {
                return value.Build(context, inner, name);
            }

            return new WrapMod<S, T>(context, inner, name);
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