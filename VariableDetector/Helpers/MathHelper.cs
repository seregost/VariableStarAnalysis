using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Helpers
{
    public static class MathHelper
    {
        public static decimal DiffMag(this decimal us, decimal them)
        {
            return -2.5m * (decimal)Math.Log10((double)us) + 2.5m * (decimal)Math.Log10((double)them);
        }

        public static decimal Mag(this decimal us)
        {
            return -2.5m * (decimal)Math.Log10((double)us);
        }
    }
}
