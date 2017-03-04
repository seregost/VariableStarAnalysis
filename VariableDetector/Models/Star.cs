using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Models
{
    public class Star
    {
        /// <summary>
        /// PPMX name for star
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Right Ascencsion coordinate of star
        /// </summary>
        public decimal RA { get; set; }

        /// <summary>
        /// Declination of star
        /// </summary>
        public decimal DEC { get; set; }

        public decimal ImgX { get; set; }
        public decimal ImgY { get; set; }
        /// <summary>
        /// Error flags for star - used for filtering
        /// </summary>
        public int Flags { get; set; }

        /// <summary>
        /// Calculated VMag for the star
        /// </summary>
        public decimal VMag { get; set; }

        /// <summary>
        /// Calculated VMag error for the star
        /// </summary>
        public decimal e_VMag { get; set; }

        /// <summary>
        /// Minimum SNR for the star - used for filtering.
        /// </summary>
        public decimal MinSNR { get; set; }

        /// <summary>
        /// Minimum SNR for the star - used for filtering.
        /// </summary>
        public bool Saturated { get; set; }

        /// <summary>
        /// The average magnitude of the star.
        /// </summary>
        public decimal AvgInstrumentalMag { get; set; }

        /// <summary>
        /// B-V color index of the star
        /// </summary>
        public decimal ColorIndex { get; set; }

        /// <summary>
        /// Instrumental color index based on difference between b and g filters
        /// </summary>
        public decimal InstrumentalColorIndex { get; set; }

        /// <summary>
        /// Calculated color index based off image analysis.
        /// </summary>
        public decimal CalculatedColorIndex { get; set; }

        /// <summary>
        /// Entry from PPMX catalog containing previous measurements.
        /// </summary>
        public PPMEntry CatalogEntry { get; set; }

        public List<Star> Comparables { get; set; }
        /// <summary>
        /// Indicator for having proper VMag and B-V values in the catalog.
        /// </summary>
        public bool ValidCatalogMag { get; set; }

        public bool ValidColorIndex { get; set; }

        public decimal EnsembleError { get; set; }

        public decimal Score { get; set; }

        public List<SampleData> Samples { get; set; }
    }

    public class SampleData
    {
        /// <summary>
        /// Sample time in JD.
        /// </summary>
        public decimal Time { get; set; }

        public decimal InstrumentalFlux { get; set; }

        /// <summary>
        /// Instrumental mag series for the star
        /// </summary>
        public decimal InstrumentalVMag { get; set; }

        /// <summary>
        /// Instrumental mag series for the star using B filter
        /// </summary>
        public decimal InstrumentalBMag { get; set; }

        /// <summary>
        /// Instrumental uncertainty derived from SNR.
        /// </summary>
        public decimal Uncertainty { get; set; }

        /// <summary>
        /// Differential magnitude of star.
        /// </summary>
        public decimal DifferentialVMag { get; set; }

        /// <summary>
        /// Approximate apparent magnitude of the star.
        /// </summary>
        public decimal ApparentVMag { get; set; }

        /// <summary>
        /// Apparent magnitude with TFA applied.
        /// </summary>
        public decimal TFAVMag { get; set; }
    }
}
