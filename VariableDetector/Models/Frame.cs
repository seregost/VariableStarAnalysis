using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Models
{
    public class Frame
    {
        public int ID { get; set; }

        public string Filename { get; set; }

        public decimal Time { get; set; }

        public int Duration { get; set; }

        public int ISO { get; set; }

        public string Camera { get; set; }

        public decimal LocLat { get; set; }

        public decimal LocLng { get; set; }
    }
}
