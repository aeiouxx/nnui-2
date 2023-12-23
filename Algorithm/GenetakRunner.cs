using GeneticTAP.Utilities;

namespace GeneticTAP.Algorithm
{
    internal class GenetakRunner
    {
        private static int seed = Environment.TickCount;
        #region Data
        private readonly Pub[] _pubs;
        private readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));
        private readonly DistanceCache _distanceCache;

        private Chromosome[] _population;
        private readonly int[] _sortedGenome;
        private int GenomeLength => _sortedGenome.Length;
        #endregion
        #region Parameters
        /// <summary>
        /// How many chromosomes in the population.
        /// </summary>
        public int PopulationSize
        { get; set; }
        /// <summary>
        /// How many random chromosomes to select from the population for the tournament.
        /// </summary>
        public int TournamentSize
        { get; set; }
        /// <summary>
        /// How many generations to run the algorithm for.
        /// Could also use a different termination condition such as no improvement in the last N generations, etc.
        /// </summary>
        public int Generations
        { get; set; }
        /// <summary>
        /// Chance of a chromosome's genome mutating.
        /// </summary>
        public double MutationRate
        { get; set; }
        /// <summary>
        /// Chance for two chromosomes to swap parts of their genomes.
        /// </summary>
        public double CrossoverRate
        { get; set; }
        /// <summary>
        /// How many fittest chromosomes to keep from the previous generation.
        /// </summary>
        public int ElitismCount
        { get; set; }
        #endregion
        public GenetakRunner(Pub[] pubs, int populationSize)
        {
            /// <summary>
            /// https://en.wikipedia.org/wiki/Haversine_formula
            /// </summary>
            double CalculateDistance(int pub1, int pub2)
            {
                var earthRadiusEstimate = 6371.0;
                var dLatitude = MathHelpers.DegreesToRadians(_pubs[pub2].Latitude - _pubs[pub1].Latitude);
                var dLongitude = MathHelpers.DegreesToRadians(_pubs[pub2].Longitude - _pubs[pub1].Longitude);
                var a = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2) +
                        Math.Cos(MathHelpers.DegreesToRadians(_pubs[pub1].Latitude)) * Math.Cos(MathHelpers.DegreesToRadians(_pubs[pub2].Latitude)) *
                        Math.Sin(dLongitude / 2) * Math.Sin(dLongitude / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var distance = earthRadiusEstimate * c;
                return distance;
            }
            PopulationSize = populationSize;
            _pubs = pubs;
            _population = new Chromosome[PopulationSize];
            _sortedGenome = Enumerable.Range(0, _pubs.Length).ToArray();
            _distanceCache = new DistanceCache(CalculateDistance);
        }
        /// <summary>
        /// Don't touch the algorithm parameters until run is finished or you'll go to jail.
        /// </summary>
        public Chromosome? Run()
        {
            Console.WriteLine("==============================================================================");
            Console.WriteLine("RUNNING GENETAK");
            Console.WriteLine("==============================================================================");
            Parallel.For(0, PopulationSize, i =>
            {
                var chromosome = new Chromosome(GetRandomGenome());
                chromosome.Fitness = CalculateFitness(chromosome);
                _population[i] = chromosome;
            });

            for (int generation = 0; generation < Generations; generation++)
            {
                var newPopulation = new Chromosome[PopulationSize];
                if (ElitismCount > 0)
                {
                    AddElites(newPopulation);
                }
                for (int initialized = ElitismCount; initialized < PopulationSize;)
                {
                    Chromosome firstCandidate = TournamentSelect();
                    Chromosome secondCandidate = TournamentSelect();
                    if (_random.Value.NextDouble() < CrossoverRate)
                    {
                        (firstCandidate, secondCandidate) = Crossover(firstCandidate, secondCandidate);
                    }
                    if (_random.Value.NextDouble() < MutationRate)
                    {
                        Mutate(firstCandidate);
                    }
                    if (_random.Value.NextDouble() < MutationRate)
                    {
                        Mutate(secondCandidate);
                    }
                    if (initialized < PopulationSize)
                    {
                        newPopulation[initialized++] = firstCandidate;
                    }
                    if (initialized < PopulationSize)
                    {
                        newPopulation[initialized++] = secondCandidate;
                    }
                }
                var babyGoat = newPopulation.MaxBy(c => c.Fitness);
                Console.WriteLine($"Generation {generation,3} best: {-babyGoat.Fitness:F3} km.");
                _population = newPopulation;
            }
            var goat = _population.MaxBy(c => c.Fitness);
            var missingPubs = _sortedGenome.Except(goat.Genome);
            if (missingPubs.Any())
            {
                Console.WriteLine("==============================================================================");
                Console.WriteLine("GENETAK FAILED");
                Console.WriteLine("==============================================================================");
                Console.WriteLine("Missing pubs:");
                foreach (var pub in missingPubs)
                {
                    Console.WriteLine($"{_pubs[pub].Name}");
                }
                return null;
            }
            Console.WriteLine("==============================================================================");
            Console.WriteLine("GENETAK FINISHED");
            Console.WriteLine("==============================================================================");
            Console.WriteLine("Best solution:");
            foreach (var pub in goat.Genome)
            {
                Console.WriteLine($"{_pubs[pub].Name}");
            }
            Console.WriteLine($"Distance: {-goat.Fitness:F3} km.");


            return goat;
        }


        #region Stuff
        private int[] GetRandomGenome()
        {
            var genome = new int[_sortedGenome.Length];
            Array.Copy(_sortedGenome, genome, _sortedGenome.Length);
            Shuffle(genome);
            return genome;
        }
        private void Shuffle(int[] array)
        {
            var random = _random.Value;
            int n = array.Length;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
        private double CalculateFitness(Chromosome chromosome)
        {
            double fitness = 0;
            for (int i = 0; i < chromosome.Genome.Length - 1; i++)
            {
                fitness -= _distanceCache.GetCreateDistance(chromosome.Genome[i], chromosome.Genome[i + 1]);
            }
            return fitness;
        }

        private Chromosome TournamentSelect()
        {
            Chromosome? goated = null;
            for (int i = 0; i < TournamentSize; i++)
            {
                Chromosome randomSelection = _population[_random.Value.Next(PopulationSize)];
                if (goated == null || randomSelection.Fitness > goated.Fitness)
                {
                    goated = randomSelection;
                }
            }

            return (Chromosome)goated.Clone();
        }

        private void AddElites(Chromosome[] toFill)
        {
            PriorityQueue<Chromosome, double> queue = new(ElitismCount);
            foreach (var chromosome in _population)
            {
                if (queue.Count < ElitismCount)
                {
                    queue.Enqueue(chromosome, chromosome.Fitness);
                }
                else if (chromosome.Fitness > queue.Peek().Fitness)
                {
                    queue.Dequeue();
                    queue.Enqueue(chromosome, chromosome.Fitness);
                }
            }
            for (int i = 0; i < ElitismCount; i++)
            {
                toFill[i] = (Chromosome)queue.Dequeue().Clone();
            }
        }

        /// <summary>
        /// Find a random range of genes from parent 1/2 and copy them to kids 2/1.
        /// Copy the rest of the genes from parent 1/2 to kids 1/2.
        /// </summary>
        /// <param name="mommy"></param>
        /// <param name="daddy"></param>
        /// <returns></returns>
        // TODO: Refactor and test this unreadable garbage
        private (Chromosome kidOne, Chromosome kidTwo) Crossover(Chromosome mommy, Chromosome daddy)
        {
            var startInclusive = _random.Value.Next(GenomeLength - 2);
            var endInclusive = _random.Value.Next(startInclusive + 1, GenomeLength - 1);
            var kidOneGenome = new int[GenomeLength];
            var kidTwoGenome = new int[GenomeLength];
            var kidOneCopiedGenes = new bool[GenomeLength];
            var kidTwoCopiedGenes = new bool[GenomeLength];
            for (int i = startInclusive; i <= endInclusive; i++)
            {
                kidOneGenome[i] = daddy.Genome[i];
                kidTwoGenome[i] = mommy.Genome[i];

                kidOneCopiedGenes[daddy.Genome[i]] = true;
                kidTwoCopiedGenes[mommy.Genome[i]] = true;
            }
            int kidOneCurrentGeneIndex = (endInclusive + 1) % GenomeLength;
            int kidTwoCurrentGeneIndex = (endInclusive + 1) % GenomeLength;
            int genesToCopy = GenomeLength - (endInclusive - startInclusive + 1);
            int mommyCopyGeneIndex = kidOneCurrentGeneIndex;
            int daddyCopyGeneIndex = kidTwoCurrentGeneIndex;
            for (int i = 0; i < genesToCopy; i++)
            {
                while (kidOneCopiedGenes[mommy.Genome[mommyCopyGeneIndex]])
                {
                    mommyCopyGeneIndex = (mommyCopyGeneIndex + 1) % GenomeLength;
                }
                kidOneGenome[kidOneCurrentGeneIndex] = mommy.Genome[mommyCopyGeneIndex];
                kidOneCopiedGenes[kidOneGenome[kidOneCurrentGeneIndex]] = true;
                kidOneCurrentGeneIndex = (kidOneCurrentGeneIndex + 1) % GenomeLength;
                while (kidTwoCopiedGenes[daddy.Genome[daddyCopyGeneIndex]])
                {
                    daddyCopyGeneIndex = (daddyCopyGeneIndex + 1) % GenomeLength;
                }
                kidTwoGenome[kidTwoCurrentGeneIndex] = daddy.Genome[daddyCopyGeneIndex];
                kidTwoCopiedGenes[kidTwoGenome[kidTwoCurrentGeneIndex]] = true;
                kidTwoCurrentGeneIndex = (kidTwoCurrentGeneIndex + 1) % GenomeLength;
            }
            Chromosome kidOne = new Chromosome(kidOneGenome);
            kidOne.Fitness = CalculateFitness(kidOne);
            Chromosome kidTwo = new Chromosome(kidTwoGenome);
            kidTwo.Fitness = CalculateFitness(kidTwo);
            return (kidOne, kidTwo);
        }

        /// <summary>
        /// Simply swap two random genes in the genome.
        /// </summary>
        /// <param name="chromosome"></param>
        private void Mutate(Chromosome chromosome)
        {
            int firstIndex = _random.Value.Next(GenomeLength);
            int secondIndex = _random.Value.Next(GenomeLength);
            while (firstIndex == secondIndex)
            {
                secondIndex = _random.Value.Next(GenomeLength);
            }
            (chromosome.Genome[firstIndex], chromosome.Genome[secondIndex]) = (chromosome.Genome[secondIndex], chromosome.Genome[firstIndex]);
            chromosome.Fitness = CalculateFitness(chromosome);
        }
        #endregion
    }
}
