using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Models
{
    /// <summary>
    /// Stores a single sample of a star at a time.
    /// </summary>
    public class SampleData
    {
        // TODO: FOREIGN KEY OF IMAGE
        // TODO: FOREIGN KEY OF STAR
        // TODO: FOREIGN KEY OF CHART

        /// <summary>
        /// Sample time in JD.
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// X location in image of sample
        /// </summary>
        public decimal ImgX { get; set; }

        /// <summary>
        /// Y location in image of sample
        /// </summary>
        public decimal ImgY { get; set; }

        /// <summary>
        /// Measured instrumental V flux
        /// </summary>
        public decimal InstrumentalVFlux { get; set; }

        /// <summary>
        /// Measured instrumental B flux.
        /// </summary>
        public decimal InstrumentalBFlux { get; set; }

        /// <summary>
        /// SNR measurement
        /// </summary>
        public decimal SNR { get; set; }

        public int Flag { get; set; }

        /// <summary>
        /// Size of photometric apeture
        /// </summary>
        public int ApetureSize { get; set; }

        /// <summary>
        /// Instrumental uncertainty derived from SNR.
        /// </summary>
        public decimal Uncertainty { get; set; }

        /// <summary>
        /// Instrumental mag series for the star
        /// </summary>
        public decimal InstrumentalVMag { get; set; }

        /// <summary>
        /// Instrumental mag series for the star using B filter
        /// </summary>
        public decimal InstrumentalBMag { get; set; }
        
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
