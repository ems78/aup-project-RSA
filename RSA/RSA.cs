using System.Numerics;
using System.Text;

namespace AlgoritmiUPrimjeniRSA
{
    public static class RSA
    {
        public static KeyPair GenerateKeyPair() => GenerateKeyPair(2048);
        public static KeyPair GenerateKeyPair(int bitlenght)
        {
            // Generating two primes, checking if the GCD of (n-1)(p-1) and e is 1.
            BigInteger q, p, n, phi, d = new();

            do
            {
                q = FindPrime(bitlenght / 2);
            } while (q % 0x10001 == 1);

            do
            {
                p = FindPrime(bitlenght / 2);
            } while (p % 0x10001 == 1);

            // Setting n as QP, phi to tortiery function of QP.
            n = q * p;
            phi = (q - 1) * (p - 1);

            // Computing D such that DE = 1 mod phi.
            d = Maths.ModularInverse(0x10001, phi);

            // Returning the key pair.
            return KeyPair.Generate(n, d);
        }


        //Finds a prime of the given bit length, to be used as n and p in RSA key calculations.
        public static BigInteger FindPrime(int bitlength)
        {
            if (bitlength % 8 != 0)
            {
                throw new Exception("Invalid bit length for key given, cannot generate primes.");
            }

            byte[] randomBytes = new byte[(bitlength / 8) + 1];
            Maths.rand.NextBytes(randomBytes);
            // Making the extra byte 0x0 so that the number is always positive.
            randomBytes[randomBytes.Length - 1] = 0x0;

            //Setting the bottom bit and top two bits of the number.
            //This ensures the number is odd, and ensures the high bit of N is set when generating keys.
            Utils.SetBitInByte(0, ref randomBytes[0]);
            Utils.SetBitInByte(7, ref randomBytes[randomBytes.Length - 2]);
            Utils.SetBitInByte(6, ref randomBytes[randomBytes.Length - 2]);

            while (true)
            {
                //Performing a Rabin-Miller primality test.
                bool isPrime = Maths.RabinMillerTest(randomBytes, 40);
                if (isPrime) break;
                else
                {
                    Utils.IncrementByteArrayLE(ref randomBytes, 2);
                    var upper_limit = new Byte[randomBytes.Length];

                    //Clearing upper bit for unsigned, creating upper and lower bounds.
                    upper_limit[randomBytes.Length - 1] = 0x0;
                    BigInteger upper_limit_bi = new(upper_limit);
                    BigInteger lower_limit = upper_limit_bi - 20;
                    BigInteger current = new(randomBytes);

                    if (lower_limit < current && current < upper_limit_bi)
                    {
                        //Failed to find a prime, returning -1.
                        //Reached limit with no solutions.
                        return new BigInteger(-1);
                    }
                }
            }

            //Returning working BigInt.
            return new BigInteger(randomBytes);
        }


        public static string Encrypt(string text, Key key)
        {
            // Converting the text to bytes.
            byte[] bytes = Encoding.ASCII.GetBytes(text);

            // Encrypting the bytes.
            byte[] encrypted = EncryptBytes(bytes, key);

            // Converting the encrypted bytes to a string.
            return Convert.ToBase64String(encrypted);
        }


        // Encrypts a set of bytes when given a public key.
        public static byte[] EncryptBytes(byte[] bytes, Key public_key)
        {
            //Checking that the size of the bytes is less than n, and greater than 1.
            if (1 > bytes.Length || bytes.Length >= public_key.n.ToByteArray().Length)
            {
                throw new Exception("Bytes given are longer than length of key element n (" + bytes.Length + " bytes).");
            }

            //Padding the array to unsign.
            byte[] bytes_padded = new byte[bytes.Length + 2];
            Array.Copy(bytes, bytes_padded, bytes.Length);
            bytes_padded[bytes_padded.Length - 1] = 0x00;

            //Setting high byte right before the data, to prevent data loss.
            bytes_padded[bytes_padded.Length - 2] = 0xFF;

            //Computing as a BigInteger the encryption operation.
            var cipher_bigint = new BigInteger();
            var padded_bigint = new BigInteger(bytes_padded);
            cipher_bigint = BigInteger.ModPow(padded_bigint, public_key.e, public_key.n);

            //Returning the byte array of encrypted bytes.
            return cipher_bigint.ToByteArray();
        }


        public static string Decrypt(string text, Key key)
        {
            byte[] bytes = Convert.FromBase64String(text);
            byte[] ciphed = DecryptBytes(bytes, key);
            return Encoding.ASCII.GetString(ciphed);
        }


        // Deryptsa set of bytes when given a private key.
        public static byte[] DecryptBytes(byte[] bytes, Key private_key)
        {
            //Checking that the private key is legitimate, and contains d.
            if (private_key.type != KeyType.PRIVATE)
            {
                throw new Exception("Private key given for decrypt is classified as non-private in instance.");
            }

            //Decrypting.
            var plain_bigint = new BigInteger();
            var padded_bigint = new BigInteger(bytes);
            plain_bigint = BigInteger.ModPow(padded_bigint, private_key.d, private_key.n);

            //Removing all padding bytes, including the marker 0xFF.
            byte[] plain_bytes = plain_bigint.ToByteArray();
            int lengthToCopy = -1;
            for (int i = plain_bytes.Length - 1; i >= 0; i--)
            {
                if (plain_bytes[i] == 0xFF)
                {
                    lengthToCopy = i;
                    break;
                }
            }

            //Checking for a failure to find marker byte.
            if (lengthToCopy == -1)
            {
                throw new Exception("Marker byte for padding (0xFF) not found in plain bytes.\nPossible Reasons:\n1: PAYLOAD TOO LARGE\n2: KEYS INVALID\n3: ENCRYPT/DECRYPT FUNCTIONS INVALID");
            }

            //Copying into return array, returning.
            byte[] return_array = new byte[lengthToCopy];
            Array.Copy(plain_bytes, return_array, lengthToCopy);
            return return_array;
        }
    }
}
