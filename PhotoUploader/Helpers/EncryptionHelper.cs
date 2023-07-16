using System.Security.Cryptography;
using System.Text;

namespace PhotoUploader.Helpers
{
    public class EncryptionHelper
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                return hash;
            }
        }

        public static string ComputePhotoHash(byte[] photoBytes)
        {
            using (var algorithm = SHA256.Create())
            {
                byte[] hashBytes = algorithm.ComputeHash(photoBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                return hash;
            }
        }
    }
}
