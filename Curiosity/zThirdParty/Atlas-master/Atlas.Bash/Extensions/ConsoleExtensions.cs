using System;

namespace Atlas.Bash.Extensions
{
    public class ConsoleExtensions
    {
        public static string Prompt(string message, object standard)
        {
            Console.Write($"{message} ({standard}): ");

            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input)) input = standard.ToString();

            return input;
        }
    }
}