using LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Helpers;

namespace VariableDetector.Models
{
    public class PPMEntry
    {
        #region Fields
        [Layout(0, 15)]
        public string PPMX { get; set; }

        [Layout(15, 1)]
        public string m_PPMX { get; set; }

        [Layout(17, 10)]
        public int RAmas { get; set; }

        [Layout(28, 10)]
        public int DEmas { get; set; }

        [Layout(39, 7)]
        public int pmRA { get; set; }

        [Layout(47, 7)]
        public int pmDE { get; set; }

        [Layout(55, 5)]
        public int epRA { get; set; }

        [Layout(61, 5)]
        public int epDE { get; set; }

        [Layout(67, 3)]
        public int e_RAmas { get; set; }

        [Layout(71, 3)]
        public int e_DEmas { get; set; }

        [Layout(75, 3)]
        public int e_pmRA { get; set; }

        [Layout(79, 3)]
        public int e_pmDE { get; set; }

        [Layout(83, 5, 1000)]
        public decimal Cmag { get; set; }

        [Layout(89, 5, 1000)]
        public decimal Rmag { get; set; }

        [Layout(95, 5, 1000)]
        public decimal Bmag { get; set; }

        [Layout(101, 3, 1000)]
        public decimal e_Bmag { get; set; }

        [Layout(105, 5, 1000)]
        public decimal Vmag { get; set; }

        [Layout(111, 3, 1000)]
        public decimal e_Vmag { get; set; }

        [Layout(115, 5, 1000)]
        public decimal Jmag { get; set; }

        [Layout(121, 3, 1000)]
        public decimal e_Jmag { get; set; }

        [Layout(125, 5, 1000)]
        public decimal Hmag { get; set; }

        [Layout(131, 3, 1000)]
        public decimal e_Hmag { get; set; }

        [Layout(135, 5, 1000)]
        public decimal Kmag { get; set; }

        [Layout(141, 3, 1000)]
        public decimal e_Kmag { get; set; }

        [Layout(145, 2)]
        public int Nobs { get; set; }

        [Layout(148, 1)]
        public char P { get; set; }

        [Layout(150, 1)]
        public char sub { get; set; }

        [Layout(152, 1)]
        public char r_ID { get; set; }

        [Layout(154, 12)]
        public string ID { get; set; }
        #endregion
    }
}
