using System;

namespace UniStats
{
    [Serializable]
    public struct Key
    {
        public int Priority;
        public int Age;

        public Key(int priority, int age)
        {
            Priority = priority;
            Age = age;
        }

        public override string ToString()
        {
            return $"{Priority}-{Age}";
        }
    }
}