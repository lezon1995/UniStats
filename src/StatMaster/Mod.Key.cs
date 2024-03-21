using System;

namespace UniStats
{
    public struct Key : IComparable<Key>, IEquatable<Key>
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

        public override string ToString()
        {
            return $"{Priority}-{Age}";
        }
    }
}