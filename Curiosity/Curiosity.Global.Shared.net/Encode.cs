using System;

namespace Curiosity.Global.Shared.net
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

    }
}
