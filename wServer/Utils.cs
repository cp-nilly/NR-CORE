using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer
{
    static class EnumerableUtils
    {
        public static T RandomElement<T>(this IEnumerable<T> source,
                                    Random rng)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (rng.Next(count) == 0)
                {
                    current = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Sequence was empty");
            }
            return current;
        }
    }

    static class MathsUtils
    {
        public static double Dist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public static double DistSqr(double x1, double y1, double x2, double y2)
        {
            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }
        public static double DistSqr(int x1, int y1, int x2, int y2)
        {
            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
        }

        // http://stackoverflow.com/questions/1042902/most-elegant-way-to-generate-prime-numbers/1072205#1072205
        public static List<int> GeneratePrimes(int n)
        {
            var limit = ApproximateNthPrime(n);
            var bits = SieveOfEratosthenes(limit);
            var primes = new List<int>(n);
            for (int i = 0, found = 0; i < limit && found < n; i++)
                if (bits[i])
                {
                    primes.Add(i);
                    found++;
                }

            return primes;
        }

        private static int ApproximateNthPrime(int nn)
        {
            double n = (double)nn;
            double p;
            if (nn >= 7022)
                p = n * Math.Log(n) + n * (Math.Log(Math.Log(n)) - 0.9385);
            else if (nn >= 6)
                p = n * Math.Log(n) + n * Math.Log(Math.Log(n));
            else if (nn > 0)
                p = new int[] { 2, 3, 5, 7, 11 }[nn - 1];
            else
                p = 0;

            return (int)p;
        }

        private static BitArray SieveOfEratosthenes(int limit)
        {
            var bits = new BitArray(limit + 1, true);
            bits[0] = false;
            bits[1] = false;
            for (var i = 0; i * i <= limit; i++)
                if (bits[i])
                    for (var j = i * i; j <= limit; j += i)
                        bits[j] = false;

            return bits;
        }
    }
}
