using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VariableDetector.Helpers;
using VariableDetector.Db;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace VariableDetector.Models
{
    public class StarField
    {
        public List<Star> Stars { get; set; }
        public List<D_Frame> Frames { get; set; }
        public List<D_ChartEntry> ChartEntries { get; set; }
        public List<Star> ChartComparables { get; set; }

        #region Factory
        /// <summary>
        /// Load star-field specified in file
        /// </summary>
        /// <param name="filename">CSV file for star-field to load</param>
        /// <returns></returns>
        public static StarField LoadStarField(string chart, StarCatalogContext db)
        {
            StarField field = new StarField()
            {
                Stars = new List<Star>(),
                Frames = db.Frames.Where(x => x.Chart == chart).OrderBy(x => x.Time).ToList(),
                ChartComparables = new List<Star>()
            };

            Console.WriteLine("Loading chart entries.");
            field.ChartEntries = db.ChartEntries.Where(x => x.Chart == chart).ToList();

            Console.WriteLine("Loading frames.");
            int framenumber = 1;
            foreach (D_Frame frame in field.Frames)
            {
                // Get all samples for frame.
                List<F_Sample> samples = db.Samples.Where(x => x.ID_Frame == frame.ID).ToList();

                foreach(F_Sample sample in samples)
                {
                    var star = field.Stars.Where(x => x.ID == sample.ID_Star).FirstOrDefault();
                    if (star == null)
                    {
                        var d_star = db.Stars.Where(x => x.ID == sample.ID_Star).First();

                        if (d_star != null)
                        {
                            star = new Star()
                            {
                                ID = d_star.ID,
                                Name = d_star.Nameplate,
                                RA = (decimal)d_star.J2000_RA,
                                DEC = (decimal)d_star.J2000_DEC,
                                OID = d_star.OID,
                                ValidColorIndex = true,
                                Samples = new List<SampleData>()
                            };
                            field.Stars.Add(star);
                        }
                        else
                            continue;  // skip star.
                    }

                    var data = new SampleData()
                    {
                        ImgX = (decimal)sample.ImgX,
                        ImgY = (decimal)sample.ImgY,
                        ApetureSize = sample.Aperture,
                        InstrumentalVFlux = (decimal)sample.FluxV,
                        InstrumentalBFlux = (decimal)sample.FluxB,
                        SNR = (decimal)sample.SNR,
                        Flag = sample.FlagV
                    };

                    if (sample.SNR > 0)
                        data.Uncertainty = (decimal)Math.Abs((sample.FluxV + sample.FluxB / sample.SNR).Mag() - sample.FluxV.Mag());
                    else
                        data.Uncertainty = -1;


                    // Calculated fields
                    if (data.InstrumentalBFlux <= 0 || data.InstrumentalVFlux <= 0)
                        star.ValidColorIndex = false;
                    else
                    { 
                        data.InstrumentalBMag = data.InstrumentalBFlux.Mag();
                        data.InstrumentalVMag = data.InstrumentalVFlux.Mag();
                    }
                    
                    star.Flags |= sample.FlagB;
                    star.Samples.Add(data);
                }

                // Add place-holder samples for stars not containing the right sample count.
                foreach(Star star in field.Stars)
                {
                    if(star.Samples.Count < framenumber)
                        star.Samples.Add(new SampleData());
                }
                framenumber++;
            }

            foreach (Star star in field.Stars)
            {
                if (star.Samples.Count > 0)
                    star.EnsembleError = star.Samples.Average(x => x.Uncertainty);

                star.MinSNR = star.Samples.Min(x => x.SNR);

                // Load star catalog
                star.CatalogEntry = StarCatalog.GetPPMEntry(star.Name);

                star.Flags = Math.Max(star.Flags, star.Samples.Max(x => x.Flag));

                // Calculate color index.
                if (star.CatalogEntry.Vmag > 0 && star.CatalogEntry.Bmag > 0)
                {
                    star.ValidCatalogMag = true;
                    star.ColorIndex = star.CatalogEntry.Bmag - star.CatalogEntry.Vmag;
                }

                if(star.ValidColorIndex == true)
                {
                    star.AvgInstrumentalMag = star.Samples.Average(x => x.InstrumentalVMag);
                    star.InstrumentalColorIndex = star.Samples.Average(x => x.InstrumentalBMag) - star.Samples.Average(x => x.InstrumentalVMag);
                }                
            }
            var eligible = field.Stars.Where(x => x.Flags == 0 && x.ValidColorIndex == true).ToList();
            foreach (D_ChartEntry entry in field.ChartEntries)
            {
                double ra = entry.J2000_RA;
                double dec = entry.J2000_DEC;
                var star = eligible.Where(x => Math.Abs((double)x.RA - ra) < 0.001 && Math.Abs((double)x.DEC - dec) < 0.001).FirstOrDefault();

                if(star != null)
                {
                    if(star.MinSNR > 0 && star.Flags == 0 && star.OID == -1)
                    { 
                        star.ColorIndex = (decimal)entry.BVColor;
                        star.CatalogEntry.Vmag = (decimal)entry.VMag;
                        star.Label = entry.Label;
                        field.ChartComparables.Add(star);
                    }
                }
            }

            if (field.ChartComparables.Count > 0)
                field.ChartComparables[0].Label = "[CHECK] " + field.ChartComparables[0].Label;

            if (field.ChartComparables.Count < GlobalVariables.Comparables)
            {
                Console.WriteLine("Adding extra stars due to low chart count.");

                var eligiblestars = field.Stars
                    .Where(x => x.MinSNR > GlobalVariables.MinSNR)
                    .Where(x => field.ChartComparables.Contains(x) == false)
                    .Where(x => x.Flags == 0 && x.OID == -1 && x.ValidCatalogMag == true && x.ValidColorIndex == true)
                    .ToList();

                var builder = new ChartBuilder(field, eligiblestars, field.ChartComparables.ToList());
                
                field.ChartComparables = builder.Solve();

                foreach (Star star in field.ChartComparables)
                {
                    if (star.Label == null)
                        star.Label = star.CatalogEntry.PPMX.Trim();
                }
            }

            if (field.ChartComparables.Count >= GlobalVariables.Comparables)
            {
                Console.WriteLine("Finding best comparable fit.");

                var eligiblestars = field.ChartComparables.Skip(1).ToList();

                var builder = new ChartBuilder(field, eligiblestars, new List<Star>() { field.ChartComparables[0] });

                field.ChartComparables = builder.Solve();
            }

            return field;
        }

        #endregion

        #region Helper Methods
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
            Console.WriteLine("Performing photometric reduction.");
            foreach (Star star in Stars)
            {
                // Build representative list of comparable stars from the same field.
                star.Comparables = GetComparables(star);

                // Can't calculate photometry if there aren't enough comparables!
                if (star.Comparables.Count < 2)
                    continue;

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

            // Perform optional TFA filter - only useful if there are more than one samples in the time series.
            if(Stars[0].Samples.Count > 10)
                PerformTFAFilter();

            foreach (Star star in Stars)
            {
                star.Score =
                    CalcVariabilityScore(star);
            }
        }

        public void PerformTFAFilter()
        {
            // Output batch file.
            StringBuilder tfa_batch = new StringBuilder();
            tfa_batch.AppendLine("SET PATH=%PATH%;c:\\mingw\\bin;c:\\mingw\\MYSYS\\1.0\\local\\bin;c:\\mingw\\MYSYS\\1.0\\bin");

            StringBuilder tfa_trend = new StringBuilder();
            StringBuilder tfa_input = new StringBuilder();

            List<Star> trendstars = new List<Star>();
            trendstars = ChartComparables.OrderByDescending(x => x.MinSNR).Where(x => x.Flags == 0).Take(6).ToList();

            foreach (Star comparable in trendstars)
                tfa_trend.AppendLine(String.Format("{0} {1:0.00} {2:0.00}", "vin/" + comparable.Name, comparable.ImgX, comparable.ImgY));
            File.WriteAllText(".\\vin\\tfa_trend", tfa_trend.ToString());

            foreach (Star star in Stars)
            {
                // Output comparable star map.

                StringBuilder tfa_curve = new StringBuilder();

                // Output light curve file.
                for (int i = 0; i < star.Samples.Count(); i++)
                    tfa_curve.AppendLine(String.Format("{0:0.00000} {1:0.00000} {2:0.00000}", Frames[i].Time, Math.Round(star.Samples[i].ApparentVMag, 4), star.Samples[i].Uncertainty));
                File.WriteAllText(".\\vin\\" + star.Name, tfa_curve.ToString());

                // Write input file.
                tfa_input.AppendLine(String.Format("{0} {1:0.00} {2:0.00}", "vin/" + star.Name, star.ImgX, star.ImgY));
            }

            File.WriteAllText(".\\vin\\tfa_input", tfa_input.ToString());
            
            // Output date file.
            StringBuilder tfa_dates = new StringBuilder();

            int d = 0;
            foreach (D_Frame frame in Frames)
                tfa_dates.AppendLine(String.Format("{0}.FITS {1}", d++, frame.Time));
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
            List<Star> comparables = ChartComparables.Skip(1)
                .Where(x => x.ValidColorIndex == true && x.Name != target.Name).ToList();

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
                if (target.ValidColorIndex == true)
                    calc += (decimal)set.Slope * target.CalculatedColorIndex;
                else if (target.ValidCatalogMag == true)
                    calc += (decimal)set.Slope * target.ColorIndex;

                
                target.Samples[i].ApparentVMag = calc;
                target.Samples[i].TFAVMag = calc;
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

                if(score != 0)
                    score = target.Samples.Select(x => x.TFAVMag).StdDev() / score;
            }

            return score;
        }
        #endregion
    }
}
