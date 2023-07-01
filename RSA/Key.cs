using System.Numerics;

namespace AlgoritmiUPrimjeniRSA
{
    public class Key
    {
        // n, e su varijable javnog ključa.
        public BigInteger n { get; set; }
        public int e = 0x10001;
        
        // d je varijabla privatnog ključa.
        public readonly BigInteger d;

        public KeyType type { get; set; }

        public Key(BigInteger n_, KeyType type_, BigInteger d_)
        {
            // Uhvatiti edge caseove za neispravan unos.
            if (type_ == KeyType.PRIVATE && d_ < 2) { throw new Exception("Neispravna vrijednost d za privatni ključ."); }  

            n = n_;
            type = type_;
            d = d_;
        }

        public Key(BigInteger n_, KeyType type_)
        {
            // Uhvatiti edge caseove za neispravan unos.
            if (type_ == KeyType.PRIVATE) { throw new Exception("Nije pružena vrijednost d za privatni ključ."); } 

            n = n_;
            type = type_;
            d = 0;
        }
    }

    public sealed class KeyPair
    {
        public readonly Key privateKey;
        public readonly Key publicKey;

        public KeyPair(Key publicKey_, Key privateKey_)
        {
            privateKey = privateKey_;
            publicKey = publicKey_;
        }   

        public static KeyPair Generate(BigInteger n, BigInteger d)
        {
            Key publicKey = new(n, KeyType.PUBLIC);
            Key privateKey = new(n, KeyType.PRIVATE, d);
            return new KeyPair(publicKey, privateKey);
        }
        
    }

    public enum KeyType
    {
        PUBLIC,
        PRIVATE
    }
}