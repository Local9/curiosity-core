namespace Atlas.Bash.Extensions
{
    public class IntegerExtensions
    {
        public static int Parse(string value, int standard)
        {
            return int.TryParse(value, out var result) ? result : standard;
        }
    }
}