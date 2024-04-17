using System.Collections.Generic;

namespace UniStats
{
    public delegate void ChangeHandler();
    public delegate void ChangeHandler<T>(T pre, T now);

    public interface IPool
    {
        void Release();
        void OnRelease();
    }

    public interface IValue<T> : IPool
    {
        event ChangeHandler<T> OnChanged;
        T Value { get; set; }
    }

    public interface IRange<T>
    {
        T Min { get; }
        T Max { get; }
    }

    public interface IModListValue<T> : IValue<T>
    {
        IList<IMod<T>> Mods { get; }
        void Add(IMod<T> mod);
        void Add(int priority, IMod<T> mod);
        bool Remove(IMod<T> mod, bool releaseMod = true);
        bool Remove(string key, bool releaseMod = true);
        bool RemoveAll(string key, bool releaseMod = true);
        bool Contains(IMod<T> mod);
        void Clear(bool releaseMod = true);
    }

    public interface IModValue<out S, T> : IModListValue<T>
    {
        S Initial { get; }
    }

    public interface IModValue<T> : IModValue<IValue<T>, T>
    {
    }

    public interface IMod<T> : IPool
    {
        event ChangeHandler OnChanged;
        public Key Key { get; set; }
        string Name { get; set; }
        bool Enabled { get; set; }
        T Modify(T given);
    }

    public interface ITarget<in S, T>
    {
        IMod<T> Mod { get; }
        IModListValue<T> AppliesTo(S thing);
    }

    public interface IDecorator<T>
    {
        T Decorated { get; set; }
    }
}