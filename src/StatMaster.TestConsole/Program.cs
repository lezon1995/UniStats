using UniStats;

internal class Program
{
    public static void Main(string[] args)
    {
        var health = new ModValue<float>(100f);

        Console.WriteLine($"Health is {health.Value}.");

// Output: Health is 100.

        for (int i = 0; i < 100; i++)
        {
            var mod = Mod.Mul(1.10f);
            health.Add(mod);
            Console.WriteLine($"Health is {health.Value}.");

// Output: Health is 110.
            health.Remove(mod);
            Console.WriteLine($"Health is {health.Value}.");
        }
    }
}

// Output: Health is 115.