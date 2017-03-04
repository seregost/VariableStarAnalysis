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

        public static string InputDirectory = "";

        public static bool BuildStarDatabase = false;

        public static string PPMCatalogSource
        {
            get
            {
                return ConfigurationManager.AppSettings["PPMCatalogSource"];
            }
        }

        public static string VSXCatalogSource
        {
            get
            {
                return ConfigurationManager.AppSettings["VSXCatalogSource"];
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

        public static bool ParseCommands(string[] args)
        {
            for(int i=0; i< args.Count(); i++)
            {
                switch(args[i])
                {
                    case "-bdb":    // Rebuild star database.
                        BuildStarDatabase = true;
                        break;
                    case "-help":    // Rebuild star database.
                        Console.WriteLine("TODO: Add help stuff.");
                        return false;
                    case "-sf":
                        if (args.Count() > i + 1)
                        {
                            InputDirectory = args[++i];
                        }
                        else
                            Console.WriteLine("Please specify input B & V star field files.");
                        break;
                    default:
                        break;
                }
            }
            return true;
        }
    }
}
