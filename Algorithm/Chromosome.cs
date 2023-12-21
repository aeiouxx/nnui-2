namespace GeneticTAP.Algorithm
{
    internal sealed class Chromosome : ICloneable
    {
        /// <summary>
        /// Represents a particular solution, i.e. a permutation of the pubs.
        /// </summary>
        public int[] Genome
        { get; set; }

        /// <summary>
        /// The value we're maximizing, i.e. the negative of distance between pubs.
        /// </summary>
        public double Fitness
        { get; set; }

        public Chromosome(int size)
        {
            Genome = new int[size];
            Fitness = double.MinValue;
        }
        public Chromosome(int[] genome)
        {
            Genome = genome;
            Fitness = double.MinValue;
        }


        public object Clone()
        {
            var genome = new int[Genome.Length];
            Genome.CopyTo(genome, 0);
            return new Chromosome(genome)
            {
                Fitness = Fitness
            };
        }
    }
}
