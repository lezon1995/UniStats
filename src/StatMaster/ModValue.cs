using System.Collections;
using System.Numerics;
using System.Text;

namespace StatMaster
{
    [Serializable]
    public class ModValue<S, T> : IModValue<S, T> where S : IValue<T>
    {
        ModList _mods;
        ModList mods => _mods ??= new ModList(this);

        public IPriorityList<IMod<T>> Mods => _mods;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        protected S _initial;

        public virtual S Initial => _initial;

        T _cache;

        bool _dirty = true;

        public virtual T Value
        {
            get
            {
                if (_dirty)
                {
                    _cache = ComputeValue();
                    _dirty = false;
                    return _cache;
                }

                return _cache;
            }
            set { }
        }

        public event ChangeHandler<T> OnChanged;

        public ModValue(S initial)
        {
            _initial = initial;
            _initial.OnChanged += Chain;
        }

        public void Add(IMod<T> mod)
        {
            mods.Add(mod);
        }

        public void Add(int priority, IMod<T> mod)
        {
            mods.Add(priority, mod);
        }

        public bool Remove(IMod<T> mod)
        {
            if (_mods == null)
            {
                mod.Release();
                return false;
            }

            var success = _mods.Remove(mod);
            mod.Release();
            return success;
        }

        public bool Contains(IMod<T> mod)
        {
            if (_mods == null)
                return false;

            return _mods.Contains(mod);
        }

        public void Clear()
        {
            if (_mods == null)
                return;

            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                _mods.Remove(_mods[i]);
            }
        }

        protected void Chain(T pre, T now)
        {
            OnChange();
        }

        internal void OnChangeModifiers()
        {
            _dirty = true;
            var pre = _cache;
            var now = Value;
            OnChanged?.Invoke(pre, now);
        }

        internal void OnChange()
        {
            _dirty = true;
            var pre = _cache;
            var now = Value;
            OnChanged?.Invoke(pre, now);
        }

        internal void ModifiersChanged()
        {
            _dirty = true;
            var pre = _cache;
            var now = Value;
            OnChanged?.Invoke(pre, now);
        }

        public override string ToString()
        {
            return Value switch
            {
                int i => i.ToString(),
                float f => f.ToString("F1"),
                double d => d.ToString("F2"),
                _ => Value.ToString()
            };
        }

        public virtual void Release()
        {
        }

        public virtual void OnRelease()
        {
            _mods = null;
            _initial = default;
            _cache = default;
            _dirty = true;
            OnChanged = null;
        }

        public string ToString(bool showModifiers)
        {
            if (!showModifiers)
            {
                return ToString();
            }

            var builder = new StringBuilder();
            builder.Append(" \"base\" ");
            builder.Append(Initial);
            builder.Append(' ');

            if (_mods != null)
            {
                for (var i = 0; i < _mods.Count; i++)
                {
                    builder.Append(_mods[i]);
                    builder.Append(' ');
                }
            }

            builder.Append("-> ");
            builder.Append(Value);
            return builder.ToString();
        }

        T ComputeValue()
        {
            T v = Initial.Value;
            if (_mods == null)
            {
                return v;
            }

            for (var i = 0; i < _mods.Count; i++)
            {
                var mod = _mods[i];
                if (mod.Enabled)
                {
                    v = mod.Modify(v);
                }
            }

            return v;
        }

        protected class ModList : IPriorityList<IMod<T>>, IComparer<Key>
        {
            ModValue<S, T> _parent;
            SortedList<Key, IMod<T>> _mods;
            int _addCount;

            public ModList(ModValue<S, T> parent)
            {
                _parent = parent;
                _mods = new SortedList<Key, IMod<T>>();
                _addCount = 0;
            }

            public IMod<T> this[int index]
            {
                get { return _mods.GetValueAtIndex(index); }
            }

            public IEnumerator<IMod<T>> GetEnumerator()
            {
                return _mods.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IMod<T> mod)
            {
                Add(0, mod);
            }

            public void Add(int priority, IMod<T> mod)
            {
                mod.OnChanged -= _parent.ModifiersChanged;
                mod.OnChanged += _parent.ModifiersChanged;
                _mods.Add(new Key(priority, ++_addCount), mod);
                _parent.OnChangeModifiers();
            }

            public void Clear()
            {
                for (var i = 0; i < _mods.Count; i++)
                {
                    this[i].OnChanged -= _parent.ModifiersChanged;
                }

                _mods.Clear();
                _parent.OnChangeModifiers();
            }

            public bool Contains(IMod<T> mod)
            {
                return _mods.ContainsValue(mod);
            }

            public void CopyTo(IMod<T>[] array, int arrayIndex)
            {
                _mods.Values.CopyTo(array, arrayIndex);
            }

            public bool Remove(IMod<T> mod)
            {
                if (mod == null)
                    return false;

                int index = _mods.IndexOfValue(mod);
                if (index < 0)
                    return false;

                mod.OnChanged -= _parent.ModifiersChanged;
                _mods.RemoveAt(index);
                _parent.OnChangeModifiers();
                return true;
            }

            public int Count => _mods.Count;

            public bool IsReadOnly => false;

            public int Compare(Key x, Key y)
            {
                int result = x.Priority.CompareTo(y.Priority);
                if (result != 0)
                {
                    return result;
                }

                return x.Age.CompareTo(y.Age);
            }
        }

        protected struct Key : IComparable<Key>, IEquatable<Key>
        {
            public int Priority;
            public int Age;

            public Key(int priority, int age)
            {
                Priority = priority;
                Age = age;
            }

            int IComparable<Key>.CompareTo(Key other)
            {
                int result = Priority.CompareTo(other.Priority);
                if (result != 0)
                {
                    return result;
                }

                return Age.CompareTo(other.Age);
            }

            public bool Equals(Key other)
            {
                return Priority == other.Priority && Age == other.Age;
            }

            public override bool Equals(object obj)
            {
                return obj is Key other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Priority, Age);
            }

            public static bool operator ==(Key left, Key right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Key left, Key right)
            {
                return !(left == right);
            }
        }
    }

    [Serializable]
    public class ModValue<T> : ModValue<IValue<T>, T>, IModValue<T>
    {
        Action releaseAction;

        public ModValue(IValue<T> initial) : base(initial)
        {
        }

        public ModValue(T initial) : base(Property<T>.Get(initial))
        {
            releaseAction = () =>
            {
                //
                _initial.OnChanged -= Chain;
                Property<T>.Release((Property<T>)_initial);
            };
        }

        public ModValue() : this(default(T))
        {
        }

        public override void OnRelease()
        {
            releaseAction?.Invoke();
            base.OnRelease();
        }
    }

    [Serializable]
    public class RangeModValue<S, T> : ModValue<S, T>, IRange<T> where S : IValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        IValue<T> _min;
        IValue<T> _max;

        public T Min => _min.Value;
        public T Max => _max.Value;

        Action releaseAction;

        public RangeModValue(S initial, IValue<T> min, IValue<T> max) : base(initial)
        {
            _min = min;
            _max = max;
        }

        public RangeModValue(S value, T lower, IValue<T> upper) : this(value, Property<T>.Get(lower), upper)
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)_min);
            };
        }

        public RangeModValue(S value, IValue<T> lower, T upper) : this(value, lower, Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                //
                Property<T>.Release((Property<T>)_max);
            };
        }

        public RangeModValue(S value, T lower, T upper) : this(value, Property<T>.Get(lower), Property<T>.Get(upper))
        {
            releaseAction = () =>
            {
                Property<T>.Release((Property<T>)_min);
                Property<T>.Release((Property<T>)_max);
            };
        }

        public override T Value => RangeValue<T>.Clamp(base.Value, Min, Max);

        public override void OnRelease()
        {
            releaseAction?.Invoke();
            base.OnRelease();
        }
    }
}