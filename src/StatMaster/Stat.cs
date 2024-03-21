#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace UniStats
{
    /// <summary>
    /// Represents a stat that can be modified.
    /// Initial           原始初始值
    /// 
    /// InitialPlus     额外初始值
    /// BaseTimes    初始值百分比
    /// BasePlus       额外基础值
    /// TotalTimes   基础值百分比
    /// TotalPlus      额外总值
    /// Value = ((Initial + InitialPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
    /// </summary>
    public class Stat<T> : ModValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#else
        where T : struct
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
            Add(100, Mod.Add(InitialPlus, nameof(InitialPlus)));
            Add(200, Mod.Mul(BaseTimes, nameof(BaseTimes)));
            Add(300, Mod.Add(BasePlus, nameof(BasePlus)));
            Add(400, Mod.Mul(TotalTimes, nameof(TotalTimes)));
            Add(500, Mod.Add(TotalPlus, nameof(TotalPlus)));
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

        public override string ToString()
        {
            return $"{Value} = (({Initial.Value} + {InitialPlus.Value}) * {BaseTimes.Value} + {BasePlus.Value}) * {TotalTimes.Value} + {TotalPlus.Value}";
        }
    }
}