using System.Collections.Generic;

namespace Curiosity.Systems.Library
{
    public class Generators
    {
        public static List<string> GenerateNumberList(string txt, int max, int min = 0)
        {
            List<string> lst = new List<string>();
            for (int i = min; i < max + 1; i++)
                lst.Add($"{txt} #{i.ToString()}");
            return lst;
        }
    }
}
