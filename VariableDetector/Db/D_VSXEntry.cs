using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Helpers;

namespace VariableDetector.Db
{
    public class D_VSXEntry
    {
        #region Fields
        public int ID { get; set; }

        public string AUID { get; set; }

        [Layout(0, 7)]
        public string OID { get; set; }

        [Layout(8, 30)]
        public string Name { get; set; }

        [Layout(39, 1)]
        public int VFlag { get; set; }

        [Layout(41, 9)]
        public float RADeg { get; set; }

        [Layout(51, 9)]
        public float DEDeg { get; set; }

        [Layout(61, 30)]
        public string Type { get; set; }

        [Layout(93, 1)]
        public string l_max { get; set; }

        [Layout(94, 6)]
        public float max { get; set; }

        [Layout(100, 1)]
        public string u_max { get; set; }

        [Layout(101, 6)]
        public string n_max { get; set; }

        [Layout(108, 1)]
        public string f_min { get; set; }

        [Layout(109, 1)]
        public string l_min { get; set; }

        [Layout(110, 6)]
        public float min { get; set; }

        [Layout(116, 1)]
        public string u_min { get; set; }

        [Layout(117, 6)]
        public string n_min { get; set; }

        [Layout(124, 12)]
        public float Epoch { get; set; }

        [Layout(136, 1)]
        public string u_Epoch { get; set; }

        [Layout(138, 1)]
        public string l_Period { get; set; }

        [Layout(139, 16)]
        public float Period { get; set; }

        [Layout(156, 3)]
        public string u_Period { get; set; }
        #endregion
    }
}
