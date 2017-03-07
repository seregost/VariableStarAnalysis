using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using VariableDetector.Models;
using VariableDetector.Db;
using System.Text.RegularExpressions;

namespace VariableDetector.Helpers
{
    /// <summary>
    /// Load samples from pixinsight output CSV and place in star catalog.
    /// </summary>
    public class SampleManager
    {
        private List<D_Frame> frames = new List<D_Frame>();
        private List<D_Star> stars = new List<D_Star>();
        private List<F_Sample> samples = new List<F_Sample>();

        /// <summary>
        /// Load star-field specified in file
        /// </summary>
        /// <param name="filename">CSV file for star-field to load</param>
        /// <returns></returns>
        public void LoadStarField(string directory, string chartname, StarCatalogContext db)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(directory + "\\series_g.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";
            csv.ReadHeader();
            
            // Load headers
            int fluxsample = 6;
            while (fluxsample < csv.FieldHeaders.Count())
            {
                string filename = csv.FieldHeaders.GetValue(fluxsample++).ToString();
                var match = db.Frames.Where(x => filename.Contains(x.File)).First();
                match.Chart = chartname;
                frames.Add(match);
            }
            db.SaveChanges();

            csv.Read();
            csv.Read();
            csv.Read();

            int aperture = GetAperture(csv.GetField(6));

            csv.Read();

            // Pre-load all existing sample data for frames.
            List<F_Sample> existingsamples = new List<F_Sample>();
            foreach (D_Frame d_frame in frames)
                existingsamples.AddRange(db.Samples.Where(x => x.ID_Frame == d_frame.ID).ToList());

            do
            {
                // Create star if it doesn't exist.
                string name = csv.GetField(0);

                D_Star star = db.Stars.Where(x => x.Nameplate == name).FirstOrDefault();
                if(star == null)
                {
                    star = new D_Star()
                    {
                        Nameplate = name,
                        J2000_RA = csv.GetField<double>(1),
                        J2000_DEC = csv.GetField<double>(2)
                    };
                    VSXEntry entry = StarCatalog.GetVSXEntry(name);
                    if (entry != null)
                        star.OID = Convert.ToInt32(entry.OID);
                    else
                        star.OID = -1;

                    db.Stars.Add(star);
                }
                stars.Add(star);

                /////
                // Load flux samples.
                string parsedflux;
                fluxsample = 6;

                int frame = 0;
                int flag = csv.GetField<int>(3);
                while (csv.TryGetField(fluxsample++, out parsedflux))
                {
                    if(existingsamples.Where(x => x.ID_Frame == frames[frame].ID && x.ID_Star == star.ID).Count() == 0)
                    { 
                        double flux = 0.0;
                        double.TryParse(parsedflux, out flux);

                        F_Sample sample = new F_Sample()
                        {
                            Star = star,
                            ID_Frame = frames[frame].ID,
                            Aperture = aperture,
                            FluxV = flux
                        };

                        // Save flux measurement as instrumental mag.
                        samples.Add(sample);

                        frame++;
                    }
                }
            }
            while (csv.Read());

            // Save stars
            db.SaveChanges();
            
            // Supplement and persist sample data.
            if (samples.Count > 0)
            {
                foreach (F_Sample sample in samples)
                {
                    // Set star ID.
                    sample.ID_Star = sample.Star.ID;
                    db.Samples.Add(sample);
                }

                LoadInstrumentalColorIndex(directory);
                LoadSNR(directory);
                db.SaveChanges();
            }

            csv.Dispose();
        }

        private int GetAperture(string sample)
        {
            string pattern = @"\d+";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(sample);

            if (matches.Count > 0)
                return Convert.ToInt32(matches[0].Value);

            return -1;
        }
        private void LoadInstrumentalColorIndex(string directory)
        {
            // Load and parse csv.
            TextReader reader = File.OpenText(directory + "\\series_b.csv");
            var csv = new CsvReader(reader);

            csv.Configuration.Delimiter = ";";

            csv.Read();
            csv.Read();
            csv.Read();
            csv.Read();

            do
            {
                string name = csv.GetField(0);

                D_Star star = stars.Where(x => x.Nameplate == name).FirstOrDefault();
                int flag = csv.GetField<int>(3);

                // Only try to load valid stars
                if (star != null)
                {
                    int fluxsample = 6;
                    int frame = 0;
                    string parsedflux;
                    List<decimal> bmags = new List<decimal>();
                    while (csv.TryGetField(fluxsample++, out parsedflux))
                    {
                        double flux = 0.0;
                        double.TryParse(parsedflux, out flux);

                        var sample = samples.Where(x => x.ID_Frame == frames[frame].ID && x.ID_Star == star.ID).First();
                        sample.FluxB = flux;
                        sample.FlagB = flag;

                        // TODO: Store BFlag
                        frame++;
                    }
                }
            }
            while (csv.Read());

            csv.Dispose();
        }

        private string MapFile(string directory, string filename)
        {
            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
                if (file.Contains(filename))
                    return file;

            return "";
        }


        private void LoadSNR(string directory)
        {
            int i = 0;
            foreach (D_Frame frame in frames)
            {
                string file = MapFile(directory, frame.File);

                // Load and parse csv.
                TextReader reader = File.OpenText(file);
                var csv = new CsvReader(reader);

                csv.Configuration.Delimiter = ";";

                csv.Read();
                csv.Read();
                csv.Read();
                csv.Read();
                csv.Read();

                do
                {
                    string name = csv.GetField(1);

                    D_Star star = stars.Where(x => x.Nameplate == name.Trim()).FirstOrDefault();

                    if (star != null)
                    {
                        F_Sample sample = samples.Where(x => x.ID_Frame == frame.ID && x.ID_Star == star.ID).FirstOrDefault();

                        if(sample != null)
                        {
                            sample.ImgX = csv.GetField<double>(7);
                            sample.ImgY = csv.GetField<double>(8);
                            sample.SNR = csv.GetField<double>(19);

                            // HACK because sometimes pixinsight sends out a non integer flag?
                            int flag = 999;
                            int.TryParse(csv.GetField<string>(20), out flag);
                        }
                    }
                }
                while (csv.Read());

                csv.Dispose();

                i++;
            }
        }
    }
}
