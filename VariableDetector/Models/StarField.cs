using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Helpers;

namespace VariableDetector.Models
{
    public class StarField
    {
        public List<Star> Stars { get; set; }

        #region Factory
        /// <summary>
        /// Load star-field specified in file
        /// </summary>
        /// <param name="filename">CSV file for star-field to load</param>
        /// <returns></returns>
        public static StarField LoadStarField(string vfilename, string bfilename, int[] excludedframes)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(vfilename);
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";

            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();

            List<Star> stars = new List<Star>();
            do
            {
                Star star = new Star()
                {
                    Name = csv.GetField(0),
                    RA = csv.GetField<decimal>(1),
                    DEC = csv.GetField<decimal>(2),
                    Flags = csv.GetField<int>(3),
                    MinSNR = csv.GetField<decimal>(5),
                    InstrumentalVMag = new List<decimal>(),
                    InstrumentalBMag = new List<decimal>(),
                    DifferentialMag = new List<decimal>()
                };

                // Load flux measurements.
                bool validflux = true;
                int fluxsample = 6;
                int frame = 1;
                string parsedflux;

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
                        star.InstrumentalVMag.Add(flux.Mag());
                    }
                    frame++;
                }

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
                    star.AvgInstrumentalMag = star.InstrumentalVMag.Average();

                    stars.Add(star);
                }
            }
            while (csv.Read());

            csv.Dispose();

            StarField field = new StarField()
            {
                Stars = stars
            };
            field.ComputeMeasuredColorIndex(bfilename, excludedframes);
            return field;
        }

        #endregion

        #region Public Methods

        private void ComputeMeasuredColorIndex(string bfilename, int[] excludedframes)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(bfilename);
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
                            bmags.Add(flux.Mag());
                        }
                        frame++;
                    }

                    if(validflux == true)
                    {
                        star.InstrumentalBMag = bmags;
                        star.ValidColorIndex = true;

                        star.InstrumentalColorIndex = star.InstrumentalBMag.Average() - star.InstrumentalVMag.Average();
                    }
                }
            }
            while (csv.Read());

            csv.Dispose();
        }

        /// <summary>
        /// Perform full photometric reduction of the star field.
        /// </summary>
        public void DoPhotometricReduction()
        {
            foreach (Star star in Stars)
            {
                // Build representative list of comparable stars from the same field.
                List<Star> comparables = GetComparables(star);

                Star controlstar = comparables.Where(x => x.ValidCatalogMag == true).FirstOrDefault();
                if (controlstar == null)
                    controlstar = comparables[0];

                CalcDifferentialMag(star, comparables);

                // Calculate ensemble standard deviation.  This establishes measurement error bounds.
                star.EnsembleError =
                    CalcEnsembleError(star, comparables);

                CalcBVEstimate(star, comparables);

                // Estimate target vmag and error (error might include variable signal).
                CalcVMagEstimate(star, comparables);

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
            for (int i = 0; i < target.InstrumentalVMag.Count(); i++)
                average.Add(comparables.Average(x => x.InstrumentalVMag[i]));

            for (int i = 0; i < target.InstrumentalVMag.Count(); i++)
            {
                decimal dmag = target.InstrumentalVMag[i] - average[i];

                if (i > GlobalVariables.MeridianFlipFrame)
                    dmag = -dmag;

                target.DifferentialMag.Add(dmag);
            }
            return target;
        }

        public decimal CalcEnsembleError(Star target, List<Star> comparables)
        {
            List<PointD> ensembledev = new List<PointD>();
            foreach (Star comparable in comparables)
            {
                List<decimal> controldmag = new List<decimal>();
                for (int i = 0; i < comparable.InstrumentalVMag.Count(); i++)
                {
                    // Calculate control for RMS-
                    decimal controlaverage = comparables.Where(x => x.Name != comparable.Name).Average(x => x.InstrumentalVMag[i]);

                    controldmag.Add(comparable.InstrumentalVMag[i] - controlaverage);
                }
                ensembledev.Add(new PointD(Math.Abs(1.0 / ((double)target.AvgInstrumentalMag - (double)comparable.AvgInstrumentalMag)), (double)controldmag.StdDev()));
            }

            // Calculate weighted deviation - weight is the difference in instrumental mag between the target and comparable.
            return ensembledev.WeightedAverage(z => (decimal)z.Y, f => (decimal)f.X);
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
            List<decimal> tflux = new List<decimal>();
            for (int i = 0; i < target.InstrumentalVMag.Count(); i++)
            {
                List<double> bvcat = new List<double>();
                List<double> dcat = new List<double>();

                foreach (Star otherstar in comparables)
                {
                    bvcat.Add((double)otherstar.ColorIndex);
                    dcat.Add((double)otherstar.CatalogEntry.Vmag - (double)otherstar.InstrumentalVMag[i]);
                }
                XYDataSet set = new XYDataSet(bvcat, dcat);

                decimal calc = target.InstrumentalVMag[i] + (decimal)set.YIntercept;

                // transform coefficient for filter differences if the target star's color index is known.
                // TODO: Fetch instrumental BMag for all stars in frame.
                if(target.ValidColorIndex == true)
                    calc += (decimal)set.Slope * target.CalculatedColorIndex;
                else if (target.ValidCatalogMag == true)
                    calc += (decimal)set.Slope * target.ColorIndex;


                tflux.Add(calc);
            }

            target.VMag = tflux.Average();
            target.e_VMag = tflux.StdDev();

            return target;
        }

        public decimal CalcVariabilityScore(Star target)
        {
            decimal score = target.e_VMag;

            if (target.DifferentialMag.Count() > 1)
            {
                score = 0;
                for (int i = 0; i < target.DifferentialMag.Count() - 1; i++)
                    score += (target.DifferentialMag[i] - target.DifferentialMag[i + 1]) * (target.DifferentialMag[i] - target.DifferentialMag[i + 1]);
                score /= 2 * (target.DifferentialMag.Count() - 1);
                score = (decimal)Math.Sqrt((double)score);

                score = target.DifferentialMag.StdDev() / score;
            }

            return score;
        }
        #endregion
    }
}
