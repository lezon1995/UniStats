using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace UniStats
{
    [Serializable]
    public class ModValue<S, T> : IModValue<S, T> where S : IValue<T>
    {
        List<IMod<T>> _mods = new();

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        int _addCount;

        public IList<IMod<T>> Mods => _mods;

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
        protected S _initial;

        public virtual S Initial => _initial;

#if UNITY_5_3_OR_NEWER
        [SerializeField]
#endif
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
            _initial.OnChanged += OnChange;
        }

        T ComputeValue()
        {
            T v = Initial.Value;
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

        public void Add(IMod<T> mod)
        {
            Add(0, mod);
        }

        public void Add(int priority, IMod<T> mod)
        {
            mod.OnChanged -= OnModifiersChanged;
            mod.OnChanged += OnModifiersChanged;
            mod.Key = new Key(priority, ++_addCount);
            _mods.Add(mod);
            _mods.Sort(Comparison);
            OnChangeModifiers();
        }

        int Comparison(IMod<T> x, IMod<T> y)
        {
            int result = x.Key.Priority.CompareTo(y.Key.Priority);
            if (result != 0)
            {
                return result;
            }

            return x.Key.Age.CompareTo(y.Key.Age);
        }

        public bool Remove(IMod<T> mod, bool releaseMod = true)
        {
            if (mod == null)
                return false;

            int index = -1;
            for (var i = 0; i < _mods.Count; i++)
            {
                if (_mods[i] == mod)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
                return false;

            InternalRemove(mod, index, releaseMod);
            return true;
        }

        public bool Remove(string key, bool releaseMod = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                var mod = _mods[i];
                if (mod.Name == key)
                {
                    InternalRemove(mod, i, releaseMod);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveAll(string key, bool releaseMod = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            int count = 0;
            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                var mod = _mods[i];
                if (mod.Name == key)
                {
                    InternalRemove(mod, i, releaseMod);
                    count++;
                }
            }

            return count > 0;
        }

        public void Clear(bool releaseMod)
        {
            for (var i = _mods.Count - 1; i >= 0; i--)
            {
                IMod<T> mod = _mods[i];
                InternalRemove(mod, i, releaseMod);
            }
        }

        void InternalRemove(IMod<T> mod, int index, bool releaseMod)
        {
            _mods.RemoveAt(index);

            if (releaseMod)
            {
                mod.OnChanged -= OnModifiersChanged;
                mod.Release();
            }

            OnChangeModifiers();
        }

        public bool Contains(IMod<T> mod)
        {
            for (var i = 0; i < _mods.Count; i++)
            {
                if (_mods[i] == mod)
                    return true;
            }

            return false;
        }

        internal void OnChangeModifiers()
        {
            _dirty = true;
            var pre = _cache;
            var now = Value;
            OnChanged?.Invoke(pre, now);
        }

        internal void OnChange(T pre, T now)
        {
            _dirty = true;
            var _pre = _cache;
            var _now = Value;
            OnChanged?.Invoke(_pre, _now);
        }

        internal void OnModifiersChanged()
        {
            _dirty = true;
            OnChanged?.Invoke(_cache, Value);
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
            _mods.Clear();
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

            for (var i = 0; i < _mods.Count; i++)
            {
                builder.Append(_mods[i]);
                builder.Append(' ');
            }

            builder.Append("-> ");
            builder.Append(Value);
            return builder.ToString();
        }
    }

#if UNITY_5_3_OR_NEWER
    [Serializable]
    public class ModValue<T> : ModValue<Property<T>, T>, IModValue<T>
    {
        Action releaseAction;

        public ModValue(Property<T> initial) : base(initial)
        {
        }

        public ModValue(T initial) : base(Property<T>.Get(initial))
        {
            releaseAction = () =>
            {
                //
                _initial.OnChanged -= OnChange;
                Property<T>.Release(_initial);
            };
        }

        public ModValue() : this(default(T))
        {
        }

        IValue<T> IModValue<IValue<T>, T>.Initial => _initial;

        public override void OnRelease()
        {
            releaseAction?.Invoke();
            base.OnRelease();
        }
    }
#else
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
                _initial.OnChanged -= OnChange;
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
#endif

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

        public override T Value
        {
            get => RangeValue<T>.Clamp(base.Value, Min, Max);
        }

        public override void OnRelease()
        {
            releaseAction?.Invoke();
            base.OnRelease();
        }
    }
}