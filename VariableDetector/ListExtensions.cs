using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector
{
    public static class ListExtensions
    {
        public static decimal RootMeanSquare(this List<decimal> values)
        {
            return values.Count == 0 ? 0 : values.RootMeanSquare(0, values.Count);
        }

        private static decimal RootMeanSquare(this IList<decimal> values, int start, int end)
        {
            decimal s = 0;
            int i;
            for (i = start; i < end; i++)
            {
                s += values[i] * values[i];
            }
            return (decimal)Math.Sqrt((double)(s / (end - start)));
        }

        public static decimal StdDev(this IEnumerable<decimal> values)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in values)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return (decimal)Math.Sqrt(S / (k - 2));
        }

        public static decimal WeightedAverage<T>(this IEnumerable<T> records, Func<T, decimal> value, Func<T, decimal> weight)
        {
            decimal weightedValueSum = records.Sum(x => value(x) * weight(x));
            decimal weightSum = records.Sum(x => weight(x));

            if (weightSum != 0)
                return weightedValueSum / weightSum;
            else
                throw new DivideByZeroException("Weighted sum is zero.");
        }
    }
}
