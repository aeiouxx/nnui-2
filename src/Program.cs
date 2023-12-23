using GeneticTAP.Algorithm;

namespace GeneticTAP
{
    internal sealed class Program
    {
        const string Filename = "Pubs.xlsx";
        static void Main(string[] args)
        {
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, Filename);
            if (File.Exists(path))
            {
                Console.WriteLine("File found, parsing...");
                var parser = new ExcelParser();
                if (parser.ReadPubsFromFile(path) is List<Pub> pubs)
                {
                    Console.WriteLine($"Pubs (count: {pubs.Count}) parsed successfully: ");
                    Console.WriteLine($"{"NAME",-40}{"LATITUDE",-20}{"LONGITUDE",-20}");
                    foreach (var pub in pubs)
                    {
                        Console.WriteLine($"{pub.Name,-40}{pub.Latitude,-20}{pub.Longitude,-20}");
                    }
                    var runner = new GenetakRunner(pubs.ToArray(), 1_000_000)
                    {
                        Generations = 50,
                        TournamentSize = 50,
                        MutationRate = 0.01,
                        CrossoverRate = 0.8,
                        ElitismCount = 125,
                    };
                    var bestSolution = runner.Run();
                }
                else
                {
                    Console.Error.WriteLine("Error parsing pubs.");
                }
            }
            else
            {
                Console.Error.WriteLine(string.Format("File {0} not found in directory: {1}", Filename, directory));
            }
        }
    }
}