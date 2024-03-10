#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace StatMaster
{
    /// <summary>
    /// Represents a stat that can be modified.
    /// Value = ((Initial + InitialPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
    /// </summary>
    public class Stat<T> : ModValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        public IModValue<T> InitialPlus { get; }
        public IModValue<T> BasePlus { get; }
        public IModValue<T> BaseTimes { get; }
        public IModValue<T> TotalPlus { get; }
        public IModValue<T> TotalTimes { get; }

        public Stat(T initial) : base(initial)
        {
            InitialPlus = new ModValue<T>();
            BasePlus = new ModValue<T>();
            BaseTimes = new ModValue<T>(One());
            TotalPlus = new ModValue<T>();
            TotalTimes = new ModValue<T>(One());

            InitializeModifiers();
        }

        void InitializeModifiers()
        {
            Add(100, Mod.Add(InitialPlus));
            Add(200, Mod.Mul(BaseTimes));
            Add(300, Mod.Add(BasePlus));
            Add(400, Mod.Mul(TotalTimes));
            Add(500, Mod.Add(TotalPlus));
        }

        #region Utility Methods

        /// <summary>
        /// Returns the value "one" of type T.
        /// </summary>
        /// <returns>The value "one" of type T.</returns>
        static T One()
        {
#if NET7_0_OR_GREATER
            return T.One;
#else
            return Mod.GetOperator<T>().One;
#endif
        }

        #endregion Utility Methods
    }
}