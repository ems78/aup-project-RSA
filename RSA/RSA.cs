using System.Numerics;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace AlgoritmiUPrimjeniRSA
{

    // ##################################################################
    // Izvorni kod za RSA algoritam preuzet od korisnika c272 na GitHubu.
    // Repozitorij: https://github.com/c272/SharpRSA.git
    // ##################################################################
    public static class RSA
    {
        public static KeyPair GenerateKeyPair(int bitlenght, ref BigInteger Q_, ref BigInteger P_, ref BigInteger N_, ref BigInteger PHI_, ref BigInteger D_)
        {
            BigInteger q, p, n, phi, d = new();

            // Generirati dva prosta broja
            do
            {
                q = FindPrime(bitlenght / 2);
            } while (q % 0x10001 == 1);

            do
            {
                p = FindPrime(bitlenght / 2);
            } while (p % 0x10001 == 1);

            n = q * p;
            phi = (q - 1) * (p - 1);

            // Izračunati D takav da je DE = 1 mod phi.
            d = Maths.ModularInverse(0x10001, phi);

            // Proslijediti varijable u .razor datoteku.
            P_ = p;
            Q_ = q;
            N_ = n;
            PHI_ = phi;
            D_ = d;

            // Vratiti par ključeva.
            return KeyPair.Generate(n, d);
        }


        // Pronalazi primarni broj zadane duljine bita, koristi se kao n i p u RSA izračunima ključeva.
        public static BigInteger FindPrime(int bitlength)
        {
            if (bitlength % 8 != 0)
            {
                throw new Exception("Neispravna duljina bita za ključ, ne mogu se generirati prosti brojevi."); 
            }

            byte[] randomBytes = new byte[(bitlength / 8) + 1];
            Maths.rand.NextBytes(randomBytes);
            // Postaviti dodatni byte na 0x0 tako da je broj uvijek pozitivan.
            randomBytes[randomBytes.Length - 1] = 0x0;

            // Postaviti donji bit i gornja dva bita broja.
            // To osigurava da je broj neparan i osigurava da je visoki bit od N postavljen.
            Utils.SetBitInByte(0, ref randomBytes[0]);
            Utils.SetBitInByte(7, ref randomBytes[randomBytes.Length - 2]);
            Utils.SetBitInByte(6, ref randomBytes[randomBytes.Length - 2]);

            while (true)
            {
                // Izvršiti Rabin-Miller testa prostosti.
                bool isPrime = Maths.RabinMillerTest(randomBytes, 40);
                if (isPrime) break;
                else
                {
                    Utils.IncrementByteArrayLE(ref randomBytes, 2);
                    var upper_limit = new Byte[randomBytes.Length];

                    // Postavljanje gornjeg bita za unsigned, stvaranje gornje i donje granice.
                    upper_limit[randomBytes.Length - 1] = 0x0;
                    BigInteger upper_limit_bi = new(upper_limit);
                    BigInteger current = new(randomBytes);

                    if (current < upper_limit_bi)
                    {
                        // Nije uspjelo pronaći primarni broj, vraća -1.
                        // Dosegnuta granica bez rješenja.
                        return new BigInteger(-1);
                    }
                }
            }

            // Vraća primarni broj.
            return new BigInteger(randomBytes);
        }


        public static string Encrypt(string text, Key key, ref string byteRAZOR)
        {
            // Pretvoriti tekst u byteove.
            byte[] bytes = Encoding.ASCII.GetBytes(text);

            // Proslijediti konvertirane byteove u .razor
            foreach (var b in bytes)
            {
                byteRAZOR += b.ToString();
            }

            // Enkriptirati byteove.
            byte[] encrypted = EncryptBytes(bytes, key);

            // Pretvaranje enkriptiranih byteova u string.
            return Convert.ToBase64String(encrypted);
        }


        // Enkriptira set byteova sa zadanim ključem.
        public static byte[] EncryptBytes(byte[] bytes, Key public_key)
        {
            // Provjeriti da je veličina byteova manja od n, a veća od 1.
            if (1 > bytes.Length || bytes.Length >= public_key.n.ToByteArray().Length)
            {
                throw new Exception("Bytes given are longer than length of key element n (" + bytes.Length + " bytes).");
            }

            // Dodati padding u array.
            byte[] bytes_padded = new byte[bytes.Length + 2];
            Array.Copy(bytes, bytes_padded, bytes.Length);
            bytes_padded[bytes_padded.Length - 1] = 0x00;

            // Postaviti visoki byte prije podataka, marker byte.
            bytes_padded[bytes_padded.Length - 2] = 0xFF;

            // Izračunati kao BigInteger operaciju enkripcije.
            var cipher_bigint = new BigInteger();
            var padded_bigint = new BigInteger(bytes_padded);
            cipher_bigint = BigInteger.ModPow(padded_bigint, public_key.e, public_key.n);

            // Vraća byte arraya enkriptiranih byteova.
            return cipher_bigint.ToByteArray();
        }


        public static string Decrypt(string text, Key key, ref string byteRAZOR)
        {
            byte[] bytes = Convert.FromBase64String(text);

            foreach (var b in bytes)
            {
                byteRAZOR += b.ToString();
            }
            
            byte[] ciphed = DecryptBytes(bytes, key);
            return Encoding.ASCII.GetString(ciphed);
        }


        // Dekriptira set byteova sa zadanim tajnim ključem.
        public static byte[] DecryptBytes(byte[] bytes, Key private_key)
        {
            // Provjeriti da je privatni ključ legitiman i sadrži d.
            if (private_key.type != KeyType.PRIVATE)
            {
                throw new Exception("Privatni ključ za dekriptiranje klasificiran je kao neprivatni u instanci."); 
            }

            // Dekriptirati.
            var plain_bigint = new BigInteger();
            var padded_bigint = new BigInteger(bytes);
            plain_bigint = BigInteger.ModPow(padded_bigint, private_key.d, private_key.n);

            // Uklanjanje svih padding byteova.
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

            // Provjera neuspjeha u pronalaženju marker bytea.
            if (lengthToCopy == -1)
            {
                throw new Exception("Marker byte za padding (0xFF) nije pronađen u plain byteovima.\nMogući razlozi:\n1: PAYLOAD PREVELIK\n2: NEISPRAVNI KLJUČEVI\n3: NEISPRAVNE ENCRYPT/DECRYPT FUNCKIJE"); 
            }                                                                                               

            // Kopirati u povratni array.
            byte[] return_array = new byte[lengthToCopy];
            Array.Copy(plain_bytes, return_array, lengthToCopy);
            return return_array;
        }
    }
}
