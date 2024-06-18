using PiHire.BAL.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PiHire.BAL.Common
{
    public class SimpleEncrypt
    {
        public string passwordEncrypt(string plainText)
        {
            return Crypt(plainText);
        }
        public string passwordDecrypt(string encryptedText)
        {
            return Dcrypt(encryptedText);
        }

        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public string Crypt(string text)
        {
            try
            {
                return AESCrypt(text, Types.AppConstants.AES128Key);
            }
            catch (Exception)
            {
                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
                byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Convert.ToBase64String(outputBuffer);
            }
        }
        /// <summary>
        /// 128-bit encryption key size is 16 bytes. 
        /// 192-bit encryption key is 24 bytes. 
        /// 256-bit encryption key size is 32 bytes
        /// </summary>
        /// <param name="textData"></param>
        /// <param name="Encryptionkey"></param>
        /// <returns></returns>
        public string AESCrypt(string textData, string Encryptionkey)
        {
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            //set the initialization vector (IV) for the symmetric algorithm
            byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            Aes sy = Aes.Create();
            ICryptoTransform objtransform = sy.CreateEncryptor(passBytes, EncryptionkeyBytes);

            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
            //Final transform the test string.
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }

        public string Dcrypt(string text)
        {
            try
            {
                return AESDcrypt(text, Types.AppConstants.AES128Key);
            }
            catch (Exception)
            {
                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Encoding.Unicode.GetString(outputBuffer);
            }
        }

        string AESDcrypt(string EncryptedText, string Encryptionkey)
        {
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            //set the initialization vector (IV) for the symmetric algorithm
            byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            Aes sy = Aes.Create();
            ICryptoTransform objtransform = sy.CreateDecryptor(passBytes, EncryptionkeyBytes);

            byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
            byte[] TextByte = objtransform.TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(TextByte);  //it will return readable string
        }
    }
}
