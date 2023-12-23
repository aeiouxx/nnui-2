using GeneticTAP.Utilities;
using System.Diagnostics;

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

        public (Chromosome childOfThis, Chromosome childOfOther) CrossoverWith(Chromosome other, int crossoverStartInclusive, int crossoverEndInclusive,
            Func<Chromosome, double> fitnessEvaluator)
        {
            Debug.Assert(Genome.Length == other.Genome.Length);
            var genomeLength = Genome.Length;

            var childOfThisGenome = new int[genomeLength];

            // This is only usable because we know the genome is a permutation of numbers 0..genomeLength-1,
            // not applicable to other genomes (would need a hash set or something).
            Span<bool> childOfThisDuplicitGeneLookup = stackalloc bool[genomeLength];

            var childOfOtherGenome = new int[genomeLength];
            Span<bool> childOfOtherDuplicitGeneLookup = stackalloc bool[genomeLength];

            for (int i = crossoverStartInclusive; i <= crossoverEndInclusive; i++)
            {
                var childOfThisCrossGene = other.Genome[i];
                childOfThisGenome[i] = childOfThisCrossGene;
                childOfThisDuplicitGeneLookup[childOfThisCrossGene] = true;

                var childOfOtherCrossGene = Genome[i];
                childOfOtherGenome[i] = childOfOtherCrossGene;
                childOfOtherDuplicitGeneLookup[childOfOtherCrossGene] = true;
            }

            int childOfThisCurrentGeneIndex = (crossoverEndInclusive + 1) % genomeLength;
            int childOfOtherCurrentGeneIndex = (crossoverEndInclusive + 1) % genomeLength;
            int genesToCopy = genomeLength - (crossoverEndInclusive - crossoverStartInclusive + 1);
            int thisCopyGeneIndex = childOfThisCurrentGeneIndex;
            int otherCopyGeneIndex = childOfOtherCurrentGeneIndex;

            while (genesToCopy > 0)
            {
                CopyGeneFromParentSkipDuplicates(this, ref thisCopyGeneIndex, childOfThisGenome, ref childOfThisCurrentGeneIndex, childOfThisDuplicitGeneLookup);
                CopyGeneFromParentSkipDuplicates(other, ref otherCopyGeneIndex, childOfOtherGenome, ref childOfOtherCurrentGeneIndex, childOfOtherDuplicitGeneLookup);
                genesToCopy--;
            }
            Chromosome childOfThis = new Chromosome(childOfThisGenome);
            childOfThis.Fitness = fitnessEvaluator(childOfThis);
            Chromosome childOfOther = new Chromosome(childOfOtherGenome);
            childOfOther.Fitness = fitnessEvaluator(childOfOther);

            return (childOfThis, childOfOther);

            #region Local functions
            static void CopyGeneFromParentSkipDuplicates(Chromosome parent, ref int parentCopyGeneIndex, int[] childGenome, ref int childCurrentGeneIndex, Span<bool> duplicitGeneLookup)
            {
                bool isDuplicitGeneForChild = duplicitGeneLookup[parent.Genome[parentCopyGeneIndex]];
                while (isDuplicitGeneForChild)
                {
                    parentCopyGeneIndex = (parentCopyGeneIndex + 1) % childGenome.Length;
                    isDuplicitGeneForChild = duplicitGeneLookup[parent.Genome[parentCopyGeneIndex]];
                }
                var childCopiedGene = parent.Genome[parentCopyGeneIndex];
                Debug.Assert(duplicitGeneLookup[childCopiedGene] == false);
                childGenome[childCurrentGeneIndex] = childCopiedGene;
                duplicitGeneLookup[childCopiedGene] = true;

                childCurrentGeneIndex = (childCurrentGeneIndex + 1) % childGenome.Length;
            }
            #endregion
        }
        public (Chromosome childOfThis, Chromosome childOfOther) CrossoverWith(Chromosome other, Func<Chromosome, double> fitnessEvaluator)
        {
            var size = Genome.Length;
            var startInclusive = Randomizer.Generator.Next(size - 1);
            var endInclusive = Randomizer.Generator.Next(startInclusive, size - 1);
            return CrossoverWith(other, startInclusive, endInclusive, fitnessEvaluator);
        }
        public void Mutate()
        {
            var size = Genome.Length;
            var firstIndex = Randomizer.Generator.Next(size);
            var secondIndex = Randomizer.Generator.Next(size);
            while (firstIndex == secondIndex)
            {
                secondIndex = Randomizer.Generator.Next(size);
            }
            (Genome[firstIndex], Genome[secondIndex]) = (Genome[secondIndex], Genome[firstIndex]);
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
