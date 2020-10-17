﻿using System;

namespace Curiosity.Global.Shared.Utils
{
    public static class Utility
    {
        public static readonly Random RANDOM = new Random();
        public static int GenerateIdentifier() => DateTime.Now.GetHashCode();
    }
}
