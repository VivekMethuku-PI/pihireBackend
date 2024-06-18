using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiHire.BAL.Common
{
    public class Hashification
    {
        /// <summary>
        /// Perform Hashification using MD5
        /// </summary>
        /// <param name="data">Plan text</param>
        /// <returns></returns>
        public static string MD5(string data)
        {
            using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var md5data = md5.ComputeHash(Encoding.ASCII.GetBytes(data));
                return Convert.ToBase64String(md5data);// Encoding.ASCII.GetString(md5data);
            }
        }
        /// <summary>
        /// Perform Hashification using SHA 256
        /// </summary>
        /// <param name="data">Plan text</param>
        /// <returns></returns>
        public static string SHA(string data)
        {
            using (var sha = new System.Security.Cryptography.SHA256CryptoServiceProvider())
            {
                var shadata = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
                return Convert.ToBase64String(shadata);// Encoding.ASCII.GetString(shadata);
            }
        }
    }
}
