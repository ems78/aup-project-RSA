using System.Numerics;

namespace AlgoritmiUPrimjeniRSA
{
    public class Maths
    {
        public static Random rand = new(Environment.TickCount);

        /// <summary>
        /// A Rabin Miller primality test which returns true or false.
        /// </summary>
        /// <param name="num">The number to check for being likely prime.</param>
        /// <returns></returns>
        public static bool RabinMillerTest(BigInteger source, int certainty)
        {
            // Filter out basic primes.
            if (source == 2 || source == 3) return true;
            
            // Below 2, and even numbers are not prime.
            if (source < 2 || source % 2 == 0) return false;
            

            // Finding even integer below number.
            BigInteger d = source - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            // Getting a random BigInt using bytes
            Random rng = new Random(Environment.TickCount);
            byte[] bytes = new byte[source.ToByteArray().LongLength];
            BigInteger a;

            // Looping to check random factors.
            for (int i = 0; i < certainty; i++)
            {
                do
                {  // Generating new random bytes to check as a factor 
                    rng.NextBytes(bytes);
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= source - 2);

                BigInteger x = BigInteger.ModPow(a, d, source); // a^d % n
                if (x == 1 || x == source - 1) continue;

                // Iterating to check for prime.
                for (int r = 1; r < s; r++)
                {   
                    x = BigInteger.ModPow(x, 2, source); // x^2 % n

                    if (x == 1) return false;
                    else if (x == source - 1) break;
                }

                if (x != source - 1) return false;
            }

            // All tests have failed to prove that the number is composite.
            return true;
        }


        //An overload wrapper for the RabinMillerTest which accepts a byte array.
        public static bool RabinMillerTest(byte[] bytes, int acc_amt)
        {
            BigInteger b = new BigInteger(bytes);
            return RabinMillerTest(b, acc_amt);
        }


        /// <summary>
        /// Returns the greatest common denominator of both BigIntegers given.
        /// </summary>
        /// <returns>The GCD of A and B.</returns>
        public static BigInteger GCD(BigInteger a, BigInteger b)
        {
            // Looping until the numbers are zero values.
            while (a != 0 && b != 0)
            {
                if (a > b) a %= b;
                else b %= a;
            }

            return a == 0 ? b : a; 
        }

        /// <summary>
        /// Performs a modular inverse on u and v,
        /// such that d = gcd(u,v);
        /// </summary>
        /// <returns>D, such that D = gcd(u,v).</returns>
        public static BigInteger ModularInverse(BigInteger u, BigInteger v)
        {
            BigInteger inverse, u1, u3, v1, v3, t1, t3, q = new();
            BigInteger iteration;

            u1 = 1;
            u3 = u;
            v1 = 0;
            v3 = v;

            // Looping until v3 is zero.
            iteration = 1;
            while (v3 != 0)
            {
                // Divide and sub q, t3 and t1.
                q = u3 / v3;
                t3 = u3 % v3;
                t1 = u1 + q * v1;

                // Swap variables for next iteration.
                u1 = v1; v1 = t1; u3 = v3; v3 = t3;
                iteration = -iteration;
            }

            if (u3 != 1) return 0; // No inverse exists.
            else if (iteration < 0) inverse = v - u1;
            else inverse = u1;

            return inverse;
        }
    }
}