using System.Security.Cryptography;

namespace Authentication.Core.Security
{
    public static class PasswordHasher
	{
        private const int SaltSize = 16;

        private const int HashSize = 20;

        public static string Hash(string password, int iterations)
        {
            // Utworzenie soli
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            // Utworzenie sygnatury hasła tzw. hash'u
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            // Mieszanie soli i hash'u
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Zamiana do base64
            var base64Hash = Convert.ToBase64String(hashBytes);

            // Umieszczenie dodatkowo informacji o liczbie iteracji
            return string.Format("{0}${1}", iterations, base64Hash);
        }

        public static string Hash(string password)
        {
            return Hash(password, 10000);
        }

        public static bool Verify(string password, string hashedPassword)
        {
            // Wyodrębnienie zakodowanego hash'a oraz liczby iteracji
            var splittedHashString = hashedPassword.Split('$');
            if (!int.TryParse(splittedHashString[0], out int iterations))
                throw new NotSupportedException("Failed to extract the " +
                    "number of iterations from the password signature.");

            var base64Hash = splittedHashString[1];

            // Zamiana Base64 na tablicę bajtów
            var hashBytes = Convert.FromBase64String(base64Hash);

            // Pobranie wielkości soli
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Utworzenie hash'u dla podanego ciągu znaków
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Porównanie obu sygnatur znak po znaku
            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }
    }
}

