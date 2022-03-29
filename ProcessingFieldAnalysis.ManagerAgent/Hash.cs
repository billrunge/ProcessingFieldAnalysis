using Relativity.API;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Hash
    {  
        IAPILog Logger { get; set; }

        public Hash (IAPILog logger)
        {
            Logger = logger;
        }
        /// <summary>
        /// Generates a 128 character SHA512 hash for a given input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns>string</returns>
        public string GetHash(string input)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                try
                {
                    byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                } catch (Exception e)
                {
                    Logger.LogError(e, "Failed to generate SHA512 hash for input string: {input}", input);
                }
            }
            return "";
        }
    }
}
