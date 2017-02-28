using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VariableDetector.Models;

namespace VariableDetector.Test
{
    [TestClass]
    public class StarCatalogTest
    {
        [TestMethod]
        public void TestStarRetrieval()
        {
            PPMEntry entry = StarCatalog.GetPPMEntry("053543.0-052013");

            StarCatalog.Dispose();

            Assert.AreEqual(entry.PPMX, "053543.0-052013");
            Assert.AreEqual(entry.m_PPMX, " ");
            Assert.AreEqual(entry.RAmas, 302146396);
            Assert.AreEqual(entry.DEmas, -19213839);
            Assert.AreEqual(entry.pmRA, 17108);
            Assert.AreEqual(entry.pmDE, -725);
            Assert.AreEqual(entry.epRA, -22);
            Assert.AreEqual(entry.epDE, -70);
            Assert.AreEqual(entry.e_RAmas, 18);
            Assert.AreEqual(entry.e_DEmas, 19);
            Assert.AreEqual(entry.e_pmRA, 20);
            Assert.AreEqual(entry.e_pmDE, 20);
            Assert.AreEqual(entry.Cmag, 11.010m);
            Assert.AreEqual(entry.Rmag, 10.895m);
            Assert.AreEqual(entry.Bmag, -9.999m);
            Assert.AreEqual(entry.e_Bmag, -0.099m);
            Assert.AreEqual(entry.Vmag, -9.999m);
            Assert.AreEqual(entry.e_Vmag, -0.099m);
            Assert.AreEqual(entry.Jmag, 9.383m);
            Assert.AreEqual(entry.e_Jmag, 0.022m);
            Assert.AreEqual(entry.Hmag, 8.863m);
            Assert.AreEqual(entry.e_Hmag, 0.030m);
            Assert.AreEqual(entry.Kmag, 8.756m);
            Assert.AreEqual(entry.e_Kmag, 0.021m);
            Assert.AreEqual(entry.Nobs, 5);
            Assert.AreEqual(entry.P, 32);
            Assert.AreEqual(entry.sub, 'S');
            Assert.AreEqual(entry.r_ID, 'S');
            Assert.AreEqual(entry.ID, " 0477400863 ");
        }
    }
}
