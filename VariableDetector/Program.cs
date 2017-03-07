using CsvHelper;
using LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Db;
using VariableDetector.Models;
using VariableDetector.Helpers;
using nom.tam.fits;
using System.Diagnostics;

namespace VariableDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            if (GlobalVariables.ParseCommands(args) == false)
                return;

            if (GlobalVariables.BuildStarDatabase == true)
            {
                StarCatalog.BuildPPMCatalog(GlobalVariables.PPMCatalogSource);
                StarCatalog.BuildVSXCatalog(GlobalVariables.VSXCatalogSource);
                return;
            }

            using (var db = new StarCatalogContext())
            {
                if(GlobalVariables.LoadFrames == true)
                { 
                    FrameManager.GetFrameInfo(GlobalVariables.InputDirectory, db);
                    return;
                }

                if(GlobalVariables.LoadResults == true)
                {
                    string chartname = 
                        new ChartManager().LoadChart(GlobalVariables.InputDirectory, db);

                    new SampleManager().LoadStarField(GlobalVariables.InputDirectory, chartname, db);

                    return;
                }

                var field = StarField.LoadStarField(GlobalVariables.Chart, db);

                // Apply quality filter.
                field.Stars = field.Stars.OrderByDescending(x => x.AvgInstrumentalMag).Where(x => x.MinSNR > 20).ToList();

                field.DoPhotometricReduction();

                // Output results
                StringBuilder candidates = new StringBuilder();
                candidates.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", "Star", "RMag", "C-VMag", "C-VMag-E", "VMag", "C-ColorIndex", "ColorIndex", "Label", "Known Variable", "Uncertainty", "Score", "Flags", "URL"));

                StringBuilder timeseries = new StringBuilder();
                timeseries.AppendLine(String.Format("{0},{1},{2},{3},{4},{5}", "Score", "Star", "ObsDate", "VMag", "TFAMag", "Uncertainty"));

                foreach (Star star in field.Stars)
                {
                    VSXEntry entry = StarCatalog.GetVSXEntry(star.Name);

                    string variabletype = "";
                    if (entry != null)
                        variabletype = "http://www.aavso.org/vsx/index.php?view=detail.top&oid=" + entry.OID.Trim();

                    candidates.Append(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        star.Name,
                        star.CatalogEntry.Rmag,
                        star.VMag,
                        star.e_VMag,
                        star.CatalogEntry.Vmag,
                        star.CalculatedColorIndex,
                        star.ColorIndex,
                        star.Label,
                        variabletype,
                        star.EnsembleError,
                        star.Score,
                        Convert.ToString(star.Flags, 2).PadLeft(5, '0')));

                    // Output the calculated differential magnitude with the comparable star.
                    int i = 0;
                    foreach (SampleData sample in star.Samples)
                    {
                        timeseries.AppendLine(String.Format("{0},{1},{2},{3},{4},{5}", star.Score, star.Name, field.Frames[i].Time, sample.ApparentVMag - star.VMag, sample.TFAVMag - star.VMag, sample.Uncertainty));
                        i++;
                    }
                    candidates.Append(String.Format(",http://simbad.u-strasbg.fr/simbad/sim-coo?Coord={0}%20{1}%20{2}%20{3}%20{4}%20{5}&Radius=2&Radius.unit=arcsec&",
                        star.Name.Substring(0, 2), star.Name.Substring(2, 2), star.Name.Substring(4, 4), star.Name.Substring(8, 3), star.Name.Substring(11, 2), star.Name.Substring(13, 2)));
                    candidates.AppendLine();
                }

                File.WriteAllText(".\\candidates_out.csv", candidates.ToString());
                File.WriteAllText(".\\timeseries_out.csv", timeseries.ToString());
            }       
        }
    }
}
