using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace VariableDetector.Helpers
{
    public static class GlobalVariables
    {
        public static string PPMCatalogPath
        {
            get
            {
                return ConfigurationManager.AppSettings["PPMCatalogPath"];
            }
        }

        public static string VSXCatalogPath
        {
            get
            {
                return ConfigurationManager.AppSettings["VSXCatalogPath"];
            }
        }

        private static int _comparables = 40;
        public static int Comparables
        {
            get
            {
                return _comparables;
            }
            set
            {
                _comparables = value;
            }
        }

        private static int _meridianflipframe = 99999;
        public static int MeridianFlipFrame
        {
            get
            {
                return _meridianflipframe;
            }
            set
            {
                _meridianflipframe = value;
            }
        }
    }
}
