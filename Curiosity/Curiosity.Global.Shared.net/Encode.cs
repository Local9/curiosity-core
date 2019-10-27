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

        public static string Base64ToString(string message)
        {
            return BytesToStringConverted(Convert.FromBase64String(message));
        }

        //public static T ConvertToObject<T>(this string json)
        //{
        //    string decoded = Encode.BytesToStringConverted(System.Convert.FromBase64String(json));
        //    return Newton 
        //}
    }
}
