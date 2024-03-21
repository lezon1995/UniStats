using System.Collections.Generic;

namespace StatMaster
{
    public delegate void ChangeHandler();

    public delegate void ChangeHandler<T>(T pre, T now);

    public interface IValueChanged<T>
    {
        event ChangeHandler<T> OnChanged;
    }

    public interface IValueChanged
    {
        event ChangeHandler OnChanged;
    }

    public interface IPool
    {
        void Release();
        void OnRelease();
    }

    public interface IValue<T> : IValueChanged<T>, IPool
    {
        T Value { get; set; }
    }

    public interface IRange<T>
    {
        T Min { get; }
        T Max { get; }
    }

    public interface IModListValue<T> : IValue<T>
    {
        IPriorityList<IMod<T>> Mods { get; }
        void Add(IMod<T> mod);
        void Add(int priority, IMod<T> mod);
        bool Remove(IMod<T> mod);
        bool Contains(IMod<T> mod);
        void Clear();
    }

    public interface IModValue<out S, T> : IModListValue<T>
    {
        S Initial { get; }
    }

    public interface IModValue<T> : IModValue<IValue<T>, T>
    {
    }

    public interface IPriorityList<T> : ICollection<T>
    {
        T this[int index] { get; }
    }

    public interface IMod<T> : IValueChanged, IPool
    {
        bool Enabled { get; set; }
        T Modify(T given);
    }

    public interface IMod<out S, T> : IMod<T>
    {
        S Context { get; }
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