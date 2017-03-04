using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VariableDetector.Models;
using System.Linq;

namespace VariableDetector.Test
{
    [TestClass]
    public class StarFieldTest
    {
        [TestMethod]
        public void TestLoadStarField()
        {
            StarField field = StarField.LoadStarField(".\\TestSession.csv", new System.Collections.Generic.Dictionary<string, double>(), new int[] { });

            var comparables = field.GetComparables(field.Stars.First());
            Assert.AreEqual(comparables.Count(), 7);
            //StarCatalog.Dispose();
        }

        [TestMethod]
        public void TestCalcVMagEstimate()
        {
            StarField field = StarField.LoadStarField(".\\TestSession.csv", new System.Collections.Generic.Dictionary<string, double>(), new int[] { });

            var targetstar = field.Stars.First();
            var comparables = field.GetComparables(targetstar);

            // Calc based on comparables sans check star
            targetstar = field.CalcVMagEstimate(targetstar, comparables.Where(x => x.Name != "053446.9-053414").ToList());

            Assert.AreEqual(Math.Round(targetstar.VMag,5), 8.125m);
            Assert.AreEqual(Math.Round(targetstar.e_VMag,5), 0.01689m);
            //StarCatalog.Dispose();
        }
    }
}
