using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VariableDetector.Helpers
{
    public class JulianDateConverter
    {
        public static Dictionary<string, double> GetFileJD(string directory)
        { 
            string[] files = Directory.GetFiles(directory);
            Dictionary<string, double> dates = new Dictionary<string, double>();

            double min = 0;

            StringBuilder builder = new StringBuilder();

            foreach (string file in files)
            {
                DateTime date = File.GetCreationTime(file);
                double jd = Math.Round(DateTimeToJulian(date),5);

                if (jd < min || min == 0)
                    min = jd;

                string key = Path.GetFileName(file).Replace(".fit", "");

                dates.Add(key,jd);
                builder.AppendLine(String.Format("{0,-50}{1:0.00000}", key, jd));
            }

            File.WriteAllText(directory + "\\times.txt", builder.ToString());

            return dates;
        }

        public static double DateTimeToJulian(DateTime dateTime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = dateTime.ToUniversalTime() - origin;
            double unixTime = Math.Floor(diff.TotalSeconds);
            double julianDate = (unixTime / 86400) + 2440587.5;

            return julianDate;
        }
    }
}
