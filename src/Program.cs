using GeneticTAP.Algorithm;

namespace GeneticTAP
{
    internal sealed class Program
    {
        const string Filename = "Pubs.xlsx";
        static void Main(string[] args)
        {
            string executableLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string executablePath = Path.GetDirectoryName(executableLocation);
            Directory.SetCurrentDirectory(executablePath);
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, Filename);
            if (File.Exists(path))
            {
                Console.WriteLine("File found, parsing...");
                var parser = new ExcelParser();
                if (parser.TryReadPubsFromFile(path, out var pubs))
                {
                    Console.WriteLine($"Pubs (count: {pubs.Count}) parsed successfully: ");
                    var runner = new GenetakRunner(pubs.ToArray(), 100_000)
                    {
                        Generations = 35,
                        TournamentSize = 50,
                        MutationRate = 0.01,
                        CrossoverRate = 0.8,
                        ElitismCount = 100,
                    };
                    var bestSolution = runner.Run();
                }
                else
                {
                    Console.Error.WriteLine($"Error while parsing pubs at location: {path}.");
                }
            }
            else
            {
                Console.Error.WriteLine(string.Format("File {0} not found in directory: {1}", Filename, directory));
            }
        }
    }
}