using System.Numerics;

namespace AlgoritmiUPrimjeniRSA
{
    public class Maths
    {
        public static Random rand = new(Environment.TickCount);

        /// <summary>
        /// A Rabin Miller primality test which returns true or false.
        /// Rabin-Miller test primarnosti koji vraća true ili false.
        /// </summary>
        /// <param name="num">Broj koji se provjerava je li moguće primaran</param>
        /// <returns>boolean</returns>
        public static bool RabinMillerTest(BigInteger source, int certainty)
        {
            if (source == 2 || source == 3) return true;
            if (source < 2 || source % 2 == 0) return false;

            // Dohvatiti paran broj ispod zadanog broja.
            BigInteger d = source - 1;
            int s = 0;

            while (d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }

            // Dohvatiti slučajni BigInt koristeći byteove
            Random rng = new Random(Environment.TickCount);
            byte[] bytes = new byte[source.ToByteArray().LongLength];
            BigInteger a;

            // Petljom provjeriri slučajne faktore.
            for (int i = 0; i < certainty; i++)
            {
                do
                {  // Generirati nove slučajne byteove za provjeru faktora
                    rng.NextBytes(bytes);
                    a = new BigInteger(bytes);
                } while (a < 2 || a >= source - 2);

                BigInteger x = BigInteger.ModPow(a, d, source); // a^d % n
                if (x == 1 || x == source - 1) continue;

                // Iterirati za provjeru primarnosti, tako da se provjeri je li x^2 % n = 1 ili x^2 % n = n-1
                for (int r = 1; r < s; r++)
                {   
                    x = BigInteger.ModPow(x, 2, source); // x^2 % n

                    if (x == 1) return false;
                    else if (x == source - 1) break;
                }

                if (x != source - 1) return false; 
            }

            // Svi testovi su propali u dokazivanju da je broj složen.
            return true;
        }


        // Preopterećena metoda za RabinMillerTest koja prihvaća byte array.
        public static bool RabinMillerTest(byte[] bytes, int acc_amt)
        {
            BigInteger b = new BigInteger(bytes);
            return RabinMillerTest(b, acc_amt);
        }


        /// <summary>
        /// Izvodi modularni inverz na u i v,
        /// tako da je d = NZD(u,v);
        /// </summary>
        /// <returns>D, takav da je D = NZD(u, v)</returns>
        public static BigInteger ModularInverse(BigInteger u, BigInteger v)
        {
            BigInteger inverse, u1, u3, v1, v3, t1, t3, q = new();
            BigInteger iteration;

            u1 = 1;
            u3 = u;
            v1 = 0;
            v3 = v;

            // Iterirati dok je v3 različit od nule.
            iteration = 1;
            while (v3 != 0)
            {
                // Dijeliti i oduzimati q, t3 i t1.
                q = u3 / v3;
                t3 = u3 % v3;
                t1 = u1 + q * v1;

                // Zamjeniti varijable za sljedeću iteraciju.
                u1 = v1; v1 = t1; u3 = v3; v3 = t3;
                iteration = -iteration;
            }

            if (u3 != 1) return 0; // Inverz ne postoji.
            else if (iteration < 0) inverse = v - u1;
            else inverse = u1;

            return inverse;
        }
    }
}