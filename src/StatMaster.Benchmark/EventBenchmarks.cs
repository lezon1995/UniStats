using System;
using System.Collections.Generic;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using UniStats;

namespace UniStats.Benchmarks
{
    /**
    |                         Method |       Mean |     Error |    StdDev |
    |------------------------------- |-----------:|----------:|----------:|
    |                   NewEventArgs |  2.9348 ns | 0.0738 ns | 0.0616 ns |
    |          StaticCachedEventArgs |  0.0000 ns | 0.0000 ns | 0.0000 ns |
    |           DictCachedEventArgsA | 15.2306 ns | 0.0291 ns | 0.0258 ns |
    |           DictCachedEventArgsB | 15.2026 ns | 0.0157 ns | 0.0147 ns |
    |   ReadOnlyDictCachedEventArgsA | 12.0598 ns | 0.0194 ns | 0.0181 ns |
    |   ReadOnlyDictCachedEventArgsB | 13.6808 ns | 0.0911 ns | 0.0761 ns |
    |    ConditionalStaticEventArgsA |  2.4862 ns | 0.0062 ns | 0.0055 ns |
    |    ConditionalStaticEventArgsB |  8.4233 ns | 0.1910 ns | 0.1787 ns |
    | TwoConditionalStaticEventArgsA |  2.3308 ns | 0.0156 ns | 0.0146 ns |
    | TwoConditionalStaticEventArgsB |  6.9848 ns | 0.0279 ns | 0.0261 ns |
    | TwoConditionalStaticEventArgsC | 12.2878 ns | 0.1344 ns | 0.1257 ns |
     */
    public class EventBenchmarks
    {
        IValue<int> a = new Property<int>(0);
        IValue<int> b;
        IValue<int> c;
        static PropertyChangedEventArgs args = new PropertyChangedEventArgs(nameof(a));
        static PropertyChangedEventArgs argsB = new PropertyChangedEventArgs(nameof(b));
        Dictionary<string, PropertyChangedEventArgs> cache = new Dictionary<string, PropertyChangedEventArgs>();
        Dictionary<string, PropertyChangedEventArgs> readOnlyCache = new Dictionary<string, PropertyChangedEventArgs>(10);

        public EventBenchmarks()
        {
            b = a.Select(x => x + 1);
            readOnlyCache[nameof(a)] = new PropertyChangedEventArgs(nameof(a));
        }

        PropertyChangedEventArgs GetReadOnlyDictCached(string name)
        {
            if (readOnlyCache.TryGetValue(name, out var eventArgs))
            {
                return eventArgs;
            }

            return new PropertyChangedEventArgs(name);
        }

        PropertyChangedEventArgs GetDictCached(string name)
        {
            if (cache.TryGetValue(name, out var eventArgs))
            {
                return eventArgs;
            }

            cache[name] = eventArgs = new PropertyChangedEventArgs(name);
            return eventArgs;
        }

        PropertyChangedEventArgs GetOneCached(string name)
        {
            return name == nameof(a) ? args : new PropertyChangedEventArgs(name);
        }

        PropertyChangedEventArgs GetTwoCached(string name)
        {
            return name switch
            {
                nameof(a) => args,
                nameof(b) => argsB,
                _ => new PropertyChangedEventArgs(name)
            };
        }

        [Benchmark]
        public PropertyChangedEventArgs NewEventArgs() => new PropertyChangedEventArgs(nameof(a));

        [Benchmark]
        public PropertyChangedEventArgs StaticCachedEventArgs() => args;

        [Benchmark]
        public PropertyChangedEventArgs DictCachedEventArgsA() => GetDictCached(nameof(a));

        [Benchmark]
        public PropertyChangedEventArgs DictCachedEventArgsB() => GetDictCached(nameof(b));

        [Benchmark]
        public PropertyChangedEventArgs ReadOnlyDictCachedEventArgsA() => GetReadOnlyDictCached(nameof(a));

        [Benchmark]
        public PropertyChangedEventArgs ReadOnlyDictCachedEventArgsB() => GetReadOnlyDictCached(nameof(b));

        [Benchmark]
        public PropertyChangedEventArgs ConditionalStaticEventArgsA() => GetOneCached(nameof(a));

        [Benchmark]
        public PropertyChangedEventArgs ConditionalStaticEventArgsB() => GetOneCached(nameof(b));

        [Benchmark]
        public PropertyChangedEventArgs TwoConditionalStaticEventArgsA() => GetTwoCached(nameof(a));

        [Benchmark]
        public PropertyChangedEventArgs TwoConditionalStaticEventArgsB() => GetTwoCached(nameof(b));

        [Benchmark]
        public PropertyChangedEventArgs TwoConditionalStaticEventArgsC() => GetTwoCached(nameof(c));
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<EventBenchmarks>();
        }
    }
}