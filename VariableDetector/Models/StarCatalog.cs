﻿using LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using VariableDetector.Helpers;
using VariableDetector.Db;


namespace VariableDetector.Models
{
    public static class StarCatalog
    {
        #region Private
        private static DB _ppmdb = DB.Open(GlobalVariables.PPMCatalogPath, GetOptions());

        private static Options GetOptions()
        {
            Options opts = Options.Default;
            opts.CreateIfMissing = true;
            opts.Compression = CompressionType.SnappyCompression;
            opts.ReuseLogs = true;
            opts.WriteBufferSize = 62914560;
            opts.FilterPolicy = new BloomFilterPolicy(16);

            return opts;
        }

        private static string FindPPMXKey(float RA, float DEC)
        {
            List<string> keys = ConvertToKey(RA, DEC);

            foreach (string key in keys)
            {
                Slice value;
                lock (_ppmdb)
                {
                    ReadOptions options = ReadOptions.Default;
                    options.FillCache = true;
                    options.VerifyChecksums = true;
                    if (_ppmdb.TryGet(options, key, out value))
                        return key;
                }

            }
            return null;
        }

        private static List<string> ConvertToKey(float RA, float DEC)
        {
            int HH = (int)(RA / 15);
            int MM = (int)(((RA / 15) - HH) * 60);
            float SS = ((((RA / 15) - HH) * 60) - MM) * 60;

            string sign = "+";
            if (DEC < 0)
                sign = "-";

            int D = (int)(Math.Abs(DEC));
            int M = (int)((Math.Abs(DEC) - D) * 60);
            int S = (int)Math.Floor((((Math.Abs(DEC) - D) * 60) - M) * 60);

            List<string> possibilities = new List<string>();

            var SS2 = (float)(Math.Floor(SS * 10) / 10.0);
            var SS3 = (float)(Math.Floor((SS - 0.1) * 10) / 10.0);

            possibilities.Add(string.Format("{0:00}{1:00}{2:00.0}{3}{4:00}{5:00}{6:00}", HH, MM, SS, sign, D, M, S));
            possibilities.Add(string.Format("{0:00}{1:00}{2:00.0}{3}{4:00}{5:00}{6:00}", HH, MM, SS2, sign, D, M, S));
            possibilities.Add(string.Format("{0:00}{1:00}{2:00.0}{3}{4:00}{5:00}{6:00}", HH, MM, SS3, sign, D, M, S));

            return possibilities;
        }
        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/system.net.webrequest(v=vs.110).aspx
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetHTML(string url)
        {
            // Create a request for the URL. 		
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        #endregion

        #region Public
        public static string GetAUID(D_VSXEntry entry)
        {
            using (var db = new StarCatalogContext())
            {
                if(entry.AUID == null)
                { 
                    var html = GetHTML("http://www.aavso.org/vsx/index.php?view=detail.top&oid=" + entry.OID.Trim());

                    string pattern = @"\d+-\D+-\d+";
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(html);

                    if (matches.Count > 0)
                    {
                        db.VSXEntries.Attach(entry);
                        entry.AUID = matches[0].Value;
                    }
                    db.SaveChanges();
                }
                return entry.AUID;
            }
        }

   
        public static void BuildPPMCatalog(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                StreamReader stream = File.OpenText(file);

                while (!stream.EndOfStream)
                {
                    string entry = stream.ReadLine();
                    _ppmdb.Put(WriteOptions.Default, entry.Substring(0, 15), entry);
                }
                Console.WriteLine("Completed loading catalog: " + file);
            }
        }

        public static void BuildVSXCatalog(string directory)
        {
            string[] files = Directory.GetFiles(directory);

            int i = 0;
            var db = new StarCatalogContext();
            foreach (string file in files)
            {
                StreamReader stream = File.OpenText(file);
                //int found = 0;
                //int notfound = 0;
                    
                while (!stream.EndOfStream)
                {
                    string entry = stream.ReadLine();

                    D_VSXEntry md = new D_VSXEntry();

                    FixedLengthReader flr = new FixedLengthReader(entry);
                    flr.read(md);

                    db.VSXEntries.Add(md);
                    //string key = FindPPMXKey(md.RADeg, md.DEDeg);

                    //if (key != null)
                    //{
                    //    found++;
                    //    _ppmdb.Put(WriteOptions.Default, key+".vsxinfo", entry);
                    //}
                    //else
                    //{ 
                    //    notfound++;

                    //    // Just use the closes match.
                    //    key = ConvertToKey(md.RADeg, md.DEDeg)[1];

                    //    _ppmdb.Put(WriteOptions.Default, key + ".vsxinfo", entry);
                    //}
                    //if ((found+notfound)%100 == 0)
                    //    Console.Write(String.Format("\rFound: {0} Not Found: {1}", found, notfound));
                    if (i++ % 1000 == 0)
                    { 
                        db.SaveChanges();
                        db.Dispose();
                        db = new StarCatalogContext();
                    }
                }
                Console.WriteLine();
                Console.WriteLine("Completed loading catalog: " + file);
            }
        }

        public static PPMEntry GetPPMEntry(string ppmx)
        {
            string encodedstar = _ppmdb.Get(ReadOptions.Default, ppmx.Substring(0, 15)).ToString();

            PPMEntry md = new PPMEntry();

            FixedLengthReader flr = new FixedLengthReader(encodedstar);
            flr.read(md);

            return md;
        }

        public static D_VSXEntry GetVSXEntry(double ra, double dec)
        {
            Slice encodedstar;

            using (var db = new StarCatalogContext())
            {
                var result = db.VSXEntries.Where(x => Math.Abs((double)x.RADeg - (float)ra) < 0.001 && Math.Abs((double)x.DEDeg - (float)dec) < 0.001).FirstOrDefault();

                return result;
                //if (_ppmdb.TryGet(ReadOptions.Default, ppmx.Substring(0, 15) + ".vsxinfo", out encodedstar))
                //{
                //    D_VSXEntry md = new D_VSXEntry();

                //    FixedLengthReader flr = new FixedLengthReader(encodedstar.ToString());
                //    flr.read(md);

                //    return md;
                //}
                //else
                //    return null;
            }
        }

        public static void Dispose()
        {
            _ppmdb.Dispose();
        }
        #endregion
    }
}
