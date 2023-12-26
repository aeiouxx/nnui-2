using ClosedXML.Excel;

namespace GeneticTAP
{
    internal sealed record Pub(string Name, double Latitude, double Longitude);
    internal sealed class ExcelParser
    {
        public bool TryReadPubsFromFile(string path, out List<Pub> pubs, int rowsToSkip = 1)
        {
            pubs = new List<Pub>();
            using (var workbook = new XLWorkbook(path))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();
                foreach (var row in rows.Skip(rowsToSkip))
                {
                    var name = row.Cell(2).Value.ToString();
                    var coordinates = row.Cell(3).Value.ToString().Split(',');
                    if (coordinates.Length != 2)
                    {
                        return false;
                    }
                    var latitude = double.Parse(coordinates[0]);
                    var longitude = double.Parse(coordinates[1]);
                    pubs.Add(new Pub(name, latitude, longitude));
                }
            }
            return true;
        }

        public List<Pub> ReadPubsFromFile(string filepath, int rowsToSkip = 1)
        {
            var pubs = new List<Pub>();
            using (var workbook = new XLWorkbook(filepath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed();
                foreach (var row in rows.Skip(rowsToSkip))
                {
                    var name = row.Cell(2).Value.ToString();
                    var coordinates = row.Cell(3).Value.ToString().Split(',');
                    if (coordinates.Length != 2)
                    {
                        throw new Exception("Invalid coordinates");
                    }
                    var latitude = double.Parse(coordinates[0]);
                    var longitude = double.Parse(coordinates[1]);
                    pubs.Add(new Pub(name, latitude, longitude));
                }
            }
            return pubs;
        }
    }
}
