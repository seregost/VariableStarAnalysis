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
        public string PPMX;

        [Layout(15, 1)]
        public string m_PPMX;

        [Layout(17, 10)]
        public int RAmas;

        [Layout(28, 10)]
        public int DEmas;

        [Layout(39, 7)]
        public int pmRA;

        [Layout(47, 7)]
        public int pmDE;

        [Layout(55, 5)]
        public int epRA;

        [Layout(61, 5)]
        public int epDE;

        [Layout(67, 3)]
        public int e_RAmas;

        [Layout(71, 3)]
        public int e_DEmas;

        [Layout(75, 3)]
        public int e_pmRA;

        [Layout(79, 3)]
        public int e_pmDE;

        [Layout(83, 5, 1000)]
        public decimal Cmag;

        [Layout(89, 5, 1000)]
        public decimal Rmag;

        [Layout(95, 5, 1000)]
        public decimal Bmag;

        [Layout(101, 3, 1000)]
        public decimal e_Bmag;

        [Layout(105, 5, 1000)]
        public decimal Vmag;

        [Layout(111, 3, 1000)]
        public decimal e_Vmag;

        [Layout(115, 5, 1000)]
        public decimal Jmag;

        [Layout(121, 3, 1000)]
        public decimal e_Jmag;

        [Layout(125, 5, 1000)]
        public decimal Hmag;

        [Layout(131, 3, 1000)]
        public decimal e_Hmag;

        [Layout(135, 5, 1000)]
        public decimal Kmag;

        [Layout(141, 3, 1000)]
        public decimal e_Kmag;

        [Layout(145, 2)]
        public int Nobs;

        [Layout(148, 1)]
        public char P;

        [Layout(150, 1)]
        public char sub;

        [Layout(152, 1)]
        public char r_ID;

        [Layout(154, 12)]
        public string ID;
        #endregion
    }
}
