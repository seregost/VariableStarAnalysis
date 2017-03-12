using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using VariableDetector.Db;
using nom.tam.fits;

namespace VariableDetector.Helpers
{
    public class FrameManager
    {
        public static void GetFrameInfo(string directory, StarCatalogContext db)
        { 
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                string key = Path.GetFileName(file).Replace(".fit", "");

                if (db.Frames.Where(x => x.File == key).Count() == 0)
                {
                    D_Frame frame = new D_Frame() { File = key};
                    db.Frames.Add(frame);

                    Fits f = new Fits(file);
                    BasicHDU[] hdus = f.Read();

                    DateTime date = File.GetCreationTime(file);
                    double jd = Math.Round(DateTimeToJulian(date), 5);

                    for (int i = 0; i < hdus.Length; i += 1)
                    {
                        Header hdr = hdus[i].Header;

                        date = DateTime.Parse(hdr.GetStringValue("DATE-OBS")+"Z");
                        jd = Math.Round(DateTimeToJulian(date), 5);
                        frame.Time = jd;
                        frame.Duration = (int)hdr.GetDoubleValue("EXPOSURE");
                        frame.ISO = (int)hdr.GetDoubleValue("GAIN");
                        frame.LocLat = GlobalVariables.LocLat;
                        frame.LocLng = GlobalVariables.LocLng;
                        frame.Camera = hdr.GetStringValue("INSTRUME");
                        frame.Reported = 0;
                    }
                }
            }

            db.SaveChanges();
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
