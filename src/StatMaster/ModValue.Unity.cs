#if UNITY_5_3_OR_NEWER
using System;

namespace StatMaster.Unity
{
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
                _initial.OnChanged -= Chain;
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
}
#endif