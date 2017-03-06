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

        public static double LocLat
        {
            get
            {
                return Convert.ToDouble(ConfigurationManager.AppSettings["LocLat"]);
            }
        }

        public static double LocLng
        {
            get
            {
                return Convert.ToDouble(ConfigurationManager.AppSettings["LocLng"]);
            }
        }

        public static string InputDirectory = "";
        public static string Chart = "";

        public static bool BuildStarDatabase = false;
        public static bool LoadFrames = false;
        public static bool LoadResults = false;

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

        private static int _comparables = 10;
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
                    case "-help":
                        Console.WriteLine("TODO: Add help stuff.");
                        return false;

                    case "-builddb":    // Rebuild star database.
                        BuildStarDatabase = true;
                        break;

                    case "-analyze":
                        if (args.Count() > i + 1)
                        {
                            Chart = args[++i];
                        }
                        else
                            Console.WriteLine("Please specify input chart for analysis.");
                        break;

                    case "-lframes":
                        if (args.Count() > i + 1)
                        { 
                            LoadFrames = true;
                            InputDirectory = args[++i];
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Please specify directory for loading frames.");
                            return false;
                        }
                        
                    case "-lresults":
                        if (args.Count() > i + 1)
                        {
                            LoadResults = true;
                            InputDirectory = args[++i];
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Please specify directory containing results.");
                            return false;
                        }
                    default:
                        break;
                }
            }
            return true;
        }
    }
}
