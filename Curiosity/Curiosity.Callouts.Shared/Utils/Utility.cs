using System;

namespace Curiosity.Callouts.Shared.Utils
{
    public static class Utility
    {
        public static readonly Random RANDOM = new Random();
        public static int GenerateIdentifier() => DateTime.Now.GetHashCode();
    }
}
