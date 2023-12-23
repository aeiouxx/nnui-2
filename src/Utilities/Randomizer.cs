namespace GeneticTAP.Utilities
{
    internal sealed class Randomizer
    {
        public static Random Generator => _instance.Value._random.Value ?? throw new InvalidOperationException("RIP");
        private static int Seed = Environment.TickCount;
        private readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref Seed)));
        private static readonly Lazy<Randomizer> _instance = new Lazy<Randomizer>(() => new Randomizer());
    }
}
