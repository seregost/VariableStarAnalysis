using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VariableDetector.Models;
using VariableDetector.Helpers;

namespace VariableDetector.Test
{
    [TestClass]
    public class ListExtensionsTest
    {
        [TestMethod]
        public void TestRootMeanSquare()
        {
            List<decimal> testlist = new List<decimal>();
            testlist.Add(1);
            testlist.Add(2);
            testlist.Add(3);
            testlist.Add(4);

            decimal result = testlist.RootMeanSquare();

            Assert.AreEqual((decimal)Math.Round(result, 5), 2.73861m);
        }

        [TestMethod]
        public void TestStdDev()
        {
            List<decimal> testlist = new List<decimal>();
            testlist.Add(1);
            testlist.Add(2);
            testlist.Add(3);
            testlist.Add(4);

            decimal result = testlist.StdDev();

            Assert.AreEqual((decimal)Math.Round(result, 5), 1.29099m);
        }

        [TestMethod]
        public void TestWeightedAverage()
        {
            List<PointD> testlist = new List<PointD>();
            testlist.Add(new PointD(1.5, 1));
            testlist.Add(new PointD(1, 2));
            testlist.Add(new PointD(1, 3));

            decimal result = testlist.WeightedAverage(x => (decimal)x.Y, y => (decimal)y.X);

            Assert.AreEqual((decimal)Math.Round(result, 5), 1.85714m);

        }

    }
}
