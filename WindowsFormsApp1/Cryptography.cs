using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Cryptography
    {
        private static TripleDESCryptoServiceProvider clientDESCryptoServiceProvider;

        public static byte[] Key
        {
            get
            {
                return Cryptography.clientDESCryptoServiceProvider.Key;
            }
            set
            {
                Cryptography.clientDESCryptoServiceProvider.Key = value;
            }
        }

        public static byte[] IV
        {
            get
            {
                return Cryptography.clientDESCryptoServiceProvider.IV;
            }
            set
            {
                Cryptography.clientDESCryptoServiceProvider.IV = value;
            }
        }

        [DebuggerNonUserCode]
        public Cryptography()
        {

        }

        static Cryptography()
        {
            Cryptography.clientDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
        }

        public static string EncryptString(string AString)
        {
            MemoryStream memoryStream = null;
            StreamWriter streamWriter = null;
            CryptoStream cryptoStream = null;
            checked
            {
                string result;
                ICryptoTransform transform = Cryptography.clientDESCryptoServiceProvider.CreateEncryptor();
                try
                {
                    memoryStream = new MemoryStream();
                    try
                    {
                        cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                        try
                        {
                            streamWriter = new StreamWriter(cryptoStream);
                            streamWriter.Write(AString);
                            streamWriter.Flush();
                            cryptoStream.FlushFinalBlock();
                            memoryStream.Position = 0L;
                            byte[] array = new byte[(int)memoryStream.Length + 1];
                            memoryStream.Read(array, 0, (int)memoryStream.Length);
                            result = Convert.ToBase64String(array);
                        }
                        finally
                        {
                            streamWriter.Close();
                        }
                    }
                    finally
                    {
                        cryptoStream.Close();
                    }
                }
                finally
                {
                    memoryStream.Close();
                }
                return result;
            }
        }

        public static string DecryptString(string AString)
        {
            string result = string.Empty;
            MemoryStream memoryStream = null;
            CryptoStream cryptoStream = null;
            checked
            {
                byte[] array = Convert.FromBase64String(AString);
                try
                {
                    memoryStream = new MemoryStream();
                    try
                    {
                        ICryptoTransform transform = Cryptography.clientDESCryptoServiceProvider.CreateDecryptor();
                        cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                        cryptoStream.Write(array, 0, array.Length - 1);
                        cryptoStream.FlushFinalBlock();
                        memoryStream.Position = 0L;
                        int num = (int)memoryStream.Length;
                        array = new byte[num - 1 + 1];
                        memoryStream.Read(array, 0, num);
                        int arg_8D_0 = 0;
                        int num2 = num - 1;
                        int num3 = arg_8D_0;
                        string text = string.Empty;
                        while (true)
                        {
                            int arg_B7_0 = num3;
                            int num4 = num2;
                            if (arg_B7_0 > num4)
                            {
                                break;
                            }
                            text += Convert.ToString((Char)array[num3]);
                            num3++;
                        }
                        result = text;
                    }
                    finally
                    {
                        cryptoStream.Close();
                    }
                }
                finally
                {
                    memoryStream.Close();
                }
                return result;
            }
        }
    }
}
