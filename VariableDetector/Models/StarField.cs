using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VariableDetector.Helpers;

namespace VariableDetector.Models
{
    public class StarField
    {
        public List<Star> Stars { get; set; }
        public List<string> SourceFiles { get; set; }
        public List<double> ExposureDates { get; set; }
        #region Factory
        /// <summary>
        /// Load star-field specified in file
        /// </summary>
        /// <param name="filename">CSV file for star-field to load</param>
        /// <returns></returns>
        public static StarField LoadStarField(string directory, Dictionary<string, double> filemaps, int[] excludedframes)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(directory + "\\series_g.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";
            csv.ReadHeader();
            // Load headers

            int fluxsample = 6;
            int frame = 1;
            StarField field = new StarField()
            {
                Stars = new List<Star>(),
                ExposureDates = new List<double>(),
                SourceFiles = new List<string>()
            };
            
            while (fluxsample < csv.FieldHeaders.Count())
            {
                string filename = csv.FieldHeaders.GetValue(fluxsample).ToString();
                if (excludedframes.Where(x => x == frame).Count() == 0)
                {
                    KeyValuePair<string, double> match = filemaps.Where(x => filename.Contains(x.Key) == true).First();
                    field.SourceFiles.Add(filename);

                    field.ExposureDates.Add(match.Value);
                }
                fluxsample++;
                frame++;
            }
            
            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();

            do
            {
                Star star = new Star()
                {
                    Name = csv.GetField(0),
                    RA = csv.GetField<decimal>(1),
                    DEC = csv.GetField<decimal>(2),
                    Flags = csv.GetField<int>(3),
                    MinSNR = csv.GetField<decimal>(5),
                    Samples = new List<SampleData>()
                };

                // Load flux measurements.
                bool validflux = true;
                string parsedflux;

                fluxsample = 6;
                frame = 1;

                while (csv.TryGetField(fluxsample++, out parsedflux))
                {
                    if (excludedframes.Where(x => x == frame).Count() == 0)
                    {
                        decimal flux = 0.0m;
                        if (decimal.TryParse(parsedflux, out flux) == false)
                        {
                            validflux = false;
                            break;
                        }

                        SampleData sample = new SampleData()
                        {
                            InstrumentalFlux = flux,
                            InstrumentalVMag = flux.Mag()
                        };

                        // Calculate measurement uncertainty in instrumentalvmag.
                        sample.Uncertainty =
                            Math.Abs((flux + flux / star.MinSNR).Mag() - sample.InstrumentalVMag);

                        // Save flux measurement as instrumental mag.
                        star.Samples.Add(sample);
                    }
                    frame++;
                }

                if(star.Samples.Count > 0)
                    star.EnsembleError = star.Samples.Average(x => x.Uncertainty);

                // Load star catalog
                star.CatalogEntry = StarCatalog.GetPPMEntry(star.Name);

                // Calculate color index.
                if (star.CatalogEntry.Vmag > 0 && star.CatalogEntry.Bmag > 0)
                {
                    star.ValidCatalogMag = true;
                    star.ColorIndex = star.CatalogEntry.Bmag - star.CatalogEntry.Vmag;
                }

                // If a valid set of magnitude measurements were loaded, calculate instrumental mag and add to valid list.
                if (validflux == true)
                {
                    star.AvgInstrumentalMag = star.Samples.Average(x => x.InstrumentalVMag);

                    field.Stars.Add(star);
                }
            }
            while (csv.Read());
            field.ComputeMeasuredColorIndex(directory, excludedframes);
            field.GetImageCoords(directory);
            field.GetSNR(directory);

            csv.Dispose();

            return field;
        }

        #endregion

        #region Helper Methods
        private string MapFile(string directory, string filename)
        {
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
                if (file.Contains(filename))
                    return file;

            return "";
        }

        private void GetSNR(string directory)
        {
            int i = 0;
            foreach (string originalfile in SourceFiles)
            {
                string file = MapFile(directory, originalfile);

                // Load and parse csv.
                TextReader reader = File.OpenText(file);
                var csv = new CsvReader(reader);

                csv.Configuration.Delimiter = ";";

                csv.Read();
                csv.Read();
                csv.Read();
                csv.Read();
                csv.Read();

                do
                {
                    string name = csv.GetField(1);

                    Star star = Stars.Where(x => x.Name == name.Trim()).FirstOrDefault();

                    if (star != null)
                    {
                        star.Samples[i].Uncertainty =
                            Math.Abs((star.Samples[i].InstrumentalFlux + star.Samples[i].InstrumentalFlux / csv.GetField<decimal>(19)).Mag() - star.Samples[i].InstrumentalVMag);
                    }
                }
                while (csv.Read());

                csv.Dispose();

                i++;
            }
        }


        private void GetImageCoords(string directory)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(directory + "\\coords.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";

            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();

            do
            {
                string name = csv.GetField(1);

                Star star = Stars.Where(x => x.Name == name.Trim()).FirstOrDefault();

                if (star != null)
                {
                    star.ImgX = csv.GetField<decimal>(7);
                    star.ImgY = csv.GetField<decimal>(8);
                }
            }
            while (csv.Read());

            csv.Dispose();
        }

        private void ComputeMeasuredColorIndex(string directory, int[] excludedframes)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(directory + "\\series_b.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";

            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();

            do
            {
                string name = csv.GetField(0);

                Star star = Stars.Where(x => x.Name == name).FirstOrDefault();
                int flag = csv.GetField<int>(3);

                // Only try to load valid stars
                if (star != null && (flag == 0 || flag == 16))
                {
                    bool validflux = true;
                    int fluxsample = 6;
                    int frame = 1;
                    string parsedflux;

                    if (flag == 16)
                        star.Saturated = true;

                    List<decimal> bmags = new List<decimal>();
                    while (csv.TryGetField(fluxsample++, out parsedflux))
                    {
                        if (excludedframes.Where(x => x == frame).Count() == 0)
                        {
                            decimal flux = 0.0m;
                            if (decimal.TryParse(parsedflux, out flux) == false)
                            {
                                validflux = false;
                                break;
                            }
                            // Save flux measurement as instrumental mag.
                            star.Samples[frame - 1].InstrumentalBMag = flux.Mag();
                        }
                        frame++;
                    }

                    if (validflux == true)
                    {
                        star.ValidColorIndex = true;

                        star.InstrumentalColorIndex = star.Samples.Average(x => x.InstrumentalBMag) - star.Samples.Average(x => x.InstrumentalVMag);
                    }
                }
            }
            while (csv.Read());

            csv.Dispose();
        }

        public void RunTFA()
        {
            Process process = new Process();
            process.StartInfo.FileName = "tfa_batch.bat";
            process.Start();
            process.WaitForExit();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Perform full photometric reduction of the star field.
        /// </summary>
        public void DoPhotometricReduction()
        {
            foreach (Star star in Stars)
            {
                // Build representative list of comparable stars from the same field.
                star.Comparables = GetComparables(star);

                Star controlstar = star.Comparables.Where(x => x.ValidCatalogMag == true).FirstOrDefault();
                if (controlstar == null)
                    controlstar = star.Comparables[0];

                CalcDifferentialMag(star, star.Comparables);

                // Calculate ensemble standard deviation.  This establishes measurement error bounds.
                star.EnsembleError =
                    CalcEnsembleError(star, star.Comparables);

                CalcBVEstimate(star, star.Comparables);

                // Estimate target vmag and error (error might include variable signal).
                CalcVMagEstimate(star, star.Comparables);

            }

            PerformTFAFilter();
        }

        public void PerformTFAFilter()
        {
            // Output batch file.
            StringBuilder tfa_batch = new StringBuilder();
            tfa_batch.AppendLine("SET PATH=%PATH%;c:\\mingw\\bin;c:\\mingw\\MYSYS\\1.0\\local\\bin;c:\\mingw\\MYSYS\\1.0\\bin");

            StringBuilder tfa_trend = new StringBuilder();
            StringBuilder tfa_input = new StringBuilder();

            List<Star> trendstars = new List<Star>();
            trendstars = Stars.OrderByDescending(x => x.MinSNR).Where(x => x.Flags == 0).Take(10).ToList();

            foreach (Star comparable in trendstars)
                tfa_trend.AppendLine(String.Format("{0} {1:0.00} {2:0.00}", "vin/" + comparable.Name, comparable.ImgX, comparable.ImgY));
            File.WriteAllText(".\\vin\\tfa_trend", tfa_trend.ToString());

            foreach (Star star in Stars)
            {
                // Output comparable star map.

                StringBuilder tfa_curve = new StringBuilder();

                // Output light curve file.
                for (int i = 0; i < star.Samples.Count(); i++)
                    tfa_curve.AppendLine(String.Format("{0:0.00000} {1:0.00000} {2:0.00000}", ExposureDates[i], Math.Round(star.Samples[i].ApparentVMag, 4), star.Samples[i].Uncertainty));
                File.WriteAllText(".\\vin\\" + star.Name, tfa_curve.ToString());

                // Write input file.
                tfa_input.AppendLine(String.Format("{0} {1:0.00} {2:0.00}", "vin/" + star.Name, star.ImgX, star.ImgY));
            }

            File.WriteAllText(".\\vin\\tfa_input", tfa_input.ToString());
            
            // Output date file.
            StringBuilder tfa_dates = new StringBuilder();

            int d = 0;
            foreach (decimal exposuredate in ExposureDates)
                tfa_dates.AppendLine(String.Format("{0}.FITS {1}", d++, exposuredate));
            File.WriteAllText(".\\tfa_dates", tfa_dates.ToString());

            tfa_batch.AppendLine(string.Format("vartools.exe -l vin/tfa_input -oneline -rms -TFA vin/tfa_trend tfa_dates 25.0 1 0 0 -o vout"));
            File.WriteAllText(".\\tfa_batch.bat", tfa_batch.ToString());
            RunTFA();

            // Reload new magnitudes.
            foreach (Star star in Stars)
            {
                TextReader reader = File.OpenText(".\\vout\\" + star.Name);

                for (int i = 0; i < star.Samples.Count(); i++)
                {
                    string line = reader.ReadLine();
                    string[] fields = line.Split(' ');

                    fields = fields.Where(x => x.Trim().Length > 0).ToArray();

                    star.Samples[i].TFAVMag = Convert.ToDecimal(fields[1]);
                }
                // REcompute averages
                star.VMag = star.Samples.Average(x => x.TFAVMag);
                star.e_VMag = star.Samples.Select(x => x.TFAVMag).StdDev();

                // Calculate a score to emphasize non-periodic changes in star flux.
                star.Score =
                    CalcVariabilityScore(star);
            }
        }

        /// <summary>
        /// Load comparables stars for the specified target.  Maximum comparables is constrained by
        /// GlobalVariables.Comparables.
        /// </summary>
        /// <param name="target">The target star being analyzed.</param>
        /// <returns></returns>
        public List<Star> GetComparables(Star target)
        {
            // Build representative field of comparable stars.
            List<Star> comparables =
                Stars
                    .Where(x => x.AvgInstrumentalMag < target.AvgInstrumentalMag && x.ValidCatalogMag == true)
                    .Take(GlobalVariables.Comparables / 2)
                    .ToList();

            List<Star> less =
                Stars
                    .Where(x => x.AvgInstrumentalMag > target.AvgInstrumentalMag && x.ValidCatalogMag == true)
                    .OrderBy(x => x.AvgInstrumentalMag)
                    .Take(GlobalVariables.Comparables - comparables.Count)
                    .ToList();

            if (less.Count + comparables.Count < GlobalVariables.Comparables)
            {
                comparables =
                    Stars
                        .Where(x => x.AvgInstrumentalMag < target.AvgInstrumentalMag && x.ValidCatalogMag == true)
                        .Take(GlobalVariables.Comparables - less.Count)
                        .ToList();
            }

            comparables.AddRange(less);

            return comparables;
        }

        public Star CalcDifferentialMag(Star target, List<Star> comparables)
        {
            List<decimal> average = new List<decimal>();
            for (int i = 0; i < target.Samples.Count; i++)
                average.Add(comparables.Average(x => x.Samples[i].InstrumentalVMag));

            for (int i = 0; i < target.Samples.Average(x => x.InstrumentalVMag); i++)
            {
                decimal dmag = target.Samples.Average(x => x.InstrumentalVMag) - average[i];

                if (i > GlobalVariables.MeridianFlipFrame)
                    dmag = -dmag;

                target.Samples[i].DifferentialVMag = dmag;
            }
            return target;
        }

        public decimal CalcEnsembleError(Star target, List<Star> comparables)
        {
            List<PointD> ensembledev = new List<PointD>();
            foreach (Star comparable in comparables)
            {
                List<decimal> controldmag = new List<decimal>();
                for (int i = 0; i < comparable.Samples.Count; i++)
                {
                    // Calculate control for RMS-
                    decimal controlaverage = comparables.Where(x => x.Name != comparable.Name).Average(x => x.Samples[i].InstrumentalVMag);

                    controldmag.Add(comparable.Samples[i].InstrumentalVMag - controlaverage);
                }
                ensembledev.Add(new PointD(Math.Abs(1.0 / ((double)target.AvgInstrumentalMag - (double)comparable.AvgInstrumentalMag)), (double)controldmag.StdDev()));
            }

            // Calculate weighted deviation - weight is the difference in instrumental mag between the target and comparable.
            return target.EnsembleError;
            //return ensembledev.WeightedAverage(z => (decimal)z.Y, f => (decimal)f.X);
        }

        public Star CalcBVEstimate(Star target, List<Star> comparables)
        {
            if(target.ValidColorIndex == true)
            { 
                List<double> bvcat = new List<double>();
                List<double> bvins = new List<double>();

                foreach (Star otherstar in comparables.Where(x => x.ValidColorIndex == true))
                {
                    bvcat.Add((double)otherstar.ColorIndex);
                    bvins.Add((double)otherstar.InstrumentalColorIndex);
                }
                XYDataSet set = new XYDataSet(bvins, bvcat);

                target.CalculatedColorIndex = (decimal)set.Slope * target.InstrumentalColorIndex + (decimal)set.YIntercept;
            }
            return target;
        }

        public Star CalcVMagEstimate(Star target, List<Star> comparables)
        {
            //////////////////////////////
            // Do magnitude estimate.
            for (int i = 0; i < target.Samples.Count; i++)
            {
                List<double> bvcat = new List<double>();
                List<double> dcat = new List<double>();

                foreach (Star otherstar in comparables)
                {
                    bvcat.Add((double)otherstar.ColorIndex);
                    dcat.Add((double)otherstar.CatalogEntry.Vmag - (double)otherstar.Samples[i].InstrumentalVMag);
                }
                XYDataSet set = new XYDataSet(bvcat, dcat);

                decimal calc = target.Samples[i].InstrumentalVMag + (decimal)set.YIntercept;

                // transform coefficient for filter differences if the target star's color index is known.
                // TODO: Fetch instrumental BMag for all stars in frame.
                if(target.ValidColorIndex == true)
                    calc += (decimal)set.Slope * target.CalculatedColorIndex;
                else if (target.ValidCatalogMag == true)
                    calc += (decimal)set.Slope * target.ColorIndex;


                target.Samples[i].ApparentVMag = calc;
            }

            target.VMag = target.Samples.Average(x => x.ApparentVMag);
            target.e_VMag = target.Samples.Select(x => x.ApparentVMag).StdDev();

            return target;
        }

        public decimal CalcVariabilityScore(Star target)
        {
            decimal score = target.e_VMag;

            if (target.Samples.Count > 1)
            {
                score = 0;
                for (int i = 0; i < target.Samples.Count - 1; i++)
                {
                    decimal vmag0 = target.Samples[i].TFAVMag;
                    decimal vmag1 = target.Samples[i+1].TFAVMag;
                    score += (vmag0 - vmag1) * (vmag0 - vmag1);
                }
                score /= 2 * (target.Samples.Count - 1);
                score = (decimal)Math.Sqrt((double)score);

                score = target.Samples.Select(x => x.TFAVMag).StdDev() / score;
            }

            return score;
        }
        #endregion
    }
}
