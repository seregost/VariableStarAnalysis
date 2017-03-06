using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace VariableDetector.Db
{
    public class StarCatalogContext : DbContext
    {
        public DbSet<D_Frame> Frames { get; set; }
        public DbSet<D_Star> Stars { get; set; }
        public DbSet<F_Sample> Samples { get; set; }
    }

    public class D_Frame
    {
        public int ID { get; set; }
        public string Chart { get; set; }
        public double Time { get; set; }
        public int Duration { get; set; }
        public int ISO { get; set; }
        public string Camera { get; set; }
        public double LocLat { get; set; }
        public double LocLng { get; set; }
        public string File { get; set; }
    }

    public class D_Star
    {
        public int ID { get; set; }
        public string Nameplate { get; set; }
        public int OID { get; set; }
        public double J2000_RA { get; set; }
        public double J2000_DEC { get; set; }
    }

    public class F_Sample
    {
        public int ID { get; set; }
        public int ID_Star { get; set; }
        public int ID_Frame { get; set; }
        public int Aperture { get; set; }
        public double ImgX { get; set; }
        public double ImgY { get; set; }
        public double FluxB { get; set; }
        public double FlagB { get; set; }
        public double FluxV { get; set; }
        public double FlagV { get; set; }
        public double SNR { get; set; }

        [NotMapped]
        public D_Star Star { get; set; }
    }
}
