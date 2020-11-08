using System;

namespace Curiosity.Systems.Library.Utils
{
    public class Encode
    {
        public static string StringToBase64(string stringToEncode)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stringToEncode));
        }

        public static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new System.IO.MemoryStream(bytes))
            {
                using (var streamReader = new System.IO.StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        public static string Base64ToString(string message)
        {
            try
            {
                return BytesToStringConverted(Convert.FromBase64String(message));
            }
            catch (Exception ex)
            {
                throw new Exception("Error Decoding Message");
            }
        }
    }
}
