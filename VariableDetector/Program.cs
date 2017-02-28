using CsvHelper;
using LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Models;
using VariableDetector.Helpers;

namespace VariableDetector
{
    class Program
    {
        static int[] EXCLUDEDFRAMES = { };

        static void Main(string[] args)
        {
            //StarCatalog.BuildPPMCatalog(".\\PPM.source");
            //StarCatalog.BuildVSXCatalog(".\\VSX.source");
            var field = StarField.LoadStarField(args[0], EXCLUDEDFRAMES);
            field.ComputeMeasuredColorIndex(args[1], EXCLUDEDFRAMES);

            // Apply filter.
            field.Stars = field.Stars.OrderByDescending(x => x.AvgInstrumentalMag).Where(x=>x.Flags == 0 && x.MinSNR > 20).ToList();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", "Star", "Control", "RMag", "C-VMag", "C-VMag-E", "VMag", "C-ColorIndex", "ColorIndex",  "Saturated", "Known Variable", "Uncertainty", "Score"));
            foreach (Star star in field.Stars)
            {
                // Build representative list of comparable stars from the same field.
                List<Star> comparables = field.GetComparables(star);
                    
                Star controlstar = comparables.Where(x => x.ValidCatalogMag == true).FirstOrDefault();
                if (controlstar == null)
                    controlstar = comparables[0];

                field.CalcDifferentialMag(star, comparables);

                // Calculate ensemble standard deviation.  This establishes measurement error bounds.
                decimal error =
                    field.CalcEnsembleError(star, comparables);

                field.CalcBVEstimate(star, comparables);

                // Estimate target vmag and error (error might include variable signal).
                field.CalcVMagEstimate(star, comparables);

                // Calculate a score to emphasize non-periodic changes in star flux.
                decimal score =
                    field.CalcVariabilityScore(star);

                // Loop through and check if any of the stars flux measurements deviate from the initial measurement by more than the rms.
                //if (star.DifferentialMag.StdDev() > control.StdDev()*1.1m)
                //if(score > 1.5m && star.DifferentialMag.RootMeanSquare() > rms)
                if (true)
                {
                    VSXEntry entry = StarCatalog.GetVSXEntry(star.Name);
                    string variabletype = "";
                    if (entry != null)
                    {
                        //variabletype = String.Format("{0:1}/{1:1} {2:1}:{3:0.00}-{4:0.00}", entry.VFlag.ToString().Trim(), entry.Period,entry.n_min, entry.min, entry.max);
                        variabletype = "\"=HYPERLINK(\"\"http://www.aavso.org/vsx/index.php?view=detail.top&oid="+entry.OID + "\"\",\"\"" + entry.Name.Trim()+ "\"\")\"";

                    }
                    builder.Append(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", 
                        star.Name, 
                        controlstar.Name, 
                        star.CatalogEntry.Rmag, 
                        star.VMag, 
                        star.e_VMag, 
                        star.CatalogEntry.Vmag, 
                        star.CalculatedColorIndex, 
                        star.ColorIndex,
                        star.Saturated == true ? "X" : "",
                        variabletype,
                        error, 
                        score));

                    // Output the calculated differential magnitude with the comparable star.
                    for (int i = 0; i < star.InstrumentalVMag.Count(); i++)
                        builder.Append(String.Format(",{0}", star.DifferentialMag[i] - star.DifferentialMag.Average()));
                    builder.Append(String.Format(",http://simbad.u-strasbg.fr/simbad/sim-coo?Coord={0}%20{1}%20{2}%20{3}%20{4}%20{5}&Radius=2&Radius.unit=arcsec&",
                        star.Name.Substring(0, 2), star.Name.Substring(2, 2), star.Name.Substring(4, 4), star.Name.Substring(8, 3), star.Name.Substring(11, 2), star.Name.Substring(13, 2)));
                    builder.AppendLine();
                }

            }
            File.WriteAllText(".\\candidates_out.csv", builder.ToString());
        }
    }
}
