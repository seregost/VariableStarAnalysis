using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VariableDetector.Models;
using VariableDetector.Db;

namespace VariableDetector.Helpers
{
    public class ChartManager
    {
        public string LoadChart(string directory, StarCatalogContext db)
        {
            TextReader reader = File.OpenText(directory + "\\chart.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = "\t";
            csv.ReadHeader();

            string chartname = csv.FieldHeaders.GetValue(0).ToString();

            if(db.ChartEntries.Where(x => x.Chart == chartname).Count() == 0)
            { 
                csv.Read();
                do
                {
                    D_ChartEntry entry = new D_ChartEntry()
                    {
                        Chart = chartname,
                        AUID = csv.GetField(0).ToString(),
                        Label = csv.GetField(3).ToString(),
                        Comments = csv.GetField(6).ToString()
                    };


                    // prune off degree character
                    entry.J2000_RA = GetCoord(csv.GetField(1).ToString());
                    entry.J2000_DEC = GetCoord(csv.GetField(2).ToString());
                    entry.VMag = GetValue(csv.GetField(4).ToString());
                    entry.BVColor = GetValue(csv.GetField(5).ToString());
                    db.ChartEntries.Add(entry);
                }
                while (csv.Read());
                db.SaveChanges();
            }
            csv.Dispose();

            return chartname;
        }

        private double GetCoord(string coord)
        {
            string[] value = coord.Split('[', ']');

            return Convert.ToDouble(value[1].Substring(0, value[1].Length - 1));
        }

        private double GetValue(string input)
        {
            string[] value = input.Split('(');

            return Convert.ToDouble(value[0].Trim());
        }
    }
}
