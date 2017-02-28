using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Models
{
    class XPMEntry
    {
        public float RA { get; set; }
        public float DEC { get; set; }
        public float MuRAp { get; set; }
        public float MuDEp { get; set; }
        public float MuRAx { get; set; }
        public float MuDEx { get; set; }
        public float Fmag { get; set; }
        public float Jmag { get; set; }
        public float Vmag { get; set; }
        public float Nmag { get; set; }
        public float j_m { get; set; }
        public float er_jmag { get; set; }
        public float h_m { get; set; }
        public float er_hmag { get; set; }
        public float k_m { get; set; }
        public float er_kmag { get; set; }
        public int pts_key { get; set; }
        public int Bmag_ua { get; set; }
        public int Rmag_ua { get; set; }
        public int field { get; set; }
        public float dT { get; set; }
        public bool good_gal { get; set; }
        public bool gal_star { get; set; }
    }
}
