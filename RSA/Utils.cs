using System.Numerics;

namespace AlgoritmiUPrimjeniRSA
{
    class Utils
    {
        /// <summary>
        /// Postavlja bit u prosljeđenom ref byteu, koristeći indeks od 0-7 s desna.
        /// </summary>
        /// <param name="bitNumFromRight">Indeks bita od desne strane bytea.</param>
        /// <param name="toSet">Ref byte za postavljanje.</param>
        public static void SetBitInByte(int bitNumFromRight, ref byte toSet)
        {
            byte mask = (byte)(1 << bitNumFromRight);
            toSet |= mask;
        }

        /// <summary>
        /// Inkrementira byte array kao cjelinu, za zadanu količinu. (little endian).
        /// </summary>
        public static void IncrementByteArrayLE(ref byte[] randomBytes, int amt)
        {
            BigInteger n = new(randomBytes);
            n += amt;
            randomBytes = n.ToByteArray();
        }

        /// <summary>
        /// Dekrementira byte array kao cjelinu, za zadanu količinu. (little endian).
        /// </summary>
        public static void DecrementByteArrayLE(ref byte[] randomBytes, int amt)
        {
            BigInteger n = new(randomBytes);
            n -= amt;
            randomBytes = n.ToByteArray();
        }
    }
}
