using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableDetector.Helpers;

namespace VariableDetector.Models
{
    public class VSXEntry
    {
        #region Fields
        [Layout(0, 7)]
        public string OID;

        [Layout(8, 30)]
        public string Name;

        [Layout(39, 1)]
        public int VFlag;

        [Layout(41, 9)]
        public float RADeg;

        [Layout(51, 9)]
        public float DEDeg;

        [Layout(61, 30)]
        public string Type;

        [Layout(93, 1)]
        public string l_max;

        [Layout(94, 6)]
        public float max;

        [Layout(100, 1)]
        public string u_max;

        [Layout(101, 6)]
        public string n_max;

        [Layout(108, 1)]
        public string f_min;

        [Layout(109, 1)]
        public string l_min;

        [Layout(110, 6)]
        public float min;

        [Layout(116, 1)]
        public string u_min;

        [Layout(117, 6)]
        public string n_min;

        [Layout(124, 12)]
        public float Epoch;

        [Layout(136, 1)]
        public string u_Epoch;

        [Layout(138, 1)]
        public string l_Period;

        [Layout(139, 16)]
        public float Period;

        [Layout(156, 3)]
        public string u_Period;
        #endregion
    }
}
