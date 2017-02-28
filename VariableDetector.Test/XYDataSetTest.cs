using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VariableDetector.Helpers;

namespace VariableDetector.Test
{
    [TestClass]
    public class XYDataSetTest
    {
        [TestMethod]
        public void TestLinearRegression()
        {
            List<double> bvcat = new List<double>();
            List<double> dcat = new List<double>();

            bvcat.Add(-0.069);
            bvcat.Add(0.624);
            bvcat.Add(0.088);
            bvcat.Add(0.430);
            bvcat.Add(1.432);
            bvcat.Add(0.232);

            dcat.Add(-5.94685);
            dcat.Add(-6.0644);
            dcat.Add(-5.98385);
            dcat.Add(-6.0599);
            dcat.Add(-6.16395);
            dcat.Add(-6.00395);

            XYDataSet set = new XYDataSet(bvcat, dcat);
            Assert.AreEqual(Math.Round(set.Slope,4), -0.1402);
            Assert.AreEqual(Math.Round(set.YIntercept,4), -5.9732);
        }
    }
}
