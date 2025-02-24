using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CustomEncryption
{
    public static class Rijndael
    {
        private const int VERSION_SIZE = 1;
        private const int BLOCK_SIZE_SIZE = 1;
        private const int KEY_SIZE_SIZE = 1;
        private const int IV_SIZE = 16;
        private const int HEADER_SIZE = VERSION_SIZE + BLOCK_SIZE_SIZE + KEY_SIZE_SIZE + IV_SIZE;

        public static byte[] Decrypt(byte[] encryptedData, string key)
        {
            try
            {
                // 메타데이터 읽기
                byte version = encryptedData[0];
                byte blockSize = encryptedData[1];
                byte keySize = encryptedData[2];

                // IV 추출
                byte[] iv = new byte[IV_SIZE];
                Buffer.BlockCopy(encryptedData, 3, iv, 0, IV_SIZE);

                // 암호화된 데이터 추출
                byte[] encrypted = new byte[encryptedData.Length - HEADER_SIZE];
                Buffer.BlockCopy(encryptedData, HEADER_SIZE, encrypted, 0, encrypted.Length);

                // 키 해시 생성
                byte[] keyHash;
                using (var sha256 = SHA256.Create())
                {
                    keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                }

                // 복호화
                using (var aes = Aes.Create())
                {
                    aes.Key = keyHash;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        return decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Decryption failed: {ex.Message}");
                return null;
            }
        }

        public static string DecryptToString(byte[] encryptedData, string key)
        {
            byte[] decrypted = Decrypt(encryptedData, key);
            return decrypted != null ? Encoding.UTF8.GetString(decrypted) : null;
        }

        public static string DecryptToString(string encryptedData, string key)
        {
            try
            {
                byte[] encryptedBytes = new byte[encryptedData.Length];
                for (int i = 0; i < encryptedData.Length; i++)
                {
                    encryptedBytes[i] = (byte)encryptedData[i];
                }
                return DecryptToString(encryptedBytes, key);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Decryption failed: {ex.Message}");
                return null;
            }
        }

        public static byte[] Encrypt(byte[] data, string key)
        {
            try
            {
                // 키 해시 생성
                byte[] keyHash;
                using (var sha256 = SHA256.Create())
                {
                    keyHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                }

                // IV 생성
                byte[] iv;
                using (var rng = new RNGCryptoServiceProvider())
                {
                    iv = new byte[IV_SIZE];
                    rng.GetBytes(iv);
                }

                // 암호화
                byte[] encrypted;
                using (var aes = Aes.Create())
                {
                    aes.Key = keyHash;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                    }
                }

                // 메타데이터 추가
                byte version = 0x01;
                byte blockSize = 0x10; // 16바이트
                byte keySize = 0x20;   // 32바이트

                // 최종 버퍼 구성: [버전][블록크기][키크기][IV][암호화된 데이터]
                byte[] result = new byte[HEADER_SIZE + encrypted.Length];
                result[0] = version;
                result[1] = blockSize;
                result[2] = keySize;
                Buffer.BlockCopy(iv, 0, result, 3, IV_SIZE);
                Buffer.BlockCopy(encrypted, 0, result, HEADER_SIZE, encrypted.Length);

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Encryption failed: {ex.Message}");
                return null;
            }
        }

        public static byte[] EncryptString(string data, string key)
        {
            return Encrypt(Encoding.UTF8.GetBytes(data), key);
        }
    }
}