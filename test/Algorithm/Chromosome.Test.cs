using GeneticTAP.Algorithm;

namespace GeneticTAP.Tests.Algorithm
{
    public class Chromosome_Test
    {
        [Fact]
        public void CrossoverTest_CompleteSwap_ChildShouldEqualParent()
        {
            var genomeA = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var genomeB = new int[] { 7, 6, 5, 4, 3, 2, 1, 0 };
            var start = 0;
            var end = 7;
            var genomeInheritFromACrossoverFromB = new int[] { 7, 6, 5, 4, 3, 2, 1, 0 };
            var genomeInheritFromBCrossoverFromA = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            var (childInheritFromACrossoverFromB, childInheritFromBCrossoverFromA)
                = new Chromosome(genomeA).CrossoverWith(new Chromosome(genomeB), start, end, _ => 0);
            Assert.Equal(genomeInheritFromACrossoverFromB, childInheritFromACrossoverFromB.Genome);
            Assert.Equal(genomeInheritFromBCrossoverFromA, childInheritFromBCrossoverFromA.Genome);
        }

        [Fact]
        public void CrossoverTest_PartialUntilEnd()
        {
            var genomeA = new int[] { 3, 2, 1, 0, 4 };
            var genomeB = new int[] { 0, 1, 2, 3, 4 };
            var start = 3;
            var end = 4;
            var genomeInheritFromACrossoverFromB = new int[] { 2, 1, 0, 3, 4 };
            var genomeInheritFromBCrossoverFromA = new int[] { 1, 2, 3, 0, 4 };
            var (childInheritFromACrossoverFromB, childInheritFromBCrossoverFromA)
                = new Chromosome(genomeA).CrossoverWith(new Chromosome(genomeB), start, end, _ => 0);
            Assert.Equal(genomeInheritFromACrossoverFromB, childInheritFromACrossoverFromB.Genome);
            Assert.Equal(genomeInheritFromBCrossoverFromA, childInheritFromBCrossoverFromA.Genome);
        }

        [Fact]
        public void CrossoverTest_PartialFromStart()
        {
            var genomeA = new int[] { 5, 1, 7, 8, 4, 0, 6, 2, 3, 9 };
            var genomeB = new int[] { 9, 1, 2, 5, 3, 4, 0, 8, 7, 6 };

            var start = 0;
            var end = 4;

            var genomeInheritFromACrossoverFromB = new int[] { 9, 1, 2, 5, 3, 0, 6, 7, 8, 4 };
            var genomeInheritFromBCrossoverFromA = new int[] { 5, 1, 7, 8, 4, 0, 6, 9, 2, 3 };

            var (childInheritFromACrossoverFromB, childInheritFromBCrossoverFromA)
                = new Chromosome(genomeA).CrossoverWith(new Chromosome(genomeB), start, end, _ => 0);

            Assert.Equal(genomeInheritFromACrossoverFromB, childInheritFromACrossoverFromB.Genome);
            Assert.Equal(genomeInheritFromBCrossoverFromA, childInheritFromBCrossoverFromA.Genome);
        }


        [Fact]
        public void CrossoverTest_SwapInside()
        {
            var genomeA = new int[] { 2, 4, 0, 3, 1 };
            var genomeB = new int[] { 0, 3, 2, 1, 4 };

            var start = 2;
            var end = 3;

            var genomeInheritFromACrossoverFromB = new int[] { 0, 3, 2, 1, 4 };
            var genomeInheritFromBCrossoverFromA = new int[] { 2, 1, 0, 3, 4 };


            var (childInheritFromACrossoverFromB, childInheritFromBCrossoverFromA)
                = new Chromosome(genomeA).CrossoverWith(new Chromosome(genomeB), start, end, _ => 0);

            Assert.Equal(genomeInheritFromACrossoverFromB, childInheritFromACrossoverFromB.Genome);
            Assert.Equal(genomeInheritFromBCrossoverFromA, childInheritFromBCrossoverFromA.Genome);
        }

        [Fact]
        public void CrossoverTest_SwapSingle()
        {
            var genomeA = new int[] { 0, 1, 2, 3, 4 };
            var genomeB = new int[] { 4, 3, 2, 1, 0 };


            var swapIndex = 1;

            var genomeInheritFromACrossoverFromB = new int[] { 1, 3, 2, 4, 0 };
            var genomeInheritFromBCrossoverFromA = new int[] { 3, 1, 2, 0, 4 };


            var (childInheritFromACrossoverFromB, childInheritFromBCrossoverFromA)
                = new Chromosome(genomeA).CrossoverWith(new Chromosome(genomeB), swapIndex, swapIndex, _ => 0);

            Assert.Equal(genomeInheritFromACrossoverFromB, childInheritFromACrossoverFromB.Genome);
            Assert.Equal(genomeInheritFromBCrossoverFromA, childInheritFromBCrossoverFromA.Genome);
        }

    }
}
