﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class ListExtension
    {
        public static List<T> Slice<T>(this List<T> inputList, int startIndex, int endIndex)
        {
            return inputList.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
        }
    }
}
