using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link2Pi
{
    internal class Program
    {
        static string CleanUpUnits(string units)
        {
            // Replace common unit abbreviations with their full forms or symbols
            units = units.Replace("deg", "\xB0"); // Degree symbol
            if (units == "g") units = "G";
            if (units == "C") units = "\xB0\x43"; // degrees Celsius symbol
            return units;
        }

        static void Main(string[] args)
        {
            //Check argumants and open files
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Link2Pi <data_file>");
                return;
            }
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found: " + args[0]);
                return;
            }

            string infile = args[0];
            string outfile = args[0] + ".txt";
            int iName = 19;

            // Read all lines from the CSV file
            var lines = File.ReadAllLines(infile)
            .Select(line => line.Replace("\"", ""))
            .Skip(iName)
            .ToArray();
           

            // Split each line into fields
            var rows = lines.Select(line => line.Split(',')).ToList();

            // Transpose rows to columns
            int columnCount = rows[0].Length;
            var columns = new List<List<string>>();

            for (int col = 0; col < columnCount; col++)
            {
                var column = new List<string>();
                foreach (var row in rows)
                {
                    if (row.Length < columnCount) continue; //skip the header rows
                    column.Add(row[col]);
                }
                columns.Add(column);
            }

            // Write the Pi file header
            var writer = new StreamWriter(outfile, false, Encoding.GetEncoding(1252));      //Important to use the 1252 encoding to match Pi Toolbox ASCII format      
            // File header

            writer.WriteLine("PiToolboxVersionedASCIIDataSet");
            writer.WriteLine("Version\t2");
            writer.WriteLine();
            writer.WriteLine("{OutingInformation}");
            writer.WriteLine($"CarName\tLink");
            writer.WriteLine("FirstLapNumber\t0");

            // Cycle through and create channel blocks
            for (int i = 1; i < columns.Count; i++) //skip the row with the timestamp
            {
                //extract units and name from string

                string channelName = columns[i][0];
                string units = columns[i][1];

                //fix units representation
                units = CleanUpUnits(units);

                writer.WriteLine();
                writer.WriteLine("{ChannelBlock}");
                writer.WriteLine($"Time\t{channelName}[{units}]");

                for (int j = 2; j < columns[i].Count; j++) 
                {
                    writer.WriteLine($"{columns[0][j]}\t{columns[i][j]}");
                }
            }
            writer.Close();
        }
    }
}
