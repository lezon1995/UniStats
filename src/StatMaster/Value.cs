using System.Numerics;

namespace StatMaster
{
    [Serializable]
    public partial class Property<T> : IValue<T>
    {
        public event ChangeHandler<T> OnChanged;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        T _value;

        public virtual T Value
        {
            get => _value;
            set
            {
                if (!_value.Equals(value))
                {
                    var pre = _value;
                    _value = value;
                    OnChanged?.Invoke(pre, value);
                }
            }
        }

        public Property()
        {
        }

        public Property(T value)
        {
            _value = value;
        }

        public void OnRelease()
        {
            _value = default;
            OnChanged = null;
        }
    }

    public partial class Property<T>
    {
        static Queue<Property<T>> pool = new Queue<Property<T>>();

        public static Property<T> Get(T initial = default)
        {
            if (pool.TryDequeue(out var value))
            {
                value._value = initial;
                return value;
            }

            value = new Property<T>(initial);
            return value;
        }

        public static void Release(Property<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public void Release()
        {
            Release(this);
        }
    }

    public static class Property
    {
        public static IValue<T> Create<T>(Func<T> getter, out ChangeHandler<T> onChange)
        {
            return DerivedValue<T>.Get(getter, out onChange);
        }

        public static IValue<T> Create<T>(Func<T> getter, Action<T> setter, out ChangeHandler<T> onChange)
        {
            return DerivedValue<T>.Get(getter, setter, out onChange);
        }

        partial class DerivedValue<T> : IValue<T>
        {
            public event ChangeHandler<T> OnChanged;

            public Func<T> Getter { get; internal set; }
            public Action<T> Setter { get; internal set; }

            public T Value
            {
                get => Getter();
                set => Setter?.Invoke(value);
            }

            DerivedValue(Func<T> getter, out ChangeHandler<T> onChange)
            {
                Getter = getter;
                Setter = null;
                onChange = OnChange;
            }

            DerivedValue(Func<T> getter, Action<T> setter, out ChangeHandler<T> onChange)
            {
                Getter = getter;
                Setter = setter;
                onChange = OnChange;
            }

            void OnChange(T pre, T now)
            {
                OnChanged?.Invoke(pre, now);
            }

            public void OnRelease()
            {
                Getter = null;
                Setter = null;
                OnChanged = null;
            }
        }

        partial class DerivedValue<T>
        {
            static Queue<DerivedValue<T>> pool = new Queue<DerivedValue<T>>();

            public static DerivedValue<T> Get(Func<T> getter, out ChangeHandler<T> onChange)
            {
                if (pool.TryDequeue(out var value))
                {
                    value.Getter = getter;
                    onChange = value.OnChange;
                    return value;
                }

                value = new DerivedValue<T>(getter, out onChange);
                return value;
            }

            public static DerivedValue<T> Get(Func<T> getter, Action<T> setter, out ChangeHandler<T> onChange)
            {
                if (pool.TryDequeue(out var value))
                {
                    value.Getter = getter;
                    value.Setter = setter;
                    onChange = value.OnChange;
                    return value;
                }

                value = new DerivedValue<T>(getter, setter, out onChange);
                return value;
            }

            static void Release(DerivedValue<T> value)
            {
                value.OnRelease();
                pool.Enqueue(value);
            }

            public void Release()
            {
                Release(this);
            }
        }
    }

    [Serializable]
    public partial class RangeValue<T> : IValue<T>, IRange<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        public event ChangeHandler<T> OnChanged;
        T _value;

        public T Value
        {
            get => _value;
            set
            {
                var now = Clamp(value, Min, Max);
                if (_value != now)
                {
                    var pre = _value;
                    _value = now;
                    OnChange(pre, now);
                }
            }
        }

        public T Min => Lower.Value;
        public T Max => Upper.Value;

        public IValue<T> Lower;
        public IValue<T> Upper;

        public static T Clamp(T value, T min, T max)
        {
#if NET7_0_OR_GREATER
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
#else
            var op = Mod.GetOperator<T>();
            return op.Max(min, op.Min(max, value));
#endif
        }

        #region Constructor

        Action releaseAction;

        public RangeValue(T value, IValue<T> lower, IValue<T> upper)
        {
            _value = value;
            Lower = lower;
            Lower.OnChanged += BoundChanged;

            Upper = upper;
            Upper.OnChanged += BoundChanged;
        }

        public RangeValue(T value, T lower, IValue<T> upper) : this(value, Property<T>.Get(lower), upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Lower);
            };
        }

        public RangeValue(T value, IValue<T> lower, T upper) : this(value, lower, Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Upper);
            };
        }

        public RangeValue(T value, T lower, T upper) : this(value, Property<T>.Get(lower), Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)Lower);
                Property<T>.Release((Property<T>)Upper);
            };
        }

        #endregion

        void BoundChanged(T pre, T now)
        {
            Value = _value;
        }

        protected void OnChange(T pre, T now)
        {
            OnChanged?.Invoke(pre, now);
        }

        public void OnRelease()
        {
            _value = default;

            Lower.OnChanged -= BoundChanged;
            Upper.OnChanged -= BoundChanged;

            releaseAction?.Invoke();
            releaseAction = null;

            Lower = null;
            Upper = null;

            OnChanged = null;
        }
    }

    public partial class RangeValue<T>
    {
        static Queue<RangeValue<T>> pool = new Queue<RangeValue<T>>();

        public static RangeValue<T> Get(T initial, IValue<T> lower, IValue<T> upper)
        {
            if (pool.TryDequeue(out var value))
            {
                value._value = initial;

                value.Lower = lower;
                value.Lower.OnChanged += value.BoundChanged;

                value.Upper = upper;
                value.Upper.OnChanged += value.BoundChanged;
                return value;
            }

            value = new RangeValue<T>(initial, lower, upper);
            return value;
        }

        public static RangeValue<T> Get(T initial, T lower, IValue<T> upper)
        {
            if (pool.TryDequeue(out var value))
            {
                value.releaseAction = () =>
                {
                    //
                    Property<T>.Release((Property<T>)value.Lower);
                };

                var _lower = Property<T>.Get(lower);
                value._value = initial;

                value.Lower = _lower;
                value.Lower.OnChanged += value.BoundChanged;

                value.Upper = upper;
                value.Upper.OnChanged += value.BoundChanged;
                return value;
            }

            value = new RangeValue<T>(initial, lower, upper);
            return value;
        }

        public static RangeValue<T> Get(T initial, IValue<T> lower, T upper)
        {
            if (pool.TryDequeue(out var value))
            {
                value.releaseAction = () =>
                {
                    //
                    Property<T>.Release((Property<T>)value.Upper);
                };

                var _upper = Property<T>.Get(upper);
                value._value = initial;

                value.Lower = lower;
                value.Lower.OnChanged += value.BoundChanged;

                value.Upper = _upper;
                value.Upper.OnChanged += value.BoundChanged;
                return value;
            }

            value = new RangeValue<T>(initial, lower, upper);
            return value;
        }

        public static RangeValue<T> Get(T initial, T lower, T upper)
        {
            if (pool.TryDequeue(out var value))
            {
                value.releaseAction = () =>
                {
                    //
                    Property<T>.Release((Property<T>)value.Lower);
                    Property<T>.Release((Property<T>)value.Upper);
                };

                var _lower = Property<T>.Get(lower);
                var _upper = Property<T>.Get(upper);

                value._value = initial;

                value.Lower = _lower;
                value.Lower.OnChanged += value.BoundChanged;

                value.Upper = _upper;
                value.Upper.OnChanged += value.BoundChanged;
                return value;
            }

            value = new RangeValue<T>(initial, lower, upper);
            return value;
        }

        public static void Release(RangeValue<T> value)
        {
            value.OnRelease();
            pool.Enqueue(value);
        }

        public void Release()
        {
            Release(this);
        }
    }
}